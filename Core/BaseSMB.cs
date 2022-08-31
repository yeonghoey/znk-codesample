using UnityEngine;

// A StateMachineBehaviour with subdivided callback methods
public class BaseSMB : StateMachineBehaviour
{
    public virtual void OnEnter() { }
    public virtual void OnUpdateEntering() { }
    public virtual void OnEnterSolo() { }
    public virtual void OnUpdate() { }
    public virtual void OnUpdateSolo() { }
    public virtual void OnExitSolo() { }
    public virtual void OnUpdateExiting() { }
    public virtual void OnExit() { }

    protected Animator LastAnimator { get; private set; }
    protected AnimatorStateInfo LastStateInfo { get; private set; }
    protected int LastLayerIndex { get; private set; }

    class TransitionState
    {
        public float lastNormalizedTime = 0f;
        public bool soloEntered = false;
        public bool soloExited = false;
    }

    // NOTE: There can be up to two transition state 
    // because two transition of the same SMB can exist when transitioning to self;
    private TransitionState[] tsBuffer = new TransitionState[2] { new TransitionState(), new TransitionState() };
    private int tsCount = 0;

    public sealed override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (tsCount > 1)
        {
            // NOTE: Unexpected; Ignore it.
            return;
        }

        LastAnimator = animator;
        LastStateInfo = stateInfo;
        LastLayerIndex = layerIndex;

        var ts = tsBuffer[tsCount];
        ts.lastNormalizedTime = stateInfo.normalizedTime;
        ts.soloEntered = false;
        ts.soloExited = false;
        tsCount += 1;
        OnEnter();
    }

    public sealed override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        TransitionState current = null;
        float normalizedTime = stateInfo.normalizedTime;
        for (int i = 0; i < tsCount; i++)
        {
            var ts = tsBuffer[i];
            if (normalizedTime >= ts.lastNormalizedTime)
            {
                current = ts;
                break;
            }
        }

        if (current == null)
        {
            // NOTE: Unexpected; Ignore it.
            return;
        }

        LastAnimator = animator;
        LastStateInfo = stateInfo;
        LastLayerIndex = layerIndex;

        OnUpdate();

        current.lastNormalizedTime = normalizedTime;

        if (!animator.IsInTransition(layerIndex))
        {
            if (!current.soloEntered)
            {
                OnEnterSolo();
                current.soloEntered = true;
            }
            OnUpdateSolo();
        }

        if (animator.IsInTransition(layerIndex))
        {
            if (!current.soloEntered)
            {
                OnUpdateEntering();
            }

            if (current.soloEntered && !current.soloExited)
            {
                OnExitSolo();
                current.soloExited = true;
            }

            if (current.soloEntered && current.soloExited)
            {
                OnUpdateExiting();
            }
        }
    }

    public sealed override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (tsCount < 1)
        {
            // NOTE: This can happen when using animator.Play() on Start().
            // Ignore it.
            return;
        }

        LastAnimator = animator;
        LastStateInfo = stateInfo;
        LastLayerIndex = layerIndex;

        var current = tsBuffer[0];
        if (!current.soloEntered)
        {
            OnEnterSolo();
        }
        if (!current.soloExited)
        {
            OnExitSolo();
        }

        var temp = tsBuffer[0];
        tsBuffer[0] = tsBuffer[1];
        tsBuffer[1] = temp;

        tsCount -= 1;

        OnExit();
    }
}