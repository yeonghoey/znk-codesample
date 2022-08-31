using System;
using UnityEngine;

public interface IMETPlayerAnimationEventProxyOnFootstep : IMessageExchangeTarget
{
    void OnStepLeft();
    void OnStepRight();
}

public interface IMETPlayerAnimationEventProxyOnHit : IMessageExchangeTarget
{
    void OnHit();
    void OnImpact();
}

public interface IMETPlayerAnimationEventProxyOnTake : IMessageExchangeTarget
{
    void OnTake();
}

public interface IMETPlayerAnimationEventProxyOnDrop : IMessageExchangeTarget
{
    void OnDrop();
}

public class PlayerAnimationEventProxy : MonoBehaviour
{
    [SerializeField] private MessageExchange messageExchange;

    private static readonly Action<IMETPlayerAnimationEventProxyOnFootstep> callOnStepLeft = t => t.OnStepLeft();
    private static readonly Action<IMETPlayerAnimationEventProxyOnFootstep> callOnStepRight = t => t.OnStepRight();
    private static readonly Action<IMETPlayerAnimationEventProxyOnHit> callOnHit = t => t.OnHit();
    private static readonly Action<IMETPlayerAnimationEventProxyOnHit> callOnImpact = t => t.OnImpact();
    private static readonly Action<IMETPlayerAnimationEventProxyOnTake> callOnTake = t => t.OnTake();
    private static readonly Action<IMETPlayerAnimationEventProxyOnDrop> callOnDrop = t => t.OnDrop();

    // NOTE: Because of blending walk and run animations and
    // how animation events work, the two same step event can be triggered almost simultaneously;
    // To prevent this, aggregate consecutive step events.
    private Cooldown stepCooldown = new Cooldown();

    void Start()
    {
        stepCooldown.Set(duration: 0.1f, isReadyInitially: true);
    }

    void Update()
    {
        stepCooldown.Tick(Time.deltaTime);
    }

    void FootL()
    {
        if (stepCooldown.Claim())
        {
            messageExchange.Invoke(callOnStepLeft);
        }
    }

    void FootR()
    {
        if (stepCooldown.Claim())
        {
            messageExchange.Invoke(callOnStepRight);
        }
    }

    void Hit()
    {
        messageExchange.Invoke(callOnHit);
    }

    void Impact()
    {
        messageExchange.Invoke(callOnImpact);
    }

    void TakeSignpost()
    {
        messageExchange.Invoke(callOnTake);
    }

    void DropSignpost()
    {
        messageExchange.Invoke(callOnDrop);
    }
}
