using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [field: Header("Configuration (General)")]
    [field: Tooltip("Height of the character in metres")]
    [field: SerializeField] public float CharacterHeight { get; set; } = 1.6F;
    [field: Tooltip("Radius of the character in metres")]
    [field: SerializeField] public float CharacterRadius { get; set; } = 0.4F;
    [field: Tooltip("Collision group for the character to be a part of")]
    [field: SerializeField] public uint PhysicsColliderGroup { get; private set; } = (1 << 2);
    [field: Tooltip("Collision group for the character to collide against")]
    [field: SerializeField] public uint PhysicsCollisionGroup { get; private set; } = (1 << 0);
    [field: Tooltip("The maximum allowed number of collision bounces used during resolution")]
    [field: SerializeField] public int MaxCollisionBounces { get; private set; } = 4;
    [field: SerializeField] public float CollisionSkinWidth { get; private set; } = 0.01F;

    /**
     * General data...
    **/
    BlobAssetReference<Unity.Physics.Collider> capsuleCollider;


    [field: Header("Configuration (Look)")]
    [field: Tooltip("Look speed in degrees per second")]
    [field: SerializeField] public float LookSpeed { get; private set; } = 90F;
    [field: Tooltip("Look sensitivity as a multiplier (applied only to mouse input)")]
    [field: SerializeField] public float LookSensitivity { get; private set; } = 0.5F;
    [field: Tooltip("Maximum look down (-pitch) angle")]
    [field: SerializeField] public float LookMinPitch { get; private set; } = -80F;
    [field: Tooltip("Maximum look up (+pitch) angle")]
    [field: SerializeField] public float LookMaxPitch { get; private set; } = 80F;
    [field: Tooltip("The amount of inertia applied to looking")]
    [field: SerializeField] public float LookInertia { get; private set; } = 0.08F;

    /**
     * Look data...
    **/
    InputAction lookAction;
    Vector3 lookRotation = Vector3.zero;
    Vector2 lookInputTarget = Vector2.zero;
    Vector2 lookInputCurrent = Vector2.zero;
    Vector2 lookDampingVelocity = Vector2.zero;


    [field: Header("Configuration (Move)")]
    [field: Tooltip("Maximum walking speed")]
    [field: SerializeField] public float WalkSpeed { get; private set; } = 3.5F;
    [field: Tooltip("Maximum dashing speed")]
    [field: SerializeField] public float DashSpeed { get; private set; } = 7.0F;
    [field: Tooltip("The maximum angle of slope that can be walked on without sliding down")]
    [field: SerializeField] public float MaxSlopeAngle { get; private set; } = 45F;
    [field: SerializeField] public float GroundCastDistance { get; private set; } = 0.08F;
    [field: SerializeField] public float MoveInertia { get; private set; } = 0.08F;
    [field: Tooltip("Maximum height of a step/stair the player can step onto automatically")]
    [field: SerializeField] public float StepOffset { get; private set; } = 0.3F;
    [field: Tooltip("Gravity multiplier applied when airborne")]
    [field: SerializeField] public float Gravity { get; private set; } = 19.62F;
    /**
     * Move data...
    **/
    InputAction moveAction;
    InputAction dashAction;
    Vector2 moveInputTarget     = Vector2.zero;
    Vector2 moveInputCurrent    = Vector2.zero;
    Vector2 moveInputDampingVelocity = Vector2.zero;
    float moveSpeedTarget = 0F;
    float moveSpeedCurrent = 0F;
    float moveSpeedDampingVelocity = 0F;
    float verticalVelocity = 0f;


    [field: Header("Configuration (Debug)")]
    [field: SerializeField] public bool ShowDebugGizmos { get; private set; } = true;
    [field: SerializeField] public Color GroundedColor { get; private set; } = Color.green;
    [field: SerializeField] public Color AirborneColor { get; private set; } = Color.red;


    [field: Header("References (Internal)")]
    [field: SerializeField] public Camera Camera { get; private set; } = null;
    [field: SerializeField] public PlayerStateMachine StateMachine { get; private set; } = null;


    /**
     * General properties
    **/
    public bool IsGrounded { get; private set; } = false;
    public Vector3 GroundNormal { get; private set; } = Vector3.zero;

    /// <summary>
    /// Initializes player parameters from SoM data.
    /// </summary>
    public void Initialize(SoMProjectData projectData)
    {
        LookSpeed = projectData.playerTurnSpeed;

        WalkSpeed = projectData.playerWalkSpeed;
        DashSpeed = projectData.playerDashSpeed;
    }
   
    /// <summary>
    /// Reads 2D look vector (X = Horizontal turn, Y = Vertical pitch).
    /// When it is a mouse input, lookSensitivity is applied.
    /// </summary>
    public Vector2 ReadLookInput()
    {
        Vector2 lookValue = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;

        if (lookAction.activeControl?.device is Mouse)
            lookValue *= LookSensitivity;

        return lookValue;
    }
        
    /// <summary>
    /// Reads 2D movement vector (X = Strafe, Y = Forward/Backward).
    /// </summary>
    public Vector2 ReadMoveInput() =>
        moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    
    /// <summary>
    /// Reads dash value
    /// </summary>
    public bool ReadDashInput() =>
        dashAction != null && dashAction.IsPressed();

    /// <summary>
    /// Apply a look input to the player controller
    /// </summary>
    public unsafe void Turn(Vector2 input) =>
        lookInputTarget = input;

    /// <summary>
    /// Apply a move input to the player controller
    /// </summary>
    public unsafe void Move(Vector2 input, float speed)
    {
        moveInputTarget = input;
        moveSpeedTarget = speed;
    }

    /// <summary>
    /// Immediately moves the player to a specific position, and bearing
    /// </summary>
    public unsafe void Teleport(Vector3 position, Vector3 rotation)
    {
        // Set current pitch from rotation... Also set this on the camera.
        lookRotation = rotation;
        transform.localRotation        = Quaternion.Euler(0F, lookRotation.x, 0F);
        Camera.transform.localRotation = Quaternion.Euler(lookRotation.y, 0F, 0F);

        // Set transform position (to-do: add safty checks)
        transform.position = position + new Vector3(0F, GroundCastDistance, 0F);
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Start()
    {
        StateMachine = new PlayerStateMachine(this);

        // We'll also hook in here to grab our actions
        lookAction = GameManager.Instance.InputActions.FindAction("Player/Look");
        moveAction = GameManager.Instance.InputActions.FindAction("Player/Move");
        dashAction = GameManager.Instance.InputActions.FindAction("Player/Dash");

        // Construct the player capsule blob once for physics sweeps
        CreateCollider();
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void Update()
    {
        // Check if the controller is grounded ahead of time here...
        if (CastCollider(transform.position, Vector3.down, GroundCastDistance, out ColliderCastHit hit))
        {
            IsGrounded   = Vector3.Angle(hit.SurfaceNormal, Vector3.up) <= MaxSlopeAngle;
            GroundNormal = hit.SurfaceNormal; 
        }
        else
            IsGrounded = false;

        // Tick the state machine after calculating general properties
        StateMachine.Tick();

        // Turning is processed outside of the input being applied, so inertia can be used
        TurnInternal();
        MoveInternal();
    }

    /// <summary>
    /// MonoBehaviour Implementation.<br/>
    /// </summary>
    void OnDrawGizmos()
    {
        if (!ShowDebugGizmos)
            return;
    }

    /// <summary>
    /// Internal processing function for turn.
    /// </summary>
    void TurnInternal()
    {
        // We want optional inertia handling
        // When look inertia is zero or below, we just use the raw input value.
        if (LookInertia <= 0F)
            lookInputCurrent = lookInputTarget;
        else
            lookInputCurrent = Vector2.SmoothDamp(lookInputCurrent, lookInputTarget, ref lookDampingVelocity, LookInertia);

        // Calculate our look delta values now, using the smoothed input
        float deltaYaw = (lookInputCurrent.x * LookSpeed) * Time.deltaTime;
        float deltaPitch = (lookInputCurrent.y * LookSpeed) * Time.deltaTime;

        // Apply yaw rotation to the character body
        lookRotation.x += deltaYaw;
        transform.localRotation = Quaternion.Euler(0f, lookRotation.x, 0f);

        // Apply pitch rotation to the camera transform
        lookRotation.y = Mathf.Clamp(lookRotation.y - deltaPitch, LookMinPitch, LookMaxPitch);
        Camera.transform.localRotation = Quaternion.Euler(lookRotation.y, 0f, 0f);
    }

    /// <summary>
    /// Internal processing function for move.
    /// </summary>
    void MoveInternal()
    {
        // Smooth input
        if (MoveInertia <= 0f)
        {
            moveInputCurrent = moveInputTarget;
            moveSpeedCurrent = moveSpeedTarget;
        }
        else
        {
            moveInputCurrent = Vector2.SmoothDamp(moveInputCurrent, moveInputTarget, ref moveInputDampingVelocity, MoveInertia);
            moveSpeedCurrent = Mathf.SmoothDamp(moveSpeedCurrent, moveSpeedTarget, ref moveSpeedDampingVelocity, MoveInertia);
        }

        // Reset input targets
        moveInputTarget = Vector2.zero;
        moveSpeedTarget = 0f;
    }

    /// <summary>
    /// Creates an unmanaged capsule collider matching player height & radius.
    /// </summary>
    unsafe void CreateCollider()
    {
        float innerHeight = Mathf.Max(0.1f, CharacterHeight - (CharacterRadius * 2f));

        CapsuleGeometry geometry = new CapsuleGeometry
        {
            Vertex0 = new float3(0f, CharacterRadius, 0f),
            Vertex1 = new float3(0f, CharacterRadius + innerHeight, 0f),
            Radius  = CharacterRadius
        };

        CollisionFilter filter = new CollisionFilter
        {
            BelongsTo    = PhysicsColliderGroup,
            CollidesWith = PhysicsCollisionGroup,
            GroupIndex = 0
        };

        capsuleCollider = Unity.Physics.CapsuleCollider.Create(geometry, filter);
    }

    /// <summary>
    /// Casts the player collider through the world from origin in direction by distance
    /// </summary>
    unsafe bool CastCollider(Vector3 origin, Vector3 direction, float distance, out ColliderCastHit closestHit)
    {
        closestHit = new ColliderCastHit { };

        // We need the physics world to be able to cast. If we don't get it, return that we hit nothing
        if (!TryGetPhysicsWorld(out PhysicsWorld physicsWorld))
            return false;

        // We need to create the collider cast info
        ColliderCastInput castInfo = new ColliderCastInput
        {
            Collider    = (Unity.Physics.Collider*)capsuleCollider.GetUnsafePtr(),
            Orientation = transform.rotation,
            Start       = origin,
            End         = origin + (direction * distance)
        };

        // We can now begin the actual cast
        return physicsWorld.CastCollider(castInfo, out closestHit);
    }

    /// <summary>
    /// Gets the ECS physics world
    /// </summary>
    bool TryGetPhysicsWorld(out PhysicsWorld physicsWorld)
    {
        var defaultWorld = World.DefaultGameObjectInjectionWorld;
        if (defaultWorld != null && defaultWorld.IsCreated)
        {
            EntityManager entityManager = defaultWorld.EntityManager;

            using EntityQuery query = entityManager.CreateEntityQuery(typeof(PhysicsWorldSingleton));
            if (query.HasSingleton<PhysicsWorldSingleton>())
            {
                physicsWorld = query.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
                return true;
            }
        }

        physicsWorld = default;
        return false;
    }
}
