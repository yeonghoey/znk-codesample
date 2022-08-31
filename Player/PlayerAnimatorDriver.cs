using UnityEngine;

public class PlayerAnimatorDriver : MonoBehaviour,
    IMETPlayerParam
{
    [SerializeField] private Animator animator;
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private PlayerParam playerParam;

    private readonly int unarmedIdle = Animator.StringToHash("Main.Unarmed.Idle");
    private readonly int signpostIdle = Animator.StringToHash("Main.Signpost.Idle");
    private readonly int moveSpeed = Animator.StringToHash("moveSpeed");
    private readonly int roll = Animator.StringToHash("roll");
    private readonly int attack = Animator.StringToHash("attack");
    private readonly int attackRotatingClockwise = Animator.StringToHash("attackRotatingClockwise");
    private readonly int take = Animator.StringToHash("take");
    private readonly int drop = Animator.StringToHash("drop");
    private readonly int isHoldingSignpost = Animator.StringToHash("isHoldingSignpost");
    private readonly int getHit = Animator.StringToHash("getHit");
    private readonly int getHitFromX = Animator.StringToHash("getHitFromX");
    private readonly int getHitFromY = Animator.StringToHash("getHitFromY");
    private readonly int die = Animator.StringToHash("die");
    private readonly int isOnGround = Animator.StringToHash("isOnGround");
    private readonly int attackSpeedMultiplier = Animator.StringToHash("attackSpeedMultiplier");

    private Cooldown pauseResetCooldown;

    public bool IsOnGround
    {
        set => animator.SetBool(isOnGround, value);
    }

    void Awake()
    {
        pauseResetCooldown = new Cooldown();
        pauseResetCooldown.Set(0f, isReadyInitially: true);
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
        animator.Play(playerParam.IsHoldingSignpost ? signpostIdle : unarmedIdle);
    }

    void Update()
    {
        animator.SetFloat(moveSpeed, locomotor.Speed);
        if (!pauseResetCooldown.IsReady)
        {
            pauseResetCooldown.Tick(Time.deltaTime);
            if (pauseResetCooldown.IsReady)
            {
                animator.speed = 1f;
            }
        }
    }

    void IMETPlayerParam.OnChanged(PlayerParamData data)
    {
        animator.SetFloat(attackSpeedMultiplier, data.AttackSpeedMultiplier);
    }

    public void SetPause(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }
        animator.speed = 0f;
        pauseResetCooldown.Set(duration, isReadyInitially: false);
    }

    public void TriggerRoll()
    {
        animator.SetTrigger(roll);
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(attack);
    }

    public void SetAttackRotationDir(bool isClockwise)
    {
        animator.SetFloat(attackRotatingClockwise, isClockwise ? 1f : 0f);
    }

    public void TriggerDrop()
    {
        animator.SetTrigger(drop);
    }

    public void TriggerTake()
    {
        animator.SetTrigger(take);
    }

    public void SetIsHoldingSignpost(bool value)
    {
        animator.SetBool(isHoldingSignpost, value);
    }

    public void ResetActionTriggers()
    {
        animator.ResetTrigger(roll);
        animator.ResetTrigger(attack);
        animator.ResetTrigger(drop);
        animator.ResetTrigger(take);
    }

    public void TriggerGetHit(float x, float y)
    {
        animator.SetFloat(getHitFromX, x);
        animator.SetFloat(getHitFromY, y);
        animator.SetTrigger(getHit);
    }

    public void TriggerDie()
    {
        animator.SetTrigger(die);
    }
}
