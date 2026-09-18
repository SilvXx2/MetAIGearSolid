using UnityEngine;

public class EnemyPatrolState : IState
{
    private readonly IEnemyContext context;
    private int currentIndex;

    private int patrolDirection = 1;

    public EnemyPatrolState(IEnemyContext context)
    {
        this.context = context;
    }

    public void Enter()
    {
        currentIndex = context.GetClosestWaypointIndex();
        patrolDirection = 1;
    }

    public void Update()
    {
        if (context.Vision != null && context.Vision.CanSeeTarget)
        {
            context.StateMachine.ChangeState(context.AlertState);
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
            if (context.Waypoints.Length > 1)
            {
                if (currentIndex >= context.Waypoints.Length - 1)
                {
                    patrolDirection = -1;
                }
                else if (currentIndex <= 0)
                {
                    patrolDirection = 1;
                }

                currentIndex += patrolDirection;
            }
            return;
        }

        Vector3 desiredDirection = toWaypoint.normalized;
        Vector3 moveDirection = context.Avoidance != null
            ? context.Avoidance.GetSteeredDirection(desiredDirection)
            : desiredDirection;

        context.Mover.Move(moveDirection, context.PatrolSpeed);
        context.Mover.Rotate(moveDirection);
    }

    public void Exit()
    {
        context.Mover.Move(Vector3.zero, 0f);
    }
}
