using System;
using UnityEngine;

public class MessageExchangeSMB : BaseSMB
{
    private MessageExchange CachedMessageExchange
    {
        get
        {
            if (cachedMessageExchange == null)
            {
                cachedMessageExchange = LastAnimator.GetComponentInParent<MessageExchange>();
            }
            return cachedMessageExchange;
        }
    }

    private MessageExchange cachedMessageExchange;

    protected void Invoke<T>(Action<T> f) where T : IMessageExchangeTarget
    {
        CachedMessageExchange.Invoke<T>(f);
    }
}