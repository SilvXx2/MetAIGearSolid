using UnityEngine;

public class FlockEnemyController : EnemyController, IBoid, IFlockContext
{
    [Header("Configuración de Flocking (Clase 12)")]
    [SerializeField] private FlockingManager flockingManager;
    [SerializeField] private Transform leader;
    [SerializeField] private float leaderStoppingDistance = 2.0f;

    [Header("Reacción al Ver al Jugador (LoS)")]
    [Tooltip("True = huye al ver al jugador (ej. ratones); False = persigue al jugador (ej. escoltas).")]
    [SerializeField] private bool fleeOnPlayerDetected = true;
    [SerializeField] private float safeDistance = 12f;

    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward;

    public ISteering FlockingSteering => flockingManager;
    public Transform Leader
    {
        get => leader;
        set
        {
            leader = value;
            var leaderBeh = GetComponent<LeaderBehaviour>();
            if (leaderBeh != null)
            {
                leaderBeh.SetTarget(leader);
            }
        }
    }

    public float LeaderStoppingDistance => leaderStoppingDistance;
    public IState FlockState { get; private set; }

    public override IState AlertState => fleeOnPlayerDetected ? RunAwayState : ChaseState;
    protected override float SafeDistance => safeDistance;

    protected override void Awake()
    {
        EnsureFlockingComponents();
        base.Awake();
    }

    protected override void InitializeStates()
    {
        base.InitializeStates();
        FlockState = new EnemyFlockState(this);
        // LSP: asignamos FlockState como estado base de retorno para que al terminar de huir o perseguir vuelva al Flocking
        PatrolState = FlockState;
    }

    private void EnsureFlockingComponents()
    {
        var separation = GetComponent<SeparationBehaviour>();
        if (separation == null)
        {
            separation = gameObject.AddComponent<SeparationBehaviour>();
        }

        var cohesion = GetComponent<CohesionBehaviour>();
        if (cohesion == null)
        {
            cohesion = gameObject.AddComponent<CohesionBehaviour>();
        }

        var alignment = GetComponent<AlignmentBehaviour>();
        if (alignment == null)
        {
            alignment = gameObject.AddComponent<AlignmentBehaviour>();
        }

        var leaderBehaviour = GetComponent<LeaderBehaviour>();
        if (leaderBehaviour == null)
        {
            leaderBehaviour = gameObject.AddComponent<LeaderBehaviour>();
        }

        if (leader != null)
        {
            leaderBehaviour.SetTarget(leader);
            // En modo Leader Following (Separation + Cohesion + Arrive al Líder),
            // reducimos Alignment y Cohesion para que los seguidores nunca se frenen entre sí ni miren a la pared
            cohesion.multiplier = 0.55f;
            alignment.multiplier = 0.2f;
            leaderBehaviour.multiplier = 2.6f;
        }

        if (flockingManager == null)
        {
            flockingManager = GetComponent<FlockingManager>();
            if (flockingManager == null)
            {
                flockingManager = gameObject.AddComponent<FlockingManager>();
            }
        }

        flockingManager.Initialize();
    }
}

