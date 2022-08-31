public interface IMETPlayerSMBTake : IMessageExchangeTarget
{
    void OnEnter();
    void OnExit();
}

public class PlayerSMBTake : MessageExchangeSMB
{
    private static readonly System.Action<IMETPlayerSMBTake> callOnEnter = t => t.OnEnter();
    private static readonly System.Action<IMETPlayerSMBTake> callOnExit = t => t.OnExit();

    public override void OnEnter()
    {
        Invoke(callOnEnter);
    }

    public override void OnExit()
    {
        Invoke(callOnExit);
    }
}