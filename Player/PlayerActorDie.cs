using UnityEngine;
using FMODUnity;

public interface IMETPlayerActorDie : IMessageExchangeTarget
{
    void OnDead();
}

public class PlayerActorDie : MonoBehaviour,
    IMETPlayerHealthOnChanage,
    IMETPlayerOnBitten,
    IMETPlayerSMBDie
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private Locomotor locomotor;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private PlayerParam playerParam;
    [SerializeField] private GameObject playerBloodSplatPrefab;
    [SerializeField] private Platformer platformer;
    [SerializeField] private Collider deadCollider;
    [SerializeField] private StudioEventEmitter audioDie;

    private bool isAlreadyDead;

    void Awake()
    {
        isAlreadyDead = false;
    }

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void IMETPlayerHealthOnChanage.OnChanage(int healthRemaining)
    {
        if (healthRemaining == 0)
        {
            playerAnimatorDriver.TriggerDie();
        }
    }

    void IMETPlayerOnBitten.OnBitten(Vector3 bittenFrom)
    {
        var position = deadCollider.ClosestPoint(bittenFrom);
        var worldDir = (bittenFrom - position).ToWorldDir();
        var rotation = worldDir.WorldDirToRotation(Quaternion.identity);
        InstantiateBloodSplat(position, rotation);
    }

    void IMETPlayerSMBDie.OnEnterSolo()
    {
        if (isAlreadyDead)
        {
            InstantiateBloodSplat(locomotor.Position, Quaternion.identity);
            return;
        }
        isAlreadyDead = true;
        locomotor.Brake(playerParam.DieBrake);
        locomotor.DetectCollisions = false;
        audioDie.Play();
        messageExchange.Invoke<IMETPlayerActorDie>(t => t.OnDead());
    }

    private void InstantiateBloodSplat(Vector3 position, Quaternion rotation)
    {
        Instantiate(playerBloodSplatPrefab, position, rotation, platformer.Current);
    }
}
