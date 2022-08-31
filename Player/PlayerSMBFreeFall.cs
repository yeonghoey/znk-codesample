public interface IMETPlayerSMBFreeFall : IMessageExchangeTarget
{
    void OnEnterSolo();
    void OnExitSolo();
}

public class PlayerSMBFreeFall : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBFreeFall> callOnEnterSolo = t => t.OnEnterSolo();
    private static readonly System.Action<IMETPlayerSMBFreeFall> callOnExitSolo = t => t.OnExitSolo();

    public override void OnEnterSolo()
    {
        Invoke(callOnEnterSolo);
    }

    public override void OnExitSolo()
    {
        Invoke(callOnExitSolo);
    }
}

