using UnityEngine;

public class EnemyPatrolState : IState
{
    private readonly IEnemyContext context;
    private int currentIndex;

    public EnemyPatrolState(IEnemyContext context)
    {
        this.context = context;
    }

    public void Enter()
    {
        currentIndex = context.GetClosestWaypointIndex();
    }

    public void Update()
    {
        if (context.Vision != null && context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.ChaseState);
            return;
        }

        if (context.Waypoints == null || context.Waypoints.Length == 0)
        {
            context.Mover.Move(Vector3.zero, 0f);
            return;
        }

        Transform targetWaypoint = context.Waypoints[currentIndex];
        if (targetWaypoint == null)
        {
            context.Mover.Move(Vector3.zero, 0f);
            return;
        }

        Vector3 toWaypoint = targetWaypoint.position - context.Transform.position;
        toWaypoint.y = 0f;

        if (toWaypoint.sqrMagnitude <= context.WaypointThreshold * context.WaypointThreshold)
        {
            currentIndex = (currentIndex + 1) % context.Waypoints.Length;
            return;
        }

        Vector3 moveDirection = toWaypoint.normalized;
        context.Mover.Move(moveDirection, context.PatrolSpeed);
        context.Mover.Rotate(moveDirection);
    }

    public void Exit()
    {
        context.Mover.Move(Vector3.zero, 0f);
    }
}
