using UnityEngine;

public class EnemyPatrolState : IState, IDescribedState
{
    private readonly IEnemyContext context;
    private readonly IPatrolRouteStrategy routeStrategy;
    private readonly Seek seekSteering;
    private int currentIndex = -1;

    public EnemyPatrolState(IEnemyContext context, IPatrolRouteStrategy routeStrategy = null)
    {
        this.context = context;
        this.routeStrategy = routeStrategy ?? new PingPongPatrolStrategy();
        this.seekSteering = new Seek(context.Transform);
    }

    public string StatusLabel => StateLabelFormatter.WithNavigation("PATRULLA", context.PathNavigator);
    public NpcAlertLevel AlertLevel => NpcAlertLevel.Calm;

    public void Enter()
    {
        context.PathNavigator?.ClearPath();
        if (context.Waypoints == null || context.Waypoints.Length == 0) return;

        if (currentIndex < 0 || currentIndex >= context.Waypoints.Length)
        {
            routeStrategy.Reset();
            int closestIndex = context.GetClosestWaypointIndex();
            currentIndex = routeStrategy.GetInitialIndex(
                context.Transform,
                context.Waypoints,
                closestIndex,
                context.WaypointThreshold);
        }
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

        if (currentIndex < 0 || currentIndex >= context.Waypoints.Length)
        {
            currentIndex = 0;
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
                ApplyWaypointOrientationIfConfigured(targetWaypoint);

                PatrolStepResult step = routeStrategy.GetNextWaypoint(currentIndex, context.Waypoints);
                currentIndex = step.NextIndex;

                if (step.ShouldPauseForIdle && context.IdleState != null)
                {
                    context.StateMachine.ChangeState(context.IdleState);
                    return;
                }
            }
            return;
        }

        seekSteering.Target = targetWaypoint;

        Vector3 moveDirection;
        if (context.PathNavigator != null)
        {
            moveDirection = context.PathNavigator.GetMoveDirection(targetWaypoint.position, seekSteering);
        }
        else
        {
            Vector3 desiredDirection = seekSteering.GetDir();
            moveDirection = context.Avoidance != null
                ? context.Avoidance.GetSteeredDirection(desiredDirection)
                : desiredDirection;
        }

        context.Mover.Move(moveDirection, context.PatrolSpeed);
        context.Mover.Rotate(moveDirection);
    }

    public void Exit()
    {
        context.PathNavigator?.ClearPath();
        context.Mover.Move(Vector3.zero, 0f);
    }

    private void ApplyWaypointOrientationIfConfigured(Transform reachedWaypoint)
    {
        if (reachedWaypoint == null) return;

        PatrolPointMetadata metadata = reachedWaypoint.GetComponent<PatrolPointMetadata>();
        if (metadata == null || !metadata.OrientToWaypointForward) return;

        Vector3 lookDir = reachedWaypoint.forward;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            context.Transform.rotation = Quaternion.LookRotation(lookDir.normalized);
        }
    }
}
