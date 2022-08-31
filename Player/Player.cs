using System;
using UnityEngine;

public interface IMETPlayerOnAggroed : IMessageExchangeTarget
{
    void OnAggroChanged(int aggroedCount);
}

public interface IMETPlayerOnAttacked : IMessageExchangeTarget
{
    void OnAttacked(IPlayerOnAttackedInfo attackedInfo);
}

public interface IMETPlayerOnBitten : IMessageExchangeTarget
{
    void OnBitten(Vector3 bittenFrom);
}

public interface IPlayerOnAttackedInfo
{
    Vector3 AttackedFrom { get; }
    float BrakeMultiplier { get; }
    float KnockbackSpeed { get; }
    int Damage { get; }
}

public class PlayerOnAttackedInfo : IPlayerOnAttackedInfo
{
    public Vector3 attackedFrom;
    public float brakeMultiplier;
    public float knockbackSpeed;
    public int damage;

    Vector3 IPlayerOnAttackedInfo.AttackedFrom => attackedFrom;
    float IPlayerOnAttackedInfo.BrakeMultiplier => brakeMultiplier;
    float IPlayerOnAttackedInfo.KnockbackSpeed => knockbackSpeed;
    int IPlayerOnAttackedInfo.Damage => damage;
}

public class Player : MonoBehaviour,
    IZombieTarget,
    IZombieActorAttackVictim,
    IZombieActorBiteVictim,
    IFreeFallDeathCheckerTarget,
    ITrapTarget,
    IMITLevelEnd,
    IMETPlayerSMBGetHit,
    IMETPlayerSMBRoll
{
    [SerializeField] private ZombieAggroTracker zombieAggroTracker;
    [SerializeField] private MessageInterchange messageInterchange;
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private PlayerParam playerParam;

    public static Vector3 InitialPosition;
    public static Quaternion InitialRotation;

    private static readonly Action<IMETPlayerOnAggroed, int> callOnAggroChanged = (t, n) => t.OnAggroChanged(n);

    private PlayerOnAttackedInfo attackedInfo = new PlayerOnAttackedInfo();

    private const float damageableDuration = 0.25f;
    private const float skipSequentialHitDuration = 1.5f;

    private Cooldown damageableCooldown;
    private Cooldown skipSequentialHitCooldown;
    private bool isGetHitting;
    private bool isRolling;
    private bool isDead;
    private bool isSafe;

    void Awake()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;

        isGetHitting = false;
        isRolling = false;
        isDead = false;
        isSafe = false;
        damageableCooldown = new Cooldown();
        damageableCooldown.Set(damageableDuration, isReadyInitially: true);
        skipSequentialHitCooldown = new Cooldown();
        skipSequentialHitCooldown.Set(skipSequentialHitDuration, isReadyInitially: true);
    }

    void OnEnable()
    {
        zombieAggroTracker.OnChanged += OnAggroChanged;
        messageInterchange.Register(this);
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        zombieAggroTracker.OnChanged -= OnAggroChanged;
        messageInterchange.Deregister(this);
        messageExchange.Deregister(this);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        damageableCooldown.Tick(dt);
        skipSequentialHitCooldown.Tick(dt);
    }

    private void OnAggroChanged(int count)
    {
        messageExchange.Invoke(callOnAggroChanged, count);
    }

    Vector3 IZombieTarget.Position { get => transform.position; }

    bool IZombieTarget.IsDead { get => isDead; }

    GameObject IZombieTarget.RootGO { get => this.gameObject; }

    void IZombieActorAttackVictim.OnAttacked(IZombieActorAttackInfo attackInfo)
    {
        if (isGetHitting || isDead || isSafe)
        {
            return;
        }

        if (damageableCooldown.IsReady && skipSequentialHitCooldown.Claim())
        {
            attackedInfo.attackedFrom = attackInfo.AttackedFrom;
            attackedInfo.brakeMultiplier = 0f;
            attackedInfo.knockbackSpeed = attackInfo.KnockbackSpeed;
            attackedInfo.damage = attackInfo.Damage;
            messageExchange.Invoke<IMETPlayerOnAttacked>(t => t.OnAttacked(attackedInfo));
        }
    }

    void IZombieActorBiteVictim.OnBitten(ZombieActorBiteInfo biteInfo)
    {
        messageExchange.Invoke<IMETPlayerOnBitten>(t => t.OnBitten(biteInfo.BittenFrom));
    }

    void IFreeFallDeathCheckerTarget.OnDeathContact()
    {
        // NOTE: Don't skip when isDead is true; actors will react to this event.
        if (isSafe)
        {
            return;
        }
        attackedInfo.attackedFrom = transform.position - transform.forward;
        attackedInfo.brakeMultiplier = 1f;
        attackedInfo.knockbackSpeed = 0f;
        attackedInfo.damage = playerParam.HealthMax;
        messageExchange.Invoke<IMETPlayerOnAttacked>(t => t.OnAttacked(attackedInfo));
    }

    bool ITrapTarget.IsRolling => isRolling;

    void ITrapTarget.OnTrapped(TrapInfo trapInfo)
    {
        if (isGetHitting || isDead || isSafe)
        {
            return;
        }
        attackedInfo.attackedFrom = trapInfo.Position;
        attackedInfo.brakeMultiplier = trapInfo.BrakeMultiplier;
        attackedInfo.knockbackSpeed = trapInfo.KnockbackSpeed;
        attackedInfo.damage = trapInfo.Damage;
        messageExchange.Invoke<IMETPlayerOnAttacked>(t => t.OnAttacked(attackedInfo));
    }

    void IMITLevelEnd.OnEndPhase(bool isGoalArrived)
    {
        if (isGoalArrived)
        {
            isSafe = true;
        }
        else
        {
            isDead = true;
        }
    }

    void IMETPlayerSMBGetHit.OnEnter()
    {
        isGetHitting = true;
    }

    void IMETPlayerSMBGetHit.OnEnterSolo() { }

    void IMETPlayerSMBGetHit.OnExitSolo()
    {
        isGetHitting = false;
    }

    void IMETPlayerSMBRoll.OnEnter()
    {
        damageableCooldown.Reset(isReady: false);
        isRolling = true;
    }
    void IMETPlayerSMBRoll.OnEnterSolo() { }
    void IMETPlayerSMBRoll.OnExitSolo() { }

    void IMETPlayerSMBRoll.OnExit()
    {
        isRolling = false;
    }
}