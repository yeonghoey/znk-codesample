using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMessageInterchangeTarget { }

[CreateAssetMenu(fileName = "MessageInterchange", menuName = "Zignpost/MessageInterchange")]
public class MessageInterchange : ScriptableObject
{
    private DefaultDictionary<Type, HashSet<IMessageInterchangeTarget>> targets =
        new DefaultDictionary<Type, HashSet<IMessageInterchangeTarget>>();

    public void Invoke<TTarget>(Action<TTarget> f)
        where TTarget : IMessageInterchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t);
        }
    }

    public void Invoke<TTarget, TArg>(Action<TTarget, TArg> f, TArg arg)
        where TTarget : IMessageInterchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t, arg);
        }
    }

    public void Invoke<TTarget, TArg0, TArg1>(Action<TTarget, TArg0, TArg1> f, TArg0 arg0, TArg1 arg1)
        where TTarget : IMessageInterchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t, arg0, arg1);
        }
    }

    public void Invoke<TTarget, TArg0, TArg1, TArg2>(Action<TTarget, TArg0, TArg1, TArg2> f, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        where TTarget : IMessageInterchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t, arg0, arg1, arg2);
        }
    }

    public void Register(IMessageInterchangeTarget target)
    {
        var baseType = typeof(IMessageInterchangeTarget);

        foreach (var t in target.GetType().GetInterfaces())
        {
            if (t != baseType && baseType.IsAssignableFrom(t))
            {
                targets[t].Add(target);
            }
        }
    }

    public void Deregister(IMessageInterchangeTarget target)
    {
        var baseType = typeof(IMessageInterchangeTarget);

        foreach (var t in target.GetType().GetInterfaces())
        {
            if (t != baseType && baseType.IsAssignableFrom(t))
            {
                targets[t].Remove(target);
            }
        }
    }
}
