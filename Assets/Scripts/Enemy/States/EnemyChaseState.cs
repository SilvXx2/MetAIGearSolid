using UnityEngine;

public class EnemyChaseState : IState
{
    private readonly IEnemyContext context;
    private float loseTimer;
    private Vector3 lastTargetPos;
    private Vector3 targetVelocity;
    private const float MinDistanceToTarget = 0.5f;
    private const float MaxPredictionTime = 1.5f;

    public EnemyChaseState(IEnemyContext context)
    {
        this.context = context;
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
            lastTargetPos = target.position;
        }

        Vector3 toTarget = target.position - context.Transform.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance > MinDistanceToTarget)
        {
            float predictionTime = Mathf.Clamp(distance / Mathf.Max(context.ChaseSpeed, 0.1f), 0.1f, MaxPredictionTime);
            Vector3 futurePosition = target.position + targetVelocity * predictionTime;
            futurePosition.y = context.Transform.position.y;

            Vector3 moveDirection = (futurePosition - context.Transform.position).normalized;
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
        context.Mover.Move(Vector3.zero, 0f);
    }
}
