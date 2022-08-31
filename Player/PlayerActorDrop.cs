using UnityEngine;
using FMODUnity;

public interface IMETPlayerActorDrop : IMessageExchangeTarget
{
    void OnDrop();
}

public class PlayerActorDrop : MonoBehaviour,
    IMETPlayerSMBIdle,
    IMETPlayerSMBDrop,
    IMETPlayerAnimationEventProxyOnDrop
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private StudioEventEmitter audioDrop;

    private FSM fsm = new FSM();

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

    void Update()
    {
        fsm.Update();
    }

    void IMETPlayerSMBIdle.OnEnterSolo()
    {
        fsm.TransitionTo<StateReady>();
    }

    void IMETPlayerSMBIdle.OnExitSolo()
    {
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerSMBDrop.OnEnter()
    {
        fsm.TransitionTo<StateDropping>();
    }

    void IMETPlayerSMBDrop.OnExit()
    {
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerAnimationEventProxyOnDrop.OnDrop()
    {
        messageExchange.Invoke<IMETPlayerActorDrop>(t => t.OnDrop());
        playerAnimatorDriver.SetIsHoldingSignpost(false);
        audioDrop.Play();
    }

    class FSM : FiniteStateMachine<PlayerActorDrop, State> { }

    class State : FiniteStateMachineState<PlayerActorDrop> { }

    class StateWait : State { }

    class StateReady : State
    {
        public override void OnUpdate()
        {
            if (C.playerParam.IsHoldingSignpost && C.playerControl.ButtonDrop)
            {
                C.playerAnimatorDriver.TriggerDrop();
                C.fsm.TransitionTo<StateWait>();
            }
        }
    }

    class StateDropping : State
    {
        public override void OnEnter()
        {
            float brake = C.playerParam.DropBrake;
            C.locomotor.Brake(brake);
        }
    }
}
