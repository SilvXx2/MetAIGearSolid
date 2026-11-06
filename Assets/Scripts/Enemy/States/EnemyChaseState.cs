using UnityEngine;

public class EnemyChaseState : IState
{
    private readonly IEnemyContext context;
    private readonly Pursuit pursuitSteering;
    private float loseTimer;
    private const float MinDistanceToTarget = 0.5f;
    private const float MaxPredictionTime = 1.5f;

    public EnemyChaseState(IEnemyContext context)
    {
        this.context = context;
        this.pursuitSteering = new Pursuit(context.Transform, context.Vision?.Target, MaxPredictionTime, context.ChaseSpeed);
    }

    public void Enter()
    {
        loseTimer = 0f;
        pursuitSteering.TargetTransform = context.Vision?.Target;
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

        if (context.Vision.CanSeeTarget)
        {
            loseTimer = 0f;
        }
        else
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= context.LoseTargetTime)
            {
                context.StateMachine.ChangeState(context.PatrolState);
                return;
            }
        }

        pursuitSteering.TargetTransform = target;
        pursuitSteering.ReferenceSpeed = context.ChaseSpeed;

        Vector3 toTarget = target.position - context.Transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance > MinDistanceToTarget)
        {
            Vector3 moveDirection;

            // Regla TP2: Si tiene línea de visión con el objetivo usa Steering (Pursuit + Avoidance);
            // en caso contrario pasa a utilizar Pathfinding (A* / Theta* + Avoidance).
            if (context.PathNavigator != null)
            {
                moveDirection = context.PathNavigator.GetMoveDirection(target.position, pursuitSteering);
            }
            else
            {
                Vector3 desiredDirection = pursuitSteering.GetDir();
                moveDirection = context.Avoidance != null
                    ? context.Avoidance.GetSteeredDirection(desiredDirection)
                    : desiredDirection;
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
        context.PathNavigator?.ClearPath();
        context.Mover.Move(Vector3.zero, 0f);
    }
}
