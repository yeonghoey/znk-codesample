using System;
using UnityEngine;
using FMODUnity;

public interface IMETPlayerActorFreeFall : IMessageExchangeTarget
{
    void OnChangeLedgeState(bool isOnLedge, Vector3 normal);
}

public class PlayerActorFreeFall : MonoBehaviour,
    IMETPlayerSMBFreeFall
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private GroundChecker groundChecker;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private StudioEventEmitter audioLand;

    private static readonly Action<IMETPlayerActorFreeFall, bool, Vector3> callOnChangeLedgeState =
        (t, isOnLedgeThis, ledgeNormal) => t.OnChangeLedgeState(isOnLedgeThis, ledgeNormal);

    private const float groundToAirPeriod = 0.35f;
    private const float airToGroundPeriod = 0.2f;
    private float transitionPeriod => isOnGround ? groundToAirPeriod : airToGroundPeriod;

    // If the position is not changed at least stuckEpsilon for period,
    // it's considered stuck and forced to be as grounded.
    private const float stuckPeriod = 0.2f;
    private const float sqrStuckEpsilon = 0.01f * 0.01f;

    private bool isOnGround;
    private bool isFreeFalling;
    private Vector3 prevPosition;
    private Cooldown transitionCooldown;
    private Cooldown stuckCooldown;

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void Start()
    {
        isOnGround = true;
        isFreeFalling = false;
        prevPosition = locomotor.Position;
        transitionCooldown = new Cooldown();
        transitionCooldown.Set(transitionPeriod, isReadyInitially: true);
        stuckCooldown = new Cooldown();
        stuckCooldown.Set(stuckPeriod, isReadyInitially: false);
    }

    void FixedUpdate()
    {
        if (isFreeFalling && playerControl.WorldDirMove != Vector3.zero)
        {
            var worldDir = playerControl.WorldDirMove;
            var targetRotation = locomotor.RotationToward(worldDir);
            var rotationSpeed = playerParam.MoveRotationSpeed;
            locomotor.RotateTowardTarget(targetRotation, rotationSpeed);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        Vector3 ledgeNormal;
        var (isOnGroundThis, isOnLedgeThis) = groundChecker.Check(out ledgeNormal);

        // Stuck prevention
        stuckCooldown.Tick(dt);
        var currentPosition = locomotor.Position;
        float sqrDiff = (currentPosition - prevPosition).sqrMagnitude;
        prevPosition = currentPosition;
        if (sqrDiff > sqrStuckEpsilon)
        {
            stuckCooldown.Reset(isReady: false);
        }
        if (stuckCooldown.IsReady)
        {
            isOnGroundThis = true;
        }

        // NOTE: Give some transition padding on isOnGround state;
        transitionCooldown.Tick(dt);
        if (isOnGroundThis == isOnGround)
        {
            transitionCooldown.Set(transitionPeriod, isReadyInitially: false);
        }
        if (transitionCooldown.Claim())
        {
            this.isOnGround = isOnGroundThis;
            playerAnimatorDriver.IsOnGround = isOnGround;
            locomotor.IsOnGround = isOnGround;
        }

        // NOTE: On the other hand, ledge state should always be updated.
        messageExchange.Invoke(callOnChangeLedgeState, isOnLedgeThis, ledgeNormal);
    }

    void IMETPlayerSMBFreeFall.OnEnterSolo()
    {
        isFreeFalling = true;
    }

    void IMETPlayerSMBFreeFall.OnExitSolo()
    {
        isFreeFalling = false;
        audioLand.Play();
    }
}