using System;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerSensorProximityTarget
{
    void OnEnter();
    void OnExit();
}

public interface IMETPlayerSensorProximity : IMessageExchangeTarget
{
    void OnEnter(Collider other);
    void OnExit(Collider other);
}

public class PlayerSensorProximity : MonoBehaviour,
    IMETPlayerParam
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private WallChecker wallChecker;

    private static readonly Action<IMETPlayerSensorProximity, Collider> callOnEnter = (t, other) => t.OnEnter(other);
    private static readonly Action<IMETPlayerSensorProximity, Collider> callOnExit = (t, other) => t.OnExit(other);

    private class Trackee
    {
        public IPlayerSensorProximityTarget target;
        public bool isEntered;
    }

    private const float refreshPeriod = 0.1f;

    private Dictionary<Collider, Trackee> candidates;
    private ObjectPool<Trackee> trackeePool;
    private List<Collider> removeStageBuffer;
    private Cooldown refreshCooldown;
    private SphereCollider trigger;

    void Awake()
    {
        candidates = new Dictionary<Collider, Trackee>();
        trackeePool = new ObjectPool<Trackee>();
        removeStageBuffer = new List<Collider>();
        refreshCooldown = new Cooldown();
        refreshCooldown.Set(refreshPeriod, isReadyInitially: true);
        trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0f;
    }

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void Update()
    {
        refreshCooldown.Tick(Time.deltaTime);
        if (refreshCooldown.Claim())
        {
            Refresh();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<IPlayerSensorProximityTarget>();
        if (target == null)
        {
            return;
        }
        var trackee = trackeePool.Take();
        trackee.target = target;
        trackee.isEntered = false;
        candidates[other] = trackee;
    }

    void OnTriggerExit(Collider other)
    {
        var target = other.GetComponent<IPlayerSensorProximityTarget>();
        if (target == null)
        {
            return;
        }
        Trackee trackee;
        bool ok = candidates.TryGetValue(other, out trackee);
        if (!ok)
        {
            return;
        }
        if (trackee.isEntered)
        {
            OnExit(other, trackee);
        }
        candidates.Remove(other);
        trackeePool.Return(trackee);
    }

    void IMETPlayerParam.OnChanged(PlayerParamData data)
    {
        trigger.radius = data.ProximityRange;
    }

    private void Refresh()
    {
        removeStageBuffer.Clear();
        foreach (var item in candidates)
        {
            var collider = item.Key;
            var trackee = item.Value;
            if (collider == null)
            {
                removeStageBuffer.Add(collider);
                trackeePool.Return(trackee);
                continue;
            }
            bool isHit = wallChecker.Check(collider);
            if (!trackee.isEntered && !isHit)
            {
                OnEnter(collider, trackee);
            }
            if (trackee.isEntered && isHit)
            {
                OnExit(collider, trackee);
            }
        }
        foreach (var collider in removeStageBuffer)
        {
            candidates.Remove(collider);
        }
    }

    private void OnEnter(Collider collider, Trackee trackee)
    {
        messageExchange.Invoke(callOnEnter, collider);
        trackee.target.OnEnter();
        trackee.isEntered = true;
    }

    private void OnExit(Collider collider, Trackee trackee)
    {
        messageExchange.Invoke(callOnExit, collider);
        trackee.target.OnExit();
        trackee.isEntered = false;
    }
}
