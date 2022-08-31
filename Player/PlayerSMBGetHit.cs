public interface IMETPlayerSMBGetHit : IMessageExchangeTarget
{
    void OnEnter();
    void OnEnterSolo();
    void OnExitSolo();
}

public class PlayerSMBGetHit : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBGetHit> callOnEnter = t => t.OnEnter();
    private static readonly System.Action<IMETPlayerSMBGetHit> callOnEnterSolo = t => t.OnEnterSolo();
    private static readonly System.Action<IMETPlayerSMBGetHit> callOnExitSolo = t => t.OnExitSolo();

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
}

