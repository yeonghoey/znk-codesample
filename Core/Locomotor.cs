using UnityEngine;

public class Locomotor : MonoBehaviour, IMETRampChecker
{
    [SerializeField] private MessageExchange messageExchange;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private new Collider collider;
    [SerializeField] private PhysicMaterial freeFallMaterial;
    [SerializeField] private Platformer platformer;
    [SerializeField] private string layerInvalidatedName;

    public Vector3 Position => rigidbody.position;
    public Quaternion Rotation => rigidbody.rotation;
    public Vector3 RelVelocity => rigidbody.velocity - platformer.Velocity;
    public float Speed => RelVelocity.WithY(0f).magnitude;

    // NOTE: This is for increasing the free falling speed;
    private const float freeFallDrag = 0.5f;

    private float defaultDrag;
    private PhysicMaterial defaultMaterial;
    private bool isOnGround;

    private bool isOnRamp;
    private Vector3 gravityPortion;

    private int layerDefault;
    private int layerInvalidated;

    void Awake()
    {
        messageExchange.Register(this);
        defaultDrag = rigidbody.drag;
        defaultMaterial = collider.sharedMaterial;
        isOnGround = true;
        isOnRamp = false;
        gravityPortion = Vector3.zero;
        layerDefault = rigidbody.gameObject.layer;
        layerInvalidated = LayerMask.NameToLayer(layerInvalidatedName);
    }

    void OnDestroy()
    {
        messageExchange.Deregister(this);
    }

    void FixedUpdate()
    {
        if (isOnRamp)
        {
            rigidbody.AddForce(-gravityPortion, ForceMode.Acceleration);
        }
    }

    void IMETRampChecker.OnChange(bool isOnRamp, Vector3 normal, Vector3 tangent)
    {
        this.isOnRamp = isOnRamp;
        this.gravityPortion = Vector3.Project(Physics.gravity, tangent);
    }

    public float Mass
    {
        get => rigidbody.mass;
        set => rigidbody.mass = value;
    }

    public bool DetectCollisions
    {
        get => rigidbody.gameObject.layer == layerDefault;
        set
        {
            rigidbody.gameObject.layer = value ? layerDefault : layerInvalidated;
        }
    }

    public bool IsOnGround
    {
        get => isOnGround;
        set
        {
            if (isOnGround != value)
            {
                isOnGround = value;
                rigidbody.drag = isOnGround ? defaultDrag : freeFallDrag;
                collider.sharedMaterial = isOnGround ? defaultMaterial : freeFallMaterial;
            }
        }

    }

    public Vector3 WorldDir => (rigidbody.rotation * Vector3.forward).WithY(0f).normalized;

    public void CorrectWorldDirMove(ref Vector3 worldDirMove, CapsuleCollider capsuleCollider, float distance, int layerMask)
    {
        if (worldDirMove == Vector3.zero)
        {
            return;
        }

        float radius = capsuleCollider.radius;
        float height = capsuleCollider.height;

        Vector3 position = rigidbody.position;
        float y = position.y;

        // NOTE: Offsets for detecting close objects;
        // i.e. When moving up the ramp, the ramp should be detected before the floor at the end
        // until the character reaches the end. Otherwise this will consider the floor as wall.
        Vector3 xzOffset = worldDirMove * -radius;
        const float yOffset = 0.05f;
        Vector3 point1 = xzOffset + position.WithY(y + radius - yOffset);
        Vector3 point2 = xzOffset + position.WithY(y + height - radius);

        RaycastHit hitInfo;
        bool isHit = Physics.CapsuleCast(point1, point2, radius, worldDirMove, out hitInfo, distance + radius, layerMask);
        if (isHit)
        {
            // NOTE: Skip if it's ramp-ish;
            if (Vector3.Angle(Vector3.up, hitInfo.normal) < 70f)
            {
                return;
            }

            Vector3 projected = Vector3.ProjectOnPlane(worldDirMove, hitInfo.normal).WithY(0f).normalized;
            if (projected != Vector3.zero)
            {
                worldDirMove = projected;
            }
        }
    }

    public Quaternion RotationToward(Vector3 worldDir)
    {
        if (worldDir == Vector3.zero)
        {
            return rigidbody.rotation;
        }
        return Quaternion.LookRotation(worldDir, Vector3.up);
    }

    public Quaternion RotationTowardTarget(Vector3 targetPosition)
    {
        var relativePos = targetPosition - rigidbody.position;
        var worldDir = relativePos.ToWorldDir();
        return RotationToward(worldDir);
    }

    public void RotateTowardTarget(Quaternion targetRotation, float rotationSpeed)
    {
        var maxDegreesDelta = rotationSpeed * Time.deltaTime;
        rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, targetRotation, maxDegreesDelta);
    }

    public void RotateTowardMovingDirection(float rotationSpeed)
    {
        Vector3 movingDir = RelVelocity.ToWorldDir();
        if (movingDir == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(movingDir, Vector3.up);
        var maxDegreesDelta = rotationSpeed * Time.deltaTime;
        rigidbody.rotation = Quaternion.RotateTowards(rigidbody.rotation, targetRotation, maxDegreesDelta);
    }

    public bool IsClockwise(Quaternion targetRotation)
    {
        return rigidbody.rotation.IsClockwise(targetRotation);
    }

    public bool IsLookingTarget(Quaternion targetRotation)
    {
        return rigidbody.rotation.IsSameOrientation(targetRotation);
    }

    public bool IsLookingTarget(Quaternion targetRotation, float angle)
    {
        return Quaternion.Angle(rigidbody.rotation, targetRotation) < angle;
    }

    public void Accelerate(Vector3 worldDir, float acceleration)
    {
        Vector3 accel = worldDir * acceleration;
        rigidbody.AddForce(accel, ForceMode.Acceleration);
    }

    public void Push(Vector3 worldDir, float speedChange)
    {
        rigidbody.AddForce(worldDir * speedChange, ForceMode.VelocityChange);
    }

    public void Brake(float multiplier)
    {
        var negVelocity = -RelVelocity * multiplier;
        negVelocity.y = 0f;
        rigidbody.AddForce(negVelocity, ForceMode.VelocityChange);
    }
}
