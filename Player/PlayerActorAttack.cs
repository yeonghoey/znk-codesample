using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using FMODUnity;

public interface IPlayerActorAttackVictim
{
    void OnAttacked(IPlayerActorAttackInfo attackInfo);
    void OnHitstop(float duration);
}

public interface IPlayerActorAttackInfo
{
    GameObject AttackerGO { get; }
    Vector3 AttackedFrom { get; }
    Vector3 WorldDirAttack { get; }
    float KnockbackSpeed { get; }
    int Damage { get; }
}

public class PlayerActorAttackInfo : IPlayerActorAttackInfo
{
    public GameObject attackerGO;
    public Vector3 attackedFrom;
    public Vector3 worldDirAttack;
    public float knockbackSpeed;
    public int damage;

    GameObject IPlayerActorAttackInfo.AttackerGO => attackerGO;
    Vector3 IPlayerActorAttackInfo.AttackedFrom => attackedFrom;
    Vector3 IPlayerActorAttackInfo.WorldDirAttack => worldDirAttack;
    float IPlayerActorAttackInfo.KnockbackSpeed => knockbackSpeed;
    int IPlayerActorAttackInfo.Damage => damage;
}

public class PlayerActorAttack : MonoBehaviour,
    IMETPlayerSMBIdle,
    IMETPlayerSMBRoll,
    IMETPlayerSMBAttack,
    IMETPlayerAnimationEventProxyOnHit
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private WallChecker wallChecker;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerSignpost playerSignpost;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private CinemachineImpulseSource impulseHit;
    [SerializeField] private StudioEventEmitter audioUnarmed;
    [SerializeField] private StudioEventEmitter audioSignpost;

    private const float angleDiffForAttackRotatingDirection = 90f;
    private const float impulsePerHit = 0.1f;
    private const float impulseMin = 0.2f;
    private const float impulseMax = 1.2f;

    private FSM fsm = new FSM();
    private PlayerActorAttackInfo attackInfo = new PlayerActorAttackInfo();
    private GameObject playerGO;
    private bool lastIsClockwise = false;

    // NOTE: See RefreshCurrentWorldDirAttack() for how these variables work
    private float sqrMagInputDirAttack;
    private Vector3 currentWorldDirAttack;

    private List<IPlayerActorAttackVictim> currentVictims;

    void Awake()
    {
        playerGO = GetComponentInParent<Player>().gameObject;
        currentVictims = new List<IPlayerActorAttackVictim>();
    }

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
        fsm.Init<StateWait>(this);
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    void Update()
    {
        fsm.Update();
    }

    void OnDrawGizmosSelected()
    {
        playerParam.AttackBoxChecker.DrawGizmo(transform.localToWorldMatrix);
    }

    // IMETPlayerSMBIdle
    void IMETPlayerSMBIdle.OnEnterSolo()
    {
        fsm.TransitionTo<StateReady>();
    }

    void IMETPlayerSMBIdle.OnExitSolo()
    {
        fsm.TransitionTo<StateWait>();
    }

    // IMETPlayerSMBRoll
    void IMETPlayerSMBRoll.OnEnter() { }

    void IMETPlayerSMBRoll.OnEnterSolo()
    {
        fsm.TransitionTo<StateReady>();
    }

    void IMETPlayerSMBRoll.OnExitSolo()
    {
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerSMBRoll.OnExit() { }

    // IMETPlayerSMBAttack
    void IMETPlayerSMBAttack.OnEnter()
    {
        fsm.TransitionTo<StatePreAttacking>();
    }

    void IMETPlayerSMBAttack.OnEnterSolo()
    {
        fsm.TransitionTo<StateAttacking>();
    }

    void IMETPlayerSMBAttack.OnExitSolo()
    {
        // NOTE: It's expected to already be in StateWait.
        // For consecutive punch attack, make it ready at this point.
        fsm.TransitionTo<StateReady>();
    }

    void IMETPlayerSMBAttack.OnExit() { }

    // IMETPlayerAnimationEventProxyOnHit
    void IMETPlayerAnimationEventProxyOnHit.OnHit()
    {
        // NOTE: Put it to StateWait, in other words, stop rotating.
        fsm.TransitionTo<StateWait>();

        var colliders = playerParam.AttackBoxChecker.Check(transform.position, transform.rotation);
        attackInfo.attackerGO = playerGO;
        attackInfo.damage = playerParam.AttackDamage;
        attackInfo.attackedFrom = transform.position;
        attackInfo.worldDirAttack = locomotor.WorldDir;
        attackInfo.knockbackSpeed = playerParam.AttakKnockbackSpeed;
        currentVictims.Clear();
        foreach (Collider other in colliders)
        {
            var victim = other.GetComponent<IPlayerActorAttackVictim>();
            if (victim != null && !wallChecker.Check(other))
            {
                victim.OnAttacked(attackInfo);
                currentVictims.Add(victim);
            }
        }

        if (currentVictims.Count > 0)
        {
            float force = Mathf.Clamp(
                impulsePerHit * currentVictims.Count,
                impulseMin,
                impulseMax);
            impulseHit.GenerateImpulseWithForce(force);
            playerSignpost.Claim(currentVictims.Count);
        }
    }

    void IMETPlayerAnimationEventProxyOnHit.OnImpact()
    {
        float duration = playerParam.AttackHitstopDuration;
        if (currentVictims.Count > 0)
        {
            playerAnimatorDriver.SetPause(duration);
        }
        foreach (var victim in currentVictims)
        {
            victim.OnHitstop(duration);
        }
        currentVictims.Clear();
    }

    private bool RefreshCurrentWorldDirAttack()
    {
        // NOTE: Update the attacking direction when only the input magnitude is increasing;
        // There are two cases for this:
        // 1) R-stick on gamepad: 
        //  The input is continuous.
        //  Player intention of attack direction would be only valid when 
        //  tilting the stick, in other words, when the magnitude is increasing
        // 2) Keyboard
        //  For the keyboard input, it's intentionally not normalized, so that
        //  the magnitude of digonal is greater than that of one direction,
        //  so that the dignal input take priority.
        float currentSqrMagInputDirAttack = playerControl.SqrMagInputDirAttack;
        if (currentSqrMagInputDirAttack >= sqrMagInputDirAttack)
        {
            sqrMagInputDirAttack = currentSqrMagInputDirAttack;
            currentWorldDirAttack = playerControl.WorldDirAttack;
            return true;
        }
        return false;
    }

    class FSM : FiniteStateMachine<PlayerActorAttack, State> { }
    class State : FiniteStateMachineState<PlayerActorAttack> { }

    class StateWait : State { }

    class StateReady : State
    {
        public override void OnUpdate()
        {
            if (C.playerControl.InputDirAttack != Vector2.zero)
            {
                C.sqrMagInputDirAttack = 0f;
                C.currentWorldDirAttack = Vector3.zero;
                C.RefreshCurrentWorldDirAttack();
                C.playerAnimatorDriver.TriggerAttack();
                C.fsm.TransitionTo<StateWait>();
            }
        }
    }

    // NOTE: This is the base class for PreAttacking and Attacking
    abstract class StateAttackingBase : State
    {
        protected Quaternion targetRotation;
        protected float rotationSpeed;

        public override void OnEnter()
        {
            C.RefreshCurrentWorldDirAttack();
            Vector3 worldDir = C.currentWorldDirAttack;
            targetRotation = C.locomotor.RotationToward(worldDir);
            rotationSpeed = C.playerParam.AttackingRotationSpeed;
            OnEnterAfterInitRotation();
        }

        protected abstract void OnEnterAfterInitRotation();

        public sealed override void OnFixedUpdate()
        {
            C.locomotor.RotateTowardTarget(targetRotation, rotationSpeed);
        }
    }

    // NOTE: PreAttacking is expected to be running between "Enter ~ EnterSolo
    // This state is for a room to commit the actual attack direction while
    // giving some feedback of attacking by playing sound and rotationing to
    // the candidate direction;
    class StatePreAttacking : StateAttackingBase
    {
        public override void OnUpdate()
        {
            // NOTE: In this PreAttacking stage,
            // the target rotation should be kept updated.
            if (C.RefreshCurrentWorldDirAttack())
            {
                targetRotation = C.locomotor.RotationToward(C.currentWorldDirAttack);
            }
        }

        protected override void OnEnterAfterInitRotation()
        {
            SetAttackRotation();
            PlayAudio();
        }

        private void SetAttackRotation()
        {
            bool isClockwise;
            if (C.locomotor.IsLookingTarget(targetRotation, angleDiffForAttackRotatingDirection))
            {
                isClockwise = !C.lastIsClockwise;
            }
            else
            {
                isClockwise = C.locomotor.IsClockwise(targetRotation);
            }
            C.lastIsClockwise = isClockwise;
            C.playerAnimatorDriver.SetAttackRotationDir(isClockwise);
        }

        private void PlayAudio()
        {
            var audio = C.playerParam.IsHoldingSignpost ? C.audioSignpost : C.audioUnarmed;
            audio.Play();
        }
    }

    // NOTE: From this point, attack direction is committed,
    // and push the character to the attack direction;
    class StateAttacking : StateAttackingBase
    {
        protected override void OnEnterAfterInitRotation()
        {
            ApplyMovement(C.currentWorldDirAttack);
        }

        private void ApplyMovement(Vector3 worldDir)
        {
            var lastSpeed = C.locomotor.Speed;
            C.locomotor.Brake(C.playerParam.AttackBrake);
            // NOTE: Keep momentum;
            var speed = C.playerParam.AttackPushSpeed + (lastSpeed * (1 - C.playerParam.AttackBrake));
            C.locomotor.Push(worldDir, speed);
        }
    }
}