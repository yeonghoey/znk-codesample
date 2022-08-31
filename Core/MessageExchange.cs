using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMessageExchangeTarget { }

public class MessageExchange : MonoBehaviour
{
    private DefaultDictionary<Type, HashSet<IMessageExchangeTarget>> targets =
        new DefaultDictionary<Type, HashSet<IMessageExchangeTarget>>();

    public void Invoke<TTarget>(Action<TTarget> f)
        where TTarget : IMessageExchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t);
        }
    }

    public void Invoke<TTarget, TArg>(Action<TTarget, TArg> f, TArg arg)
        where TTarget : IMessageExchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t, arg);
        }
    }

    public void Invoke<TTarget, TArg0, TArg1>(Action<TTarget, TArg0, TArg1> f, TArg0 arg0, TArg1 arg1)
        where TTarget : IMessageExchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t, arg0, arg1);
        }
    }

    public void Invoke<TTarget, TArg0, TArg1, TArg2>(Action<TTarget, TArg0, TArg1, TArg2> f, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        where TTarget : IMessageExchangeTarget
    {
        foreach (var t in targets[typeof(TTarget)])
        {
            f((TTarget)t, arg0, arg1, arg2);
        }
    }

    public void Register(IMessageExchangeTarget target)
    {
        var baseType = typeof(IMessageExchangeTarget);

        foreach (var t in target.GetType().GetInterfaces())
        {
            if (t != baseType && baseType.IsAssignableFrom(t))
            {
                targets[t].Add(target);
            }
        }
    }

    public void Deregister(IMessageExchangeTarget target)
    {
        var baseType = typeof(IMessageExchangeTarget);

        foreach (var t in target.GetType().GetInterfaces())
        {
            if (t != baseType && baseType.IsAssignableFrom(t))
            {
                targets[t].Remove(target);
            }
        }
    }
}

