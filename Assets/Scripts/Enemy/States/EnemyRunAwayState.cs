using UnityEngine;

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
            : (context.Avoidance != null ? context.Avoidance.ObstacleMask : LayerMask.GetMask("Enemy"));

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

        if (context.Vision.CanSeeTarget)
        {
            loseTimer = 0f;
        }
        else
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= context.LoseTargetTime)
            {
                context.StateMachine.ChangeState(context.IdleState ?? context.PatrolState);
                return;
            }
        }

        if (Time.deltaTime > 0f)
        {
            targetVelocity = (target.position - lastTargetPos) / Time.deltaTime;
            targetVelocity.y = 0f;
            lastTargetPos = target.position;
        }

        Vector3 toTarget = target.position - context.Transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance >= safeDistance && !context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.IdleState ?? context.PatrolState);
            return;
        }

        Vector3 hideVelocity = hideSteering.CalculateHideDirection(
            targetPosition: target.position,
            targetVelocity: targetVelocity,
            agentSpeed: context.ChaseSpeed,
            avoidanceSensor: context.Avoidance);

        Vector3 separationForce = separationSteering.CalculateSeparationForce();

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
            context.Mover.Move(Vector3.zero, 0f);

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
