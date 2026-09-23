using UnityEngine;

public class EnemyDecisionTreeController : EnemyController
{
    [Header("Parametros de Combate Cuerpo a Cuerpo")]
    [SerializeField] private bool hasWeapon = true;
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private int currentAmmo = 5;
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float reloadDuration = 3.5f;
    [SerializeField] private Transform weaponSpawnPoint;

    [Header("Debug")]
    [SerializeField] private string lastDecision = "Ninguna";
    [SerializeField] private float currentReloadTimer = 0f;

    public IState AttackState { get; private set; }
    private IDecisionNode rootNode;
    private IAttackEffect attackEffect;

    protected override void InitializeStates()
    {
        base.InitializeStates();
        RunAwayState ??= new EnemyRunAwayState(this, safeDistance: 12f);
        AttackState = new EnemyAttackState(this, OnEnemyAttacked, attackCooldown);

        attackEffect = GetComponent<IAttackEffect>();

        if (attackEffect == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IAttackEffect (por ejemplo AttackVisualEffect).", this);
        }
    }

    private void Start()
    {
        BuildDecisionTree();
        StateMachine.Initialize(PatrolState);
    }

    private void Update()
    {
        rootNode?.Excecute();
        StateMachine.Update();
    }

    private void BuildDecisionTree()
    {
        var accionAtacar = new DecisionActionNode(() => SetDecisionState(AttackState, "Atacar"));
        var accionAcercarme = new DecisionActionNode(() => SetDecisionState(ChaseState, "Acercarme"));
        var accionSeguirPatrullando = new DecisionActionNode(DecisionSeguirPatrullando);
        var accionHuirRecargar = new DecisionActionNode(DecisionHuirYRecargar);
        var accionBuscarArma = new DecisionActionNode(DecisionBuscarArma);

        var preguntaEstoyCerca = new DecisionQuestionNode(IsCloseToPlayer, accionAtacar, accionAcercarme);
        var preguntaVeoAlJugador = new DecisionQuestionNode(() => Vision != null && Vision.CanSeeTarget, preguntaEstoyCerca, accionSeguirPatrullando);
        var preguntaTengoMunicion = new DecisionQuestionNode(() => currentAmmo > 0, preguntaVeoAlJugador, accionHuirRecargar);
        var preguntaTengoArma = new DecisionQuestionNode(() => hasWeapon, preguntaTengoMunicion, accionBuscarArma);

        rootNode = preguntaTengoArma;
    }

    private bool IsCloseToPlayer()
    {
        Transform target = Vision?.Target;
        if (target == null) return false;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        return toTarget.sqrMagnitude <= (attackDistance * attackDistance);
    }

    private void SetDecisionState(IState state, string decisionName)
    {
        lastDecision = decisionName;
        currentReloadTimer = 0f;

        if (StateMachine.CurrentState != state)
        {
            StateMachine.ChangeState(state);
        }
    }

    private void DecisionSeguirPatrullando()
    {
        lastDecision = (StateMachine.CurrentState == IdleState) ? "En reposo / guardia" : "Seguir patrullando";
        currentReloadTimer = 0f;

        // Si ya está en PatrolState o en IdleState (rutina de patrulla), no interrumpir la máquina de estados
        if (StateMachine.CurrentState != PatrolState && StateMachine.CurrentState != IdleState)
        {
            StateMachine.ChangeState(PatrolState);
        }
    }

    private void DecisionHuirYRecargar()
    {
        if (StateMachine.CurrentState != RunAwayState)
        {
            StateMachine.ChangeState(RunAwayState);
        }

        lastDecision = $"Huyendo y Recargando ({currentReloadTimer:F1}/{reloadDuration}s)";
        currentReloadTimer += Time.deltaTime;

        if (currentReloadTimer >= reloadDuration)
        {
            currentAmmo = maxAmmo;
            currentReloadTimer = 0f;
        }
    }

    private void DecisionBuscarArma()
    {
        currentReloadTimer = 0f;
        lastDecision = "Buscando arma";

        if (weaponSpawnPoint != null && Vector3.Distance(transform.position, weaponSpawnPoint.position) > 1.5f)
        {
            Vector3 desiredDir = (weaponSpawnPoint.position - transform.position).normalized;
            desiredDir.y = 0f;
            Vector3 moveDir = Avoidance != null ? Avoidance.GetSteeredDirection(desiredDir) : desiredDir;
            Mover.Move(moveDir, PatrolSpeed);
            Mover.Rotate(moveDir);
            return;
        }

        hasWeapon = true;
        currentAmmo = maxAmmo;
    }

    private void OnEnemyAttacked()
    {
        if (currentAmmo > 0) currentAmmo--;
        attackEffect?.TriggerEffect();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        if (weaponSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, weaponSpawnPoint.position);
            Gizmos.DrawWireCube(weaponSpawnPoint.position, Vector3.one * 0.8f);
        }
    }
}
