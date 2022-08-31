using UnityEngine;
using FMODUnity;

public class PlayerReactorFootstep : MonoBehaviour,
    IMETPlayerAnimationEventProxyOnFootstep
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private float maxSpeedForLowestPitch;

    [Header("L")]
    [SerializeField] private Transform footL;
    [SerializeField] private StudioEventEmitter footLAudio;

    [Header("R")]
    [SerializeField] private Transform footR;
    [SerializeField] private StudioEventEmitter footRAudio;

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void IMETPlayerAnimationEventProxyOnFootstep.OnStepLeft()
    {
        footLAudio.Play();
        footLAudio.SetParameter("playerSpeedNormalized", CalculateSpeedNormalized());
    }

    void IMETPlayerAnimationEventProxyOnFootstep.OnStepRight()
    {
        footRAudio.Play();
        footRAudio.SetParameter("playerSpeedNormalized", CalculateSpeedNormalized());
    }

    float CalculateSpeedNormalized()
    {
        return Mathf.Min(1.0f, locomotor.Speed / maxSpeedForLowestPitch);
    }
}
