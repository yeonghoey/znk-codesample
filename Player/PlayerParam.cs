using System.Collections;
using UnityEngine;

public class PlayerParam : MonoBehaviour,
    ISerializationCallbackReceiver,
    IMETPlayerActorTake,
    IMETPlayerActorDrop,
    IMETPlayerSignpost
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private PlayerParamData unarmed;

    public bool IsHoldingSignpost => current.IsHoldingSignpost;
    public BoxChecker AttackBoxChecker => current.AttackBoxChecker;
    public float AttackBrake => current.AttackBrake;
    public float AttackHitstopDuration => current.AttackHitstopDuration;
    public float AttackingRotationSpeed => current.AttackingRotationSpeed;
    public float AttakKnockbackSpeed => current.AttackKnockbackSpeed;
    public float AttackPushSpeed => current.AttackPushSpeed;
    public float AttackSpeedMultiplier => current.AttackSpeedMultiplier;
    public float DieBrake => current.DieBrake;
    public float DropBrake => current.DropBrake;
    public float GetHitBrake => current.GetHitBrake;
    public float GetHitKnockbackMass => current.NormalMass * current.GetHitKnockbackMassMultiplier;
    public float ItemRange => current.ItemRange;
    public float LedgeSoarAccel => current.LedgeSoarAccel;
    public float MoveAcceleration => current.MoveAcceleration;
    public float MoveRotationSpeed => current.MoveRotationSpeed;
    public float NormalMass => current.NormalMass;
    public float ProximityRange => current.ProximityRange;
    public float RollBrake => current.RollBrake;
    public float RollingRadiusMultiplier => current.RollingRadiusMultiplier;
    public float RollingRotationSpeed => current.RollingRotationSpeed;
    public float RollingSpeed => current.RollingSpeed;
    public float RollingSoarSpeed => current.RollingSoarSpeed;
    public float TakeBrake => current.TakeBrake;
    public GameObject ItemSignpostOnFloorPrefab => current.ItemSignpostOnFloorPrefab;
    public GameObject SignpostBrokenPrefab => current.SignpostBrokenPrefab;
    public GameObject SignpostInHandPrefab => current.SignpostInHandPrefab;
    public int AttackDamage => current.AttackDamage;
    public int HealthMax => current.HealthMax;

    private PlayerParamData current;

    void ISerializationCallbackReceiver.OnBeforeSerialize() { }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        current = unarmed;
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
        Set(unarmed);
    }

    void IMETPlayerActorTake.OnTake(PlayerParamData signpostParamData, int maxDurability, int durability)
    {
        SetDelayed(signpostParamData);
    }

    void IMETPlayerActorDrop.OnDrop()
    {
        SetDelayed(unarmed);
    }

    void IMETPlayerSignpost.OnBreak()
    {
        SetDelayed(unarmed);
    }

    private void Set(PlayerParamData data)
    {
        current = data;
        messageExchange.Invoke<IMETPlayerParam>(t => t.OnChanged(data));
    }

    private void SetDelayed(PlayerParamData data)
    {
        // NOTE: This is for breaking the message chain;
        StartCoroutine(SetDelayedInternal(data));
    }

    private IEnumerator SetDelayedInternal(PlayerParamData data)
    {
        yield return null;
        Set(data);
    }
}

public interface IMETPlayerParam : IMessageExchangeTarget
{
    void OnChanged(PlayerParamData data);
}