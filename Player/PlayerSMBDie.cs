public interface IMETPlayerSMBDie : IMessageExchangeTarget
{
    void OnEnterSolo();
}

public class PlayerSMBDie : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBDie> callOnEnterSolo = t => t.OnEnterSolo();

    public override void OnEnterSolo()
    {
        Invoke(callOnEnterSolo);
    }
}