using System;
using UnityEngine;

public interface IMITPlayerBroadcasterOnGetHit : IMessageInterchangeTarget
{
    void OnGetHit();
}

public interface IMITPlayerBroadcasterOnDie : IMessageInterchangeTarget
{
    void OnDead(GameObject playerGO);
}

public class PlayerBroadcaster : MonoBehaviour,
    IMETPlayerSMBGetHit,
    IMETPlayerActorDie
{
    [SerializeField] private MessageInterchange messageInterchange;
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Player player;

    private static readonly Action<IMITPlayerBroadcasterOnGetHit> callOnGetHit = t => t.OnGetHit();
    private static readonly Action<IMITPlayerBroadcasterOnDie, GameObject> callOnDie = (t, go) => t.OnDead(go);

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void IMETPlayerSMBGetHit.OnEnter() { }

    void IMETPlayerSMBGetHit.OnEnterSolo()
    {
        messageInterchange.Invoke(callOnGetHit);
    }

    void IMETPlayerSMBGetHit.OnExitSolo() { }

    void IMETPlayerActorDie.OnDead()
    {
        messageInterchange.Invoke(callOnDie, player.gameObject);
    }
}
