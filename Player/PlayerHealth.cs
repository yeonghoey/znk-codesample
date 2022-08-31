using System;
using UnityEngine;
using FMODUnity;

public interface IMETPlayerHealthOnMedkit : IMessageExchangeTarget
{
    void OnUsed(ItemConsumable itemMedkit);
}

public interface IMETPlayerHealthOnChanage : IMessageExchangeTarget
{
    void OnChanage(int healthRemaining);
}

public class PlayerHealth : MonoBehaviour,
    IMETPlayerSensorItemMedkit
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private HUDDotContainer hudHealth;
    [SerializeField] private StudioEventEmitter audioMedkit;

    // NOTE: This is for ending;
    [SerializeField] private bool isInvincible = false;

    private static readonly Action<IMETPlayerHealthOnChanage, int> callOnChange =
        (t, healthRemaining) => t.OnChanage(healthRemaining);

    private const int medkitAmount = 1;

    private int healthMax;
    private int health;
    private ItemConsumable itemMedkitInteracting;

    public int Max => healthMax;
    public int Value => health;

    void Awake()
    {
        healthMax = playerParam.HealthMax;
        health = healthMax;
        itemMedkitInteracting = null;
    }

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void Start()
    {
        hudHealth.SetMax(healthMax);
    }

    void Update()
    {
        UpdateItemMedikit();
    }

    void IMETPlayerSensorItemMedkit.OnEnter(Collider other)
    {
        itemMedkitInteracting = other.GetComponent<ItemConsumable>();
    }

    void IMETPlayerSensorItemMedkit.OnExit(Collider other)
    {
        itemMedkitInteracting = null;
    }

    public void Claim(int amount)
    {
        if (isInvincible || health == 0)
        {
            return;
        }
        AdjustHealth(-amount);
    }

    private void UpdateItemMedikit()
    {
        if (health == healthMax)
        {
            return;
        }
        if (itemMedkitInteracting == null)
        {
            return;
        }
        if (itemMedkitInteracting.Claim())
        {
            AdjustHealth(+medkitAmount);
            audioMedkit.Play();
            messageExchange.Invoke<IMETPlayerHealthOnMedkit>(t => t.OnUsed(itemMedkitInteracting));
        }
    }

    private void AdjustHealth(int amount)
    {
        int value = Mathf.Clamp(health + amount, 0, healthMax);
        health = value;
        hudHealth.SetValue(value);
        messageExchange.Invoke(callOnChange, health);
    }
}
