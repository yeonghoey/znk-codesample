using System;
using System.Collections.Generic;

public class FiniteStateMachineState<TContext>
{
    public TContext C { get; private set; }

    public void Init(TContext context)
    {
        this.C = context;
    }

    public virtual void OnCreate() { }
    public virtual void OnEnter() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnExit() { }
}

public class FiniteStateMachine<TContext, TState>
    where TState : FiniteStateMachineState<TContext>
{
    public TContext Context { get; private set; }
    public TState Current { get; private set; }

    private Dictionary<Type, TState> instanceMap = new Dictionary<Type, TState>();

    public void Init<S>(TContext context)
        where S : TState, new()
    {
        Context = context;
        Current = InstanceOf<S>();
        Current.OnEnter();
    }

    public void TransitionTo<S>()
        where S : TState, new()
    {
        var nextState = InstanceOf<S>();
        if (Current == nextState)
        {
            return;
        }
        Current.OnExit();
        Current = InstanceOf<S>();
        Current.OnEnter();
    }

    public S InstanceOf<S>()
        where S : TState, new()
    {
        Type t = typeof(S);
        TState s;
        if (!instanceMap.TryGetValue(t, out s))
        {
            s = new S();
            s.Init(Context);
            s.OnCreate();
            instanceMap.Add(t, s);
        }
        return (S)s;
    }

    public void FixedUpdate()
    {
        Current.OnFixedUpdate();
    }

    public void Update()
    {
        Current.OnUpdate();
    }

    public void LateUpdate()
    {
        Current.OnLateUpdate();
    }
}