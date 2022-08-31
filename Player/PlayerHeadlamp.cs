using System.Collections;
using UnityEngine;
using FMODUnity;

public class PlayerHeadlamp : MonoBehaviour,
    IMETPlayerSensorItemHeadlamp,
    IMITLevelLightDriver
{
    [SerializeField] private MessageInterchange messageInterchange;
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private LightDriver playerHeadlampLight;
    [SerializeField] private SkinnedMeshRenderer playerHeadlamp;
    [SerializeField] private StudioEventEmitter audioHeadlampTake;

    private bool isDark => currentBrightness == 0f;
    private bool isEquipped => playerHeadlamp.enabled;

    private float currentBrightness;

    void Awake()
    {
        currentBrightness = 0f;
    }

    void OnEnable()
    {
        messageInterchange.Register(this);
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageInterchange.Deregister(this);
        messageExchange.Deregister(this);
    }

    void IMETPlayerSensorItemHeadlamp.OnEnter(Collider other)
    {
        if (playerHeadlampLight.IsOn)
        {
            return;
        }

        var headlamp = other.GetComponent<ItemConsumable>();
        if (headlamp == null)
        {
            return;
        }

        if (headlamp.Claim())
        {
            StartCoroutine(QueueEquip(delay: headlamp.EffectDuration + 0.2f));
        }
    }

    void IMETPlayerSensorItemHeadlamp.OnExit(Collider other) { }

    void IMITLevelLightDriver.OnDesiredBrightnessUpdated(float desiredBrightness)
    {
        currentBrightness = desiredBrightness;
        RefreshLampState();
    }

    private IEnumerator QueueEquip(float delay)
    {
        audioHeadlampTake.Play();
        yield return new WaitForSeconds(delay);
        playerHeadlamp.enabled = true;
        RefreshLampState();

    }

    private void RefreshLampState()
    {
        if (isEquipped && isDark)
        {
            playerHeadlampLight.Switch(true);
        }
        else
        {
            playerHeadlampLight.Switch(false);
        }
    }
}
