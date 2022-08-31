using UnityEngine;

public class PlayerActorIdle : MonoBehaviour,
    IMETPlayerSMBIdle,
    IMETPlayerActorFreeFall
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private CapsuleCollider envCollider;
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float moveCorrectionDistance;

    private FSM fsm = new FSM();
    private bool isOnLedge;
    private Vector3 ledgeWorldDir;

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
        isOnLedge = false;
        ledgeWorldDir = Vector3.zero;
    }

    void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    void Update()
    {
        fsm.Update();
    }

    void IMETPlayerSMBIdle.OnEnterSolo()
    {
        fsm.TransitionTo<StateMoving>();
    }

    void IMETPlayerSMBIdle.OnExitSolo()
    {
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerActorFreeFall.OnChangeLedgeState(bool isOnLedge, Vector3 normal)
    {
        this.isOnLedge = isOnLedge;
        this.ledgeWorldDir = normal.WithY(0f).normalized;
    }

    class FSM : FiniteStateMachine<PlayerActorIdle, State> { }

    class State : FiniteStateMachineState<PlayerActorIdle> { }

    class StateWait : State { }

    class StateMoving : State
    {
        private Vector3 worldDirMove;

        public override void OnEnter()
        {
            C.playerAnimatorDriver.ResetActionTriggers();
            C.locomotor.Mass = C.playerParam.NormalMass;
        }

        public override void OnFixedUpdate()
        {
            worldDirMove = C.playerControl.WorldDirMove;
            C.locomotor.CorrectWorldDirMove(ref worldDirMove,
                C.envCollider,
                C.moveCorrectionDistance,
                C.wallLayerMask);
            Rotate();
            Accelerate();
        }

        private void Rotate()
        {
            if (worldDirMove == Vector3.zero)
            {
                return;
            }

            float rotationSpeed = C.playerParam.MoveRotationSpeed;
            C.locomotor.RotateTowardMovingDirection(rotationSpeed);
        }

        private void Accelerate()
        {
            float accel = C.playerParam.MoveAcceleration;
            C.locomotor.Accelerate(worldDirMove, accel);

            // Ledge
            if (C.isOnLedge)
            {
                bool isTowardLedge = Vector3.Dot(C.ledgeWorldDir, worldDirMove) < 0f;
                if (isTowardLedge)
                {
                    C.locomotor.Accelerate(Vector3.up, C.playerParam.LedgeSoarAccel);
                }
            }
        }
    }
}
