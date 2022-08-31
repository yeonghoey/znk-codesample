using UnityEngine;

public interface IMETPlayerSensorItemHeadlamp : IMessageExchangeTarget
{
    void OnEnter(Collider other);
    void OnExit(Collider other);
}

public interface IMETPlayerSensorItemKnob : IMessageExchangeTarget
{
    void OnEnter(Collider other);
    void OnExit(Collider other);
}

public interface IMETPlayerSensorItemMedkit : IMessageExchangeTarget
{
    void OnEnter(Collider other);
    void OnExit(Collider other);
}

public interface IMETPlayerSensorItemSignpost : IMessageExchangeTarget
{
    void OnEnter(Collider other);
    void OnExit(Collider other);
}

public class PlayerSensorItem : MonoBehaviour,
    IMETPlayerParam,
    IMETPlayerHealthOnChanage
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private CapsuleCollider mainCollider;
    [SerializeField] private PlayerParam playerParam;

    private CapsuleCollider trigger;
    private bool isDead;

    void Awake()
    {
        trigger = gameObject.AddComponent<CapsuleCollider>();
        trigger.isTrigger = true;
        trigger.center = mainCollider.center;
        trigger.direction = mainCollider.direction;
        trigger.height = mainCollider.height;
        trigger.radius = 0f;
        isDead = false;
    }

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead)
        {
            return;
        }
        switch (other.gameObject.tag)
        {
            case "ItemHeadlamp":
                messageExchange.Invoke<IMETPlayerSensorItemHeadlamp>(t => t.OnEnter(other));
                break;
            case "ItemKnob":
                messageExchange.Invoke<IMETPlayerSensorItemKnob>(t => t.OnEnter(other));
                break;
            case "ItemMedkit":
                messageExchange.Invoke<IMETPlayerSensorItemMedkit>(t => t.OnEnter(other));
                break;
            case "ItemSignpost":
                messageExchange.Invoke<IMETPlayerSensorItemSignpost>(t => t.OnEnter(other));
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "ItemHeadlamp":
                messageExchange.Invoke<IMETPlayerSensorItemHeadlamp>(t => t.OnExit(other));
                break;
            case "ItemKnob":
                messageExchange.Invoke<IMETPlayerSensorItemKnob>(t => t.OnExit(other));
                break;
            case "ItemMedkit":
                messageExchange.Invoke<IMETPlayerSensorItemMedkit>(t => t.OnExit(other));
                break;
            case "ItemSignpost":
                messageExchange.Invoke<IMETPlayerSensorItemSignpost>(t => t.OnExit(other));
                break;
        }
    }

    void IMETPlayerParam.OnChanged(PlayerParamData data)
    {
        trigger.radius = data.ItemRange;
    }

    void IMETPlayerHealthOnChanage.OnChanage(int healthRemaining)
    {
        if (healthRemaining == 0)
        {
            isDead = true;
        }
    }
}
