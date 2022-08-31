using UnityEngine;

[CreateAssetMenu(fileName = "PlayerParamData", menuName = "Zignpost/PlayerParamData")]
public class PlayerParamData : InheritableSO<PlayerParamData>
{
    [Header("Common")]
    [SerializeField] private Inheritable<int> healthMax;
    [SerializeField] private Inheritable<float> proximityRange;
    [SerializeField] private Inheritable<float> normalMass;

    [Header("Signpost")]
    [SerializeField] private Inheritable<bool> isHoldingSignpost;
    [SerializeField] private Inheritable<GameObject> signpostInHandPrefab;
    [SerializeField] private Inheritable<GameObject> signpostBrokenPrefab;
    [SerializeField] private Inheritable<GameObject> itemSignpostOnFloorPrefab;

    [Header("Move")]
    [SerializeField] private Inheritable<float> moveAcceleration;
    [SerializeField] private Inheritable<float> moveRotationSpeed;
    [SerializeField] private Inheritable<float> ledgeSoarAccel;

    [Header("Roll")]
    [SerializeField] private Inheritable<float> rollBrake;
    [SerializeField] private Inheritable<float> rollingSpeed;
    [SerializeField] private Inheritable<float> rollingSoarSpeed;
    [SerializeField] private Inheritable<float> rollingRotationSpeed;
    [SerializeField] private Inheritable<float> rollingRadiusMultiplier;

    [Header("Attack")]
    [SerializeField] private Inheritable<float> attackBrake;
    [SerializeField] private Inheritable<float> attackPushSpeed;
    [SerializeField] private Inheritable<float> attackSpeedMultiplier;
    [SerializeField] private Inheritable<float> attackingRotationSpeed;
    [SerializeField] private Inheritable<BoxChecker> attackBoxChecker;
    [SerializeField] private Inheritable<int> attackDamage;
    [SerializeField] private Inheritable<float> attackHitstopDuration;
    [SerializeField] private Inheritable<float> attackKnockbackSpeed;

    [Header("Take")]
    [SerializeField] private Inheritable<float> itemRange;
    [SerializeField] private Inheritable<float> takeBrake;

    [Header("Drop")]
    [SerializeField] private Inheritable<float> dropBrake;

    [Header("Die")]
    [SerializeField] private Inheritable<float> dieBrake;

    [Header("GetHit")]
    [SerializeField] private Inheritable<float> getHitKnockbackMassMultiplier;
    [SerializeField] private Inheritable<float> getHitBrake;

    public bool IsHoldingSignpost => isHoldingSignpost.Value;
    public BoxChecker AttackBoxChecker => attackBoxChecker.Value;
    public float AttackBrake => attackBrake.Value;
    public float AttackHitstopDuration => attackHitstopDuration.Value;
    public float AttackingRotationSpeed => attackingRotationSpeed.Value;
    public float AttackKnockbackSpeed => attackKnockbackSpeed.Value;
    public float AttackPushSpeed => attackPushSpeed.Value;
    public float AttackSpeedMultiplier => attackSpeedMultiplier.Value;
    public float DieBrake => dieBrake.Value;
    public float DropBrake => dropBrake.Value;
    public float GetHitBrake => getHitBrake.Value;
    public float GetHitKnockbackMassMultiplier => getHitKnockbackMassMultiplier.Value;
    public float ItemRange => itemRange.Value;
    public float LedgeSoarAccel => ledgeSoarAccel.Value;
    public float MoveAcceleration => moveAcceleration.Value;
    public float MoveRotationSpeed => moveRotationSpeed.Value;
    public float NormalMass => normalMass.Value;
    public float ProximityRange => proximityRange.Value;
    public float RollBrake => rollBrake.Value;
    public float RollingRadiusMultiplier => rollingRadiusMultiplier.Value;
    public float RollingRotationSpeed => rollingRotationSpeed.Value;
    public float RollingSoarSpeed => rollingSoarSpeed.Value;
    public float RollingSpeed => rollingSpeed.Value;
    public float TakeBrake => takeBrake.Value;
    public GameObject ItemSignpostOnFloorPrefab => itemSignpostOnFloorPrefab.Value;
    public GameObject SignpostBrokenPrefab => signpostBrokenPrefab.Value;
    public GameObject SignpostInHandPrefab => signpostInHandPrefab.Value;
    public int AttackDamage => attackDamage.Value;
    public int HealthMax => healthMax.Value;
}
