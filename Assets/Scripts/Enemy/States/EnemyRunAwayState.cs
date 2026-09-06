using UnityEngine;

public class EnemyRunAwayState : IState
{
    private readonly IEnemyContext context;
    private readonly float safeDistance;
    private float loseTimer;
    private Vector3 lastTargetPos;
    private Vector3 targetVelocity;
    private const float MaxPredictionTime = 1.5f;

    public EnemyRunAwayState(IEnemyContext context, float safeDistance = 12f)
    {
        this.context = context;
        this.safeDistance = safeDistance;
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
                context.StateMachine.ChangeState(context.PatrolState);
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

        if (distance >= safeDistance)
        {
            context.StateMachine.ChangeState(context.PatrolState);
            return;
        }

        float predictionTime = Mathf.Clamp(distance / Mathf.Max(context.ChaseSpeed, 0.1f), 0.1f, MaxPredictionTime);
        Vector3 futurePosition = target.position + targetVelocity * predictionTime;
        futurePosition.y = context.Transform.position.y;

        Vector3 desiredDirection = (context.Transform.position - futurePosition).normalized;

        Vector3 moveDirection = context.Avoidance != null
            ? context.Avoidance.GetSteeredDirection(desiredDirection)
            : desiredDirection;

        context.Mover.Move(moveDirection, context.ChaseSpeed);
        context.Mover.Rotate(moveDirection);
    }

    public void Exit()
    {
        loseTimer = 0f;
        context.Mover.Move(Vector3.zero, 0f);
    }
}
