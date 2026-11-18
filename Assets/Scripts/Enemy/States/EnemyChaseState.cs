using UnityEngine;

public class EnemyChaseState : IState, IDescribedState
{
    private readonly IEnemyContext context;
    private readonly Pursuit pursuitSteering;
    private float loseTimer;
    private float searchSafetyTimer;
    private Vector3 lastKnownTargetPosition;
    private bool hasLastKnownPosition;
    private const float MinDistanceToTarget = 0.5f;
    private const float MaxPredictionTime = 1.5f;
    private const float MaxPathSearchDuration = 12f;

    public EnemyChaseState(IEnemyContext context)
    {
        this.context = context;
        this.pursuitSteering = new Pursuit(context.Transform, context.Vision?.Target, MaxPredictionTime, context.ChaseSpeed);
    }

    public string StatusLabel => context.Vision != null && context.Vision.CanSeeTarget
        ? "PERSECUCIÓN · PURSUIT"
        : StateLabelFormatter.WithNavigation("BUSCANDO", context.PathNavigator);
    public NpcAlertLevel AlertLevel => context.Vision != null && context.Vision.CanSeeTarget ? NpcAlertLevel.Hostile : NpcAlertLevel.Suspicious;

    public void Enter()
    {
        loseTimer = 0f;
        searchSafetyTimer = 0f;
        Transform target = context.Vision?.Target;
        if (target != null)
        {
            lastKnownTargetPosition = target.position;
            hasLastKnownPosition = true;
        }
        else
        {
            hasLastKnownPosition = false;
        }

        pursuitSteering.TargetTransform = target;
        pursuitSteering.ReferenceSpeed = context.ChaseSpeed;
        pursuitSteering.ResetTracking();
        context.PathNavigator?.ClearPath();
    }

    public void Update()
    {
        Transform target = context.Vision?.Target;
        if (target == null)
        {
            context.StateMachine.ChangeState(context.PatrolState);
            return;
        }

        Vector3 toTarget = target.position - context.Transform.position;
        toTarget.y = 0f;
        float distanceToTarget = toTarget.magnitude;

        // Mientras ya está en alerta (ChaseState), si lo ve en el cono O si tiene línea de visión
        // despejada dentro del rango visual (aunque haya girado para esquivar o doblar una esquina),
        // sigue percibiendo al objetivo sin perderlo.
        bool canSeeInCone = context.Vision.CanSeeTarget;
        bool hasDirectLoSInRange = distanceToTarget <= context.Vision.VisionDistance &&
                                   context.Vision.CanSeePosition(target.position);

        if (canSeeInCone || hasDirectLoSInRange)
        {
            loseTimer = 0f;
            searchSafetyTimer = 0f;
            lastKnownTargetPosition = target.position;
            hasLastKnownPosition = true;
        }
        else
        {
            loseTimer += Time.deltaTime;
            searchSafetyTimer += Time.deltaTime;

            // Durante el tiempo de memoria (LoseTargetTime) sigue actualizando la posición objetivo
            // para que Theta* arme el camino hacia donde se escondió el jugador.
            if (loseTimer < context.LoseTargetTime || !hasLastKnownPosition)
            {
                lastKnownTargetPosition = target.position;
                hasLastKnownPosition = true;
            }
        }

        pursuitSteering.TargetTransform = target;
        pursuitSteering.ReferenceSpeed = context.ChaseSpeed;

        Vector3 navigationGoal = (canSeeInCone || hasDirectLoSInRange) ? target.position : lastKnownTargetPosition;
        Vector3 toGoal = navigationGoal - context.Transform.position;
        toGoal.y = 0f;
        float distanceToGoal = toGoal.magnitude;

        // Si ya pasó el tiempo de pérdida y no lo ve:
        // solo deja de buscar cuando terminó de recorrer el camino de Pathfinding (Theta*) hacia la última posición conocida
        // o si se superó el tiempo máximo de seguridad.
        if (loseTimer >= context.LoseTargetTime)
        {
            bool reachedLastKnownPos = distanceToGoal <= Mathf.Max(context.WaypointThreshold * 1.5f, MinDistanceToTarget);
            bool stillFollowingPath = context.PathNavigator != null && context.PathNavigator.IsFollowingPath;

            if (reachedLastKnownPos || !stillFollowingPath || searchSafetyTimer >= MaxPathSearchDuration)
            {
                context.StateMachine.ChangeState(context.PatrolState);
                return;
            }
        }

        if (distanceToGoal > MinDistanceToTarget)
        {
            Vector3 moveDirection;

            // Regla TP2: Si tiene línea de visión con el objetivo usa Steering (Pursuit + Avoidance);
            // en caso contrario pasa a utilizar Pathfinding (A* / Theta* + Avoidance).
            if (context.PathNavigator != null)
            {
                ISteering steeringToUse = (canSeeInCone || hasDirectLoSInRange) ? pursuitSteering : null;
                moveDirection = context.PathNavigator.GetMoveDirection(navigationGoal, steeringToUse);
            }
            else
            {
                Vector3 desiredDirection = pursuitSteering.GetDir();
                moveDirection = context.Avoidance != null
                    ? context.Avoidance.GetSteeredDirection(desiredDirection)
                    : desiredDirection;
            }

            // Si después de calcular el camino sigue sin haber ruta ni línea de visión y ya expiró el timer, volver a patrullar
            if (loseTimer >= context.LoseTargetTime && context.PathNavigator != null && !context.PathNavigator.IsFollowingPath && !hasDirectLoSInRange)
            {
                context.StateMachine.ChangeState(context.PatrolState);
                return;
            }

            context.Mover.Move(moveDirection, context.ChaseSpeed);
            context.Mover.Rotate(moveDirection);
        }
        else
        {
            context.Mover.Move(Vector3.zero, 0f);
            if (toTarget != Vector3.zero)
            {
                context.Mover.Rotate(toTarget.normalized);
            }
        }
    }

    public void Exit()
    {
        loseTimer = 0f;
        searchSafetyTimer = 0f;
        context.PathNavigator?.ClearPath();
        context.Mover.Move(Vector3.zero, 0f);
    }
}
