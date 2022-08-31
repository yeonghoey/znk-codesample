using UnityEngine;
using FMODUnity;

public class PlayerActorRoll : MonoBehaviour,
    IMETPlayerSMBIdle,
    IMETPlayerSMBRoll
{
    [SerializeField] private CapsuleCollider mainCollider;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private StudioEventEmitter studioEventEmitter;

    // NOTE: This should include the rolling time ( which is about 0.4s)
    private const float cooldownDuration = 0.8f;

    public bool IsRolling => fsm.Current == fsm.InstanceOf<StateRolling>();

    private FSM fsm = new FSM();
    private Cooldown cooldown = new Cooldown();
    private float radiusOriginal;

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
        radiusOriginal = mainCollider.radius;
        cooldown.Set(cooldownDuration, isReadyInitially: true);
        fsm.Init<StateWait>(this);
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        cooldown.Tick(dt);
        fsm.Update();
    }

    /// IMETPlayerSMBIdle
    void IMETPlayerSMBIdle.OnEnterSolo()
    {
        fsm.TransitionTo<StateReady>();
    }

    void IMETPlayerSMBIdle.OnExitSolo()
    {
        fsm.TransitionTo<StateWait>();
    }

    // IMETPlayerSMBRoll
    void IMETPlayerSMBRoll.OnEnter()
    {
        mainCollider.radius = radiusOriginal * playerParam.RollingRadiusMultiplier;
        fsm.TransitionTo<StateRolling>();
    }

    void IMETPlayerSMBRoll.OnEnterSolo() { }

    void IMETPlayerSMBRoll.OnExitSolo()
    {
        mainCollider.radius = radiusOriginal;
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerSMBRoll.OnExit() { }

    class FSM : FiniteStateMachine<PlayerActorRoll, State> { }

    class State : FiniteStateMachineState<PlayerActorRoll> { }

    class StateWait : State { }

    class StateReady : State
    {
        public override void OnUpdate()
        {
            if (C.playerControl.ButtonRoll && C.cooldown.Claim())
            {
                C.playerAnimatorDriver.TriggerRoll();
                C.fsm.TransitionTo<StateWait>();
            }
        }
    }

    class StateRolling : State
    {
        Vector3 worldDir;
        Quaternion targetRotation;
        float rotationSpeed;

        public override void OnEnter()
        {
            worldDir = C.playerControl.WorldDirMove.ToWorldDir();
            if (worldDir == Vector3.zero)
            {
                worldDir = C.locomotor.WorldDir;
            }
            targetRotation = C.locomotor.RotationToward(worldDir);
            rotationSpeed = C.playerParam.RollingRotationSpeed;
            C.locomotor.Brake(C.playerParam.RollBrake);
            // NOTE: Soar to make the player move between ledges smoothly.
            C.locomotor.Push(Vector3.up, C.playerParam.RollingSoarSpeed);
            C.locomotor.Push(worldDir, C.playerParam.RollingSpeed);
            C.studioEventEmitter.Play();
        }

        public override void OnFixedUpdate()
        {
            C.locomotor.RotateTowardTarget(targetRotation, rotationSpeed);
        }
    }
}