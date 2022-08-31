using UnityEngine;
using Cinemachine;
using FMODUnity;

public interface IMETPlayerSignpost : IMessageExchangeTarget
{
    void OnBreak();
}

public class PlayerSignpost : MonoBehaviour,
    IMETPlayerActorTake,
    IMETPlayerActorDrop
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private PlayerAnimatorDriver playerAnimatorDriver;
    [SerializeField] private Platformer platformer;
    [SerializeField] private Transform handSlot;
    [SerializeField] private Transform breakPoint;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private SilhouetteUpdater signpostInHandSilhouetteUpdater;
    [SerializeField] private int signpostSilhouetteMaterialIndex = 1;
    [SerializeField] private CinemachineImpulseSource CamShakeBreak;
    [SerializeField] private StudioEventEmitter audioWear;
    [SerializeField] private StudioEventEmitter audioBreak;

    private PlayerParamData signpostParamData;
    private int maxDurability;
    private int durability;

    public int MaxDurability { get => maxDurability; }
    public int Durability { get => durability; }

    void OnEnable()
    {
        messageExchange.Register(this);
    }

    void OnDisable()
    {
        messageExchange.Deregister(this);
    }

    void Start()
    {
        ClearState();
    }

    void IMETPlayerActorTake.OnTake(PlayerParamData signpostParamData, int maxDurability, int durability)
    {
        this.signpostParamData = signpostParamData;
        this.maxDurability = maxDurability;
        this.durability = durability;
        ResetHand();
        RefreshSignpostEffector();
    }

    void IMETPlayerActorDrop.OnDrop()
    {
        ClearSlot(handSlot);
        PlaceItemSigpost();
        ClearState();
    }

    public void Claim(int hitCount)
    {
        if (signpostParamData == null)
        {
            return;
        }

        if (durability == 0)
        {
            ClearSlot(handSlot);
            PlaceSignpostBroken();
            ClearState();
            messageExchange.Invoke<IMETPlayerSignpost>(t => t.OnBreak());
            playerAnimatorDriver.SetIsHoldingSignpost(false);
            CamShakeBreak.GenerateImpulse();
            audioBreak.Play();
        }
        else
        {
            durability = Mathf.Clamp(durability - hitCount, 0, maxDurability);
            RefreshSignpostEffector();
            if (durability == 0)
            {
                audioWear.Play();
            }
        }
    }

    private void ResetHand()
    {
        ClearSlot(handSlot);
        var prefab = signpostParamData.SignpostInHandPrefab;
        var go = Instantiate(prefab, handSlot);
        var signpostRenderer = go.GetComponent<Renderer>();
        signpostInHandSilhouetteUpdater.Activate(signpostRenderer, signpostSilhouetteMaterialIndex);
    }

    private void ClearSlot(Transform slot)
    {
        signpostInHandSilhouetteUpdater.Deactivate();
        foreach (Transform s in slot)
        {
            Destroy(s.gameObject);
        }
    }

    private void PlaceItemSigpost()
    {
        var prefab = signpostParamData.ItemSignpostOnFloorPrefab;
        var go = Instantiate(prefab, dropPoint.position, dropPoint.rotation);
        var itemSignpost = go.GetComponent<ItemSignpost>();
        itemSignpost.InitUsed(maxDurability, durability);
        if (platformer.Current != null)
        {
            var p = go.GetComponent<Platformer>();
            p.Inherit(platformer);
        }
    }

    private void PlaceSignpostBroken()
    {
        var prefab = signpostParamData.SignpostBrokenPrefab;
        var go = Instantiate(prefab, breakPoint.position, breakPoint.rotation);
        if (platformer.Current != null)
        {
            var platformers = go.GetComponentsInChildren<Platformer>();
            foreach (var p in platformers)
            {
                p.Inherit(platformer);
            }
        }

    }

    private void ClearState()
    {
        signpostParamData = null;
        maxDurability = 0;
        durability = 0;
    }

    private void RefreshSignpostEffector()
    {
        if (durability > 0)
        {
            return;
        }
        var effector = handSlot.GetComponentInChildren<SignpostWornEffector>();
        if (effector == null)
        {
            return;
        }
        effector.enabled = true;
    }
}
