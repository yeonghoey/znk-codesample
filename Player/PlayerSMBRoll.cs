public interface IMETPlayerSMBRoll : IMessageExchangeTarget
{
    void OnEnter();
    void OnEnterSolo();
    void OnExitSolo();
    void OnExit();
}

public class PlayerSMBRoll : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBRoll> callOnEnter = t => t.OnEnter();
    private static readonly System.Action<IMETPlayerSMBRoll> callOnEnterSolo = t => t.OnEnterSolo();
    private static readonly System.Action<IMETPlayerSMBRoll> callOnExitSolo = t => t.OnExitSolo();
    private static readonly System.Action<IMETPlayerSMBRoll> callOnExit = t => t.OnExit();

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