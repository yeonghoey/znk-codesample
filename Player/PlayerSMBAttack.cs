public interface IMETPlayerSMBAttack : IMessageExchangeTarget
{
    void OnEnter();
    void OnEnterSolo();
    void OnExitSolo();
    void OnExit();
}

public class PlayerSMBAttack : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBAttack> callOnEnter = t => t.OnEnter();
    private static readonly System.Action<IMETPlayerSMBAttack> callOnEnterSolo = t => t.OnEnterSolo();
    private static readonly System.Action<IMETPlayerSMBAttack> callOnExitSolo = t => t.OnExitSolo();
    private static readonly System.Action<IMETPlayerSMBAttack> callOnExit = t => t.OnExit();

    public override void OnEnter()
    {
        Invoke(callOnEnter);
    }

    public override void OnEnterSolo()
    {
        Invoke(callOnEnterSolo);
    }

    public override void OnExitSolo()
    {
        Invoke(callOnExitSolo);
    }

    public override void OnExit()
    {
        Invoke(callOnExit);
    }
}