using UnityEngine;

public class EnemyDecisionTreeController : EnemyController, IWeaponHolder
{
    [Header("Parametros de Combate Cuerpo a Cuerpo / Arma")]
    [SerializeField] private bool hasWeapon = true;
    [SerializeField] private int maxAmmo = 5;
    [SerializeField] private int currentAmmo = 5;
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float reloadDuration = 3.5f;
    [SerializeField] private Transform weaponSpawnPoint;
    [SerializeField] private EnemyWeapon enemyWeapon;

    [Header("Debug")]
    [SerializeField] private string lastDecision = "Ninguna";
    [SerializeField] private float currentReloadTimer = 0f;

    public IState AttackState { get; private set; }
    public IState SeekWeaponState { get; private set; }
    public IState ReloadState { get; private set; }

    public bool HasWeapon => hasWeapon;
    public IReloadableWeapon EquippedWeapon => enemyWeapon;
    public Transform WeaponSpawnPoint => weaponSpawnPoint;
    public int CurrentAmmo => enemyWeapon != null ? enemyWeapon.CurrentAmmo : currentAmmo;

    // Nodo raiz del arbol
    private IDecisionNode rootNode;
    // Efecto visual al atacar
    private IAttackEffect attackEffect;
    private int lastSyncedInspectorAmmo;

    // Estado de Ataque y visual
    protected override void InitializeStates()
    {
        base.InitializeStates();

        attackEffect = GetComponent<IAttackEffect>();

        if (attackEffect == null)
        {
            Debug.LogError($"{name}: falta un componente que implemente IAttackEffect (por ejemplo AttackVisualEffect).", this);
        }

        EnsureWeaponComponent();

        AttackState = new EnemyAttackState(this, OnEnemyAttacked, attackCooldown);
        SeekWeaponState = new EnemySeekWeaponState(this, this);
        ReloadState = new EnemyReloadState(this, this);
    }

    private void EnsureWeaponComponent()
    {
        if (enemyWeapon == null)
        {
            enemyWeapon = GetComponent<EnemyWeapon>();
            if (enemyWeapon == null)
            {
                enemyWeapon = gameObject.AddComponent<EnemyWeapon>();
            }
        }

        enemyWeapon.Configure(maxAmmo, currentAmmo, attackDistance, reloadDuration, attackEffect);
        enemyWeapon.SetEquippedVisual(hasWeapon);
        lastSyncedInspectorAmmo = currentAmmo;
    }

    protected override void Start()
    {
        BuildDecisionTree();
        base.Start();
    }

    protected override void Update()
    {
        SyncWeaponWithInspector();
        //Arbol en cada frame
        rootNode?.Execute();
        base.Update();
        SyncInspectorFromWeapon();
    }

    private void SyncWeaponWithInspector()
    {
        if (enemyWeapon == null) return;

        // Permite modificar currentAmmo o maxAmmo en vivo desde el Inspector durante Play Mode para testear el árbol
        if (currentAmmo != lastSyncedInspectorAmmo)
        {
            enemyWeapon.SetAmmo(currentAmmo, maxAmmo);
            lastSyncedInspectorAmmo = enemyWeapon.CurrentAmmo;
        }

        enemyWeapon.SetEquippedVisual(hasWeapon);
    }

    private void SyncInspectorFromWeapon()
    {
        if (enemyWeapon == null) return;

        currentAmmo = enemyWeapon.CurrentAmmo;
        maxAmmo = enemyWeapon.MaxAmmo;
        currentReloadTimer = enemyWeapon.ReloadTimer;
        lastSyncedInspectorAmmo = currentAmmo;
    }

    // Preguntas del Arbol de abajo hacia atras
    private void BuildDecisionTree()
    {
        var accionAtacar = new DecisionActionNode(() => SetDecisionState(AttackState, "Atacar"));
        var accionAcercarme = new DecisionActionNode(() => SetDecisionState(ChaseState, "Acercarme"));
        var accionSeguirPatrullando = new DecisionActionNode(DecisionSeguirPatrullando);
        var accionHuirRecargar = new DecisionActionNode(DecisionHuirYRecargar);
        var accionBuscarArma = new DecisionActionNode(DecisionBuscarArma);

        // Direccion del arbol
        var preguntaEstoyCerca = new DecisionQuestionNode(IsCloseToPlayer, accionAtacar, accionAcercarme);
        var preguntaVeoAlJugador = new DecisionQuestionNode(ShouldEngagePlayer, preguntaEstoyCerca, accionSeguirPatrullando);
        var preguntaTengoMunicion = new DecisionQuestionNode(() => CurrentAmmo > 0, preguntaVeoAlJugador, accionHuirRecargar);
        var preguntaTengoArma = new DecisionQuestionNode(() => hasWeapon, preguntaTengoMunicion, accionBuscarArma);

        // Arranca aca
        rootNode = preguntaTengoArma;
    }

    private bool ShouldEngagePlayer()
    {
        if (Vision == null || Vision.Target == null) return false;

        // Si lo ve actualmente en el cono de visión, entra/mantiene persecución o ataque
        if (Vision.CanSeeTarget) return true;

        // Si ya lo había detectado y está buscándolo/persiguiéndolo con Pathfinding (Theta*) o saliendo de ataque,
        // mantenemos la rama activa hasta que EnemyChaseState agote su búsqueda y vuelva a PatrolState.
        return StateMachine != null &&
               (StateMachine.CurrentState == ChaseState || StateMachine.CurrentState == AttackState);
    }

    private bool IsCloseToPlayer()
    {
        Transform target = Vision?.Target;
        if (target == null) return false;

        float effectiveAttackRange = enemyWeapon != null ? enemyWeapon.AttackRange : attackDistance;
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > (effectiveAttackRange * effectiveAttackRange)) return false;

        return Vision.CanSeePosition(target.position);
    }

    // Cambio state segun arbol
    private void SetDecisionState(IState state, string decisionName)
    {
        lastDecision = decisionName;
        enemyWeapon?.CancelReload();
        currentReloadTimer = 0f;

        if (StateMachine.CurrentState != state)
        {
            StateMachine.ChangeState(state);
        }
    }

    private void DecisionSeguirPatrullando()
    {
        lastDecision = (StateMachine.CurrentState == IdleState) ? "En reposo / guardia" : "Seguir patrullando";
        enemyWeapon?.CancelReload();
        currentReloadTimer = 0f;

        if (StateMachine.CurrentState != PatrolState && StateMachine.CurrentState != IdleState)
        {
            StateMachine.ChangeState(PatrolState);
        }
    }

    private void DecisionHuirYRecargar()
    {
        if (StateMachine.CurrentState != ReloadState)
        {
            StateMachine.ChangeState(ReloadState);
        }

        float duration = enemyWeapon != null ? enemyWeapon.ReloadDuration : reloadDuration;
        float timer = enemyWeapon != null ? enemyWeapon.ReloadTimer : currentReloadTimer;
        lastDecision = $"Huyendo y Recargando ({timer:F1}/{duration:F1}s)";
    }

    private void DecisionBuscarArma()
    {
        enemyWeapon?.CancelReload();
        currentReloadTimer = 0f;
        lastDecision = "Buscando arma";

        if (StateMachine.CurrentState != SeekWeaponState)
        {
            StateMachine.ChangeState(SeekWeaponState);
        }
    }

    public void EquipWeapon()
    {
        hasWeapon = true;
        if (enemyWeapon != null)
        {
            enemyWeapon.RefillImmediate();
            enemyWeapon.SetEquippedVisual(true);
            currentAmmo = enemyWeapon.CurrentAmmo;
            lastSyncedInspectorAmmo = currentAmmo;
        }
        else
        {
            currentAmmo = maxAmmo;
            lastSyncedInspectorAmmo = currentAmmo;
        }
    }

    // Gasto un golpe al atacar (se cansa de tanto pegar el type)
    private void OnEnemyAttacked()
    {
        if (enemyWeapon != null)
        {
            enemyWeapon.TryAttack(transform, Vision?.Target);
            currentAmmo = enemyWeapon.CurrentAmmo;
            lastSyncedInspectorAmmo = currentAmmo;
        }
        else
        {
            if (currentAmmo > 0) currentAmmo--;
            attackEffect?.TriggerEffect();
        }
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

