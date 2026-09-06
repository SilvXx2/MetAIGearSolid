using UnityEngine;

public class EnemyController : MonoBehaviour, IEnemyContext, IMover
{
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointThreshold = 0.5f;

    [SerializeField] private float loseTargetTime = 3f;
    [SerializeField] private EnemyLineOfSightSensor visionSensor;
    [SerializeField] private EnemyObstacleAvoidanceSensor avoidanceSensor;
    [SerializeField] private Rigidbody rb;

    public StateMachine StateMachine { get; private set; }
    public IMover Mover => this;
    public IVisionSensor Vision => visionSensor;
    public IAvoidanceSensor Avoidance => avoidanceSensor;
    public IState PatrolState { get; protected set; }
    public IState ChaseState { get; protected set; }
    public IState RunAwayState { get; protected set; }
    public virtual IState AlertState => ChaseState;
    public Transform[] Waypoints => waypoints;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float WaypointThreshold => waypointThreshold;
    public float LoseTargetTime => loseTargetTime;
    public Transform Transform => transform;

    protected virtual void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (visionSensor == null)
        {
            visionSensor = GetComponent<EnemyLineOfSightSensor>() ?? gameObject.AddComponent<EnemyLineOfSightSensor>();
        }

        if (avoidanceSensor == null)
        {
            avoidanceSensor = GetComponent<EnemyObstacleAvoidanceSensor>() ?? gameObject.AddComponent<EnemyObstacleAvoidanceSensor>();
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            PhysicsMaterial frictionless = new PhysicsMaterial("EnemyFrictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
            col.material = frictionless;
        }

        InitializeStates();
    }

    protected virtual void InitializeStates()
    {
        StateMachine = new StateMachine();
        PatrolState = new EnemyPatrolState(this);
        ChaseState = new EnemyChaseState(this);
    }

    private void Start()
    {
        StateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        StateMachine.Update();
    }

    public void Move(Vector3 direction, float speed)
    {
        rb.linearVelocity = new Vector3(direction.x * speed, 0f, direction.z * speed);
    }

    public void Rotate(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public int GetClosestWaypointIndex()
    {
        if (waypoints == null || waypoints.Length == 0) return 0;

        int closestIndex = 0;
        float minDistanceSqr = float.MaxValue;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            float distSqr = (waypoints[i].position - currentPos).sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            Gizmos.DrawWireSphere(waypoints[i].position, waypointThreshold);

            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }
    }
}
