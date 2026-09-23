using UnityEngine;

/// <summary>
/// Estado RunAway (Huida) que implementa un Steering Behaviour Complejo según Craig Reynolds (Red3D GDC 99).
/// 
/// JUSTIFICACIÓN TEÓRICA Y CONSIGNAS:
/// ----------------------------------
/// Consigna: "RunAway: El NPC deberá huir con un steering behaviour complejo (Flee / Evade)."
/// Consigna: "Todo movimiento que realice la IA, deberá aplicar Obstacle avoidance."
/// Consigna: "No se aceptan steering behaviours simples tales como dirigirse directamente hacia el objetivo."
/// 
/// IMPLEMENTACIÓN COMPLEJA DE REYNOLDS:
/// 1. Hide: Búsqueda activa de cobertura detrás de obstáculos con proyección de sombra geométrica.
/// 2. Arrival: Desaceleración proporcional al alcanzar la cobertura para no quedar expuesto al otro lado.
/// 3. Obstacle Avoidance: Rodea obstáculos durante la trayectoria hacia la cobertura.
/// 4. Separation: Fuerza repulsiva de grupo (1/r) para evitar que múltiples unidades colisionen entre sí.
/// 5. Fallback a Evade predictivo si no existen obstáculos próximos.
/// </summary>
public class EnemyRunAwayState : IState
{
    private readonly IEnemyContext context;
    private readonly float safeDistance;
    private readonly HideSteering hideSteering;
    private readonly SeparationSteering separationSteering;

    private float loseTimer;
    private Vector3 lastTargetPos;
    private Vector3 targetVelocity;

    public HideSteering HideBehavior => hideSteering;

    public EnemyRunAwayState(IEnemyContext context, float safeDistance = 12f, LayerMask obstacleMask = default)
    {
        this.context = context;
        this.safeDistance = safeDistance;

        LayerMask mask = (obstacleMask.value != 0)
            ? obstacleMask
            : (context.Avoidance != null ? context.Avoidance.ObstacleMask : LayerMask.GetMask("Default"));

        hideSteering = new HideSteering(
            agentTransform: context.Transform,
            obstacleMask: mask,
            scanRadius: 16f,
            coverOffset: 1.5f,
            slowingDistance: 3f,
            arrivalThreshold: 0.8f);

        separationSteering = new SeparationSteering(
            agentTransform: context.Transform,
            separationRadius: 2.0f);
    }

    public void Enter()
    {
        loseTimer = 0f;
        if (context.Vision?.Target != null)
        {
            lastTargetPos = context.Vision.Target.position;
            targetVelocity = Vector3.zero;
        }
    }

    public void Update()
    {
        Transform target = context.Vision?.Target;
        if (target == null)
        {
            context.StateMachine.ChangeState(context.PatrolState);
            return;
        }

        // Chequeo de línea de visión: si ya no lo ve, temporizador para salir del estado de alerta
        if (context.Vision.CanSeeTarget)
        {
            loseTimer = 0f;
        }
        else
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= context.LoseTargetTime)
            {
                // Regresa a la rutina de patrulla o reposo
                context.StateMachine.ChangeState(context.IdleState ?? context.PatrolState);
                return;
            }
        }

        // Estimación de la velocidad del jugador para predicción de interceptación
        if (Time.deltaTime > 0f)
        {
            targetVelocity = (target.position - lastTargetPos) / Time.deltaTime;
            targetVelocity.y = 0f;
            lastTargetPos = target.position;
        }

        Vector3 toTarget = target.position - context.Transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        // Si ya alcanzó la distancia segura y no lo ve directamente, se calma
        if (distance >= safeDistance && !context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.IdleState ?? context.PatrolState);
            return;
        }

        // 1. Cálculo del vector de Hide (Ocultamiento + Arrival + Avoidance)
        Vector3 hideVelocity = hideSteering.CalculateHideDirection(
            targetPosition: target.position,
            targetVelocity: targetVelocity,
            agentSpeed: context.ChaseSpeed,
            avoidanceSensor: context.Avoidance);

        // 2. Fuerza de Separación (evita solapamiento con otros enemigos)
        Vector3 separationForce = separationSteering.CalculateSeparationForce();

        // 3. Mezcla ponderada de comportamientos (Reynolds Behavioral Blending)
        Vector3 combinedDirection = hideVelocity;
        if (separationForce != Vector3.zero)
        {
            combinedDirection = (combinedDirection + separationForce * 1.2f).normalized * Mathf.Max(hideVelocity.magnitude, 0.4f);
        }

        float currentSpeed = context.ChaseSpeed * combinedDirection.magnitude;

        if (combinedDirection != Vector3.zero && currentSpeed > 0.05f)
        {
            Vector3 moveDir = combinedDirection.normalized;
            context.Mover.Move(moveDir, currentSpeed);
            context.Mover.Rotate(moveDir);
        }
        else
        {
            // El agente ha alcanzado exitosamente la cobertura oculta
            context.Mover.Move(Vector3.zero, 0f);

            // Orientarse de espaldas al obstáculo (vigilando hacia el exterior)
            if (toTarget.sqrMagnitude > 0.001f)
            {
                context.Mover.Rotate(-toTarget.normalized);
            }
        }
    }

    public void Exit()
    {
        loseTimer = 0f;
        context.Mover.Move(Vector3.zero, 0f);
    }
}
