using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public interface IMETPlayerActorTake : IMessageExchangeTarget
{
    void OnTake(PlayerParamData signpostParamData, int maxDurability, int durability);
}

public class PlayerActorTake : MonoBehaviour,
    IMETPlayerSensorItemSignpost,
    IMETPlayerSMBIdle,
    IMETPlayerSMBTake,
    IMETPlayerAnimationEventProxyOnTake
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private StudioEventEmitter audioTake;

    private FSM fsm = new FSM();
    private List<ItemSignpost> itemSignpostsNearby = new List<ItemSignpost>();
    private ItemSignpost itemSignpostInteracting = null;

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

    void IMETPlayerSensorItemSignpost.OnEnter(Collider other)
    {
        var itemSignpost = other.gameObject.GetComponent<ItemSignpost>();
        if (itemSignpost != null &&
            // NOTE: When the signpostInteracting is stuttering, 
            // the interacting signpost can be found as new one.
            itemSignpost != itemSignpostInteracting)
        {
            itemSignpostsNearby.Add(itemSignpost);
        }
    }

    void IMETPlayerSensorItemSignpost.OnExit(Collider other)
    {
        var itemSignpost = other.gameObject.GetComponent<ItemSignpost>();
        itemSignpostsNearby.Remove(itemSignpost);
    }

    void IMETPlayerSMBIdle.OnEnterSolo()
    {
        fsm.TransitionTo<StateReady>();
    }

    void IMETPlayerSMBIdle.OnExitSolo()
    {
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerSMBTake.OnEnter()
    {
        fsm.TransitionTo<StateTaking>();
    }

    void IMETPlayerSMBTake.OnExit()
    {
        // NOTE: Make sure there is no itemSignpostInteracting;
        // This is important because when getting hit while taking before OnTake is called,
        // itemSignpostInteracting will remain and prevent the last one from being taken.
        itemSignpostInteracting = null;
        fsm.TransitionTo<StateWait>();
    }

    void IMETPlayerAnimationEventProxyOnTake.OnTake()
    {
        if (itemSignpostInteracting != null)
        {
            messageExchange.Invoke<IMETPlayerActorTake>(t =>
                t.OnTake(
                    itemSignpostInteracting.SignpostParamData,
                    itemSignpostInteracting.MaxDurability,
                    itemSignpostInteracting.Durability));
            itemSignpostInteracting.OnClaim();
            itemSignpostsNearby.Remove(itemSignpostInteracting);
            Destroy(itemSignpostInteracting.gameObject);
            itemSignpostInteracting = null;
        }
        audioTake.Play();
        playerAnimatorDriver.SetIsHoldingSignpost(true);
    }

    class FSM : FiniteStateMachine<PlayerActorTake, State> { }
    class State : FiniteStateMachineState<PlayerActorTake> { }

    class StateWait : State { }

    class StateReady : State
    {
        public override void OnEnter()
        {
            // NOTE: Make sure there is no itemSignpostInteracting;
            C.itemSignpostInteracting = null;
        }

        public override void OnUpdate()
        {
            if (C.itemSignpostsNearby.Count > 0 && C.playerControl.ButtonTake)
            {
                C.itemSignpostInteracting = PickClosest();
                if (C.itemSignpostInteracting == null)
                {
                    // NOTE: Unexpected. Skip if it happens.
                    return;
                }
                C.playerAnimatorDriver.TriggerTake();
                C.fsm.TransitionTo<StateWait>();
            }
        }

        private ItemSignpost PickClosest()
        {
            Vector3 position = C.locomotor.transform.position;
            ItemSignpost target = null;
            float sqrMagMin = Mathf.Infinity;
            int n = C.itemSignpostsNearby.Count;
            for (int i = n - 1; i >= 0; i--)
            {
                var itemSignpost = C.itemSignpostsNearby[i];
                if (itemSignpost == null)
                {
                    // NOTE: Unexpected. Skip if it happens.
                    continue;
                }
                float sqrMag = (itemSignpost.HUDRingPosition - position).sqrMagnitude;
                if (sqrMag < sqrMagMin)
                {
                    target = itemSignpost;
                    sqrMagMin = sqrMag;
                }
            }
            return target;
        }
    }

    class StateTaking : State
    {
        Quaternion targetRotation;

        public override void OnEnter()
        {
            var target = C.itemSignpostInteracting.HUDRingPosition;
            targetRotation = C.locomotor.RotationTowardTarget(target);
            float brake = C.playerParam.TakeBrake;
            C.locomotor.Brake(brake);
        }

        public override void OnFixedUpdate()
        {
            var rotationSpeed = C.playerParam.MoveRotationSpeed;
            C.locomotor.RotateTowardTarget(targetRotation, rotationSpeed);
        }
    }
}
