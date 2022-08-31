public interface IMETPlayerSMBDrop : IMessageExchangeTarget
{
    void OnEnter();
    void OnExit();
}

public class PlayerSMBDrop : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBDrop> callOnEnter = t => t.OnEnter();
    private static readonly System.Action<IMETPlayerSMBDrop> callOnExit = t => t.OnExit();

    public override void OnEnter()
    {
        Invoke(callOnEnter);
    }

    public override void OnExit()
    {
        Invoke(callOnExit);
    }
}