using UnityEngine;

public class PlayerKnob : MonoBehaviour,
    IMETPlayerSensorItemKnob
{
    [SerializeField] private MessageExchange messageExchange;

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void IMETPlayerSensorItemKnob.OnEnter(Collider other)
    {
        var knob = other.GetComponent<ItemKnob>();
        if (knob == null)
        {
            return;
        }
        knob.OnPlayerEnter();
    }

    void IMETPlayerSensorItemKnob.OnExit(Collider other)
    {
        var knob = other.GetComponent<ItemKnob>();
        if (knob == null)
        {
            return;
        }
        knob.OnPlayerExit();
    }
}
