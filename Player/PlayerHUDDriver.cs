using UnityEngine;
using FMODUnity;

public class PlayerHUDDriver : MonoBehaviour,
    IMETPlayerHealthOnChanage,
    IMITLevelEnd
{
    [SerializeField] private MessageInterchange messageInterchange;
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private HUDRing hudRing;
    [SerializeField] private HUDDotContainer hudHealth;
    [SerializeField] private StudioEventEmitter audioHeartbeat;

    private bool isOnEndPhase;

    void OnEnable()
    {
        messageInterchange.Register(this);
        messageExchange.Register(this);
        hudHealth.OnLoopBlink += PlayHeartbeat;
    }

    void OnDisable()
    {
        messageInterchange.Deregister(this);
        messageExchange.Deregister(this);
        hudHealth.OnLoopBlink -= PlayHeartbeat;
    }

    void Start()
    {
        isOnEndPhase = false;
        RefreshVisibility();
    }

    void IMETPlayerHealthOnChanage.OnChanage(int healthRemaining)
    {
        hudHealth.IsBlinking = healthRemaining == 1;
    }

    void IMITLevelEnd.OnEndPhase(bool isGoalArrived)
    {
        isOnEndPhase = true;
        hudHealth.IsBlinking = false;
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        hudRing.IsVisible = !isOnEndPhase;
        hudHealth.IsVisible = !isOnEndPhase;
    }

    private void PlayHeartbeat()
    {
        audioHeartbeat.Play();
    }
}
