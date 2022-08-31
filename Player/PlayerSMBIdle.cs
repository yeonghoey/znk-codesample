public interface IMETPlayerSMBIdle : IMessageExchangeTarget
{
    void OnEnterSolo();
    void OnExitSolo();
}

public class PlayerSMBIdle : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBIdle> callOnEnterSolo = t => t.OnEnterSolo();
    private static readonly System.Action<IMETPlayerSMBIdle> callOnExitSolo = t => t.OnExitSolo();

    public override void OnEnterSolo()
    {
        Invoke(callOnEnterSolo);
    }

    public override void OnExitSolo()
    {
        Invoke(callOnExitSolo);
    }
}