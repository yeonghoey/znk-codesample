using UnityEngine;
using Cinemachine;
using FMODUnity;

public class PlayerActorGetHit : MonoBehaviour,
    IMETPlayerOnAttacked,
    IMETPlayerSMBGetHit,
    IMETPlayerHealthOnChanage
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private GameObject playerBloodSplatPrefab;
    [SerializeField] private Platformer platformer;
    [SerializeField] private Transform bloodSplatSpawnPoint;
    [SerializeField] private CinemachineImpulseSource cinemachineImpulseSource;
    [SerializeField] private float impulseForce;
    [SerializeField] private StudioEventEmitter audioGetHit;

    private Vector3 attackedFrom;
    private float knockbackSpeed;
    private int damage;
    private bool isAboutToDie;

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void IMETPlayerOnAttacked.OnAttacked(IPlayerOnAttackedInfo attackedInfo)
    {
        this.attackedFrom = attackedInfo.AttackedFrom;
        this.knockbackSpeed = attackedInfo.KnockbackSpeed;
        this.damage = attackedInfo.Damage;
        this.isAboutToDie = false;
        locomotor.Brake(attackedInfo.BrakeMultiplier);
        TriggerGetHit();
    }

    private void TriggerGetHit()
    {
        Vector3 worldDir = (attackedFrom - transform.position).ToWorldDir();
        Vector3 localDir = transform.InverseTransformDirection(worldDir);
        float getHitFromX = localDir.x;
        float getHitFromY = localDir.z;
        playerAnimatorDriver.TriggerGetHit(getHitFromX, getHitFromY);
    }

    void IMETPlayerSMBGetHit.OnEnter() { }

    void IMETPlayerSMBGetHit.OnEnterSolo()
    {
        playerHealth.Claim(damage);
        InstantiateBloodSplat();
        ApplyKnockback();
        ShakeCamera();
        PlayAudio();
    }

    void IMETPlayerHealthOnChanage.OnChanage(int healthRemaining)
    {
        if (healthRemaining == 0)
        {
            this.isAboutToDie = true;
        }
    }

    void IMETPlayerSMBGetHit.OnExitSolo()
    {
        // NOTE: Skip braking because Brake will be applied by PlayerActorDie;
        if (!isAboutToDie)
        {
            locomotor.Brake(playerParam.GetHitBrake);
        }
        locomotor.Mass = playerParam.NormalMass;
    }

    private void InstantiateBloodSplat()
    {
        var position = bloodSplatSpawnPoint.position;
        var worldDir = (attackedFrom - position).ToWorldDir();
        var rotation = Quaternion.LookRotation(worldDir);
        Instantiate(playerBloodSplatPrefab, position, rotation, platformer.Current);
    }

    private void ApplyKnockback()
    {
        locomotor.Mass = playerParam.GetHitKnockbackMass;
        Vector3 worldDir = (locomotor.Position - attackedFrom).ToWorldDir();
        locomotor.Push(worldDir, speedChange: knockbackSpeed);
    }

    private void ShakeCamera()
    {
        cinemachineImpulseSource.GenerateImpulseWithForce(impulseForce);
    }

    private void PlayAudio()
    {
        audioGetHit.Play();
    }
}
