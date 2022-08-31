using UnityEngine;

public class PlayerControl : MonoBehaviour, IMITPlayerInputDriver
{
    [SerializeField] private MessageInterchange messageInterchange;

    private const float sqrMagAttackInputThreshold = 0.75f * 0.75f;

    public Vector2 InputDirMove { get; private set; }
    public Vector3 WorldDirMove { get; private set; }
    public Vector2 InputDirAttack { get; private set; }
    public float SqrMagInputDirAttack { get => InputDirAttack.sqrMagnitude; }
    public Vector3 WorldDirAttack { get; private set; }
    public bool ButtonRoll { get; private set; }
    public bool ButtonTake { get; private set; }
    public bool ButtonDrop { get; private set; }

    void OnEnable()
    {
        messageInterchange.Register(this);
    }

    void OnDisable()
    {
        messageInterchange.Deregister(this);
    }

    void IMITPlayerInputDriver.OnMove(Vector2 inputDir)
    {
        InputDirMove = inputDir;
        WorldDirMove = InputDirMove.ToWorldDir();
    }

    void IMITPlayerInputDriver.OnAttack(Vector2 inputDir)
    {
        if (inputDir.sqrMagnitude < sqrMagAttackInputThreshold)
        {
            inputDir = Vector2.zero;
        }
        InputDirAttack = inputDir;
        WorldDirAttack = InputDirAttack.ToWorldDir();
    }

    void IMITPlayerInputDriver.OnRoll(bool isPressing)
    {
        ButtonRoll = isPressing;
    }

    void IMITPlayerInputDriver.OnTake(bool isPressing)
    {
        ButtonTake = isPressing;
    }

    void IMITPlayerInputDriver.OnDrop(bool isPressing)
    {
        ButtonDrop = isPressing;
    }
}
