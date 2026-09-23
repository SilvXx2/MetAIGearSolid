using UnityEngine;

public class EnemyPatrolState : IState
{
    private readonly IEnemyContext context;
    private int currentIndex = -1;
    private int patrolDirection = 1;

    public EnemyPatrolState(IEnemyContext context)
    {
        this.context = context;
    }

    public void Enter()
    {
        if (context.Waypoints == null || context.Waypoints.Length == 0) return;

        // Solo recalcular waypoint si no está inicializado o está fuera de rango
        if (currentIndex < 0 || currentIndex >= context.Waypoints.Length)
        {
            currentIndex = context.GetClosestWaypointIndex();
            patrolDirection = 1;

            // Si el enemigo ya está sobre el waypoint más cercano, avanzar al siguiente para que no se quede quieto
            if (context.Waypoints.Length > 1 && context.Waypoints[currentIndex] != null)
            {
                Vector3 toWp = context.Waypoints[currentIndex].position - context.Transform.position;
                toWp.y = 0f;
                if (toWp.sqrMagnitude <= context.WaypointThreshold * context.WaypointThreshold)
                {
                    if (currentIndex >= context.Waypoints.Length - 1)
                    {
                        patrolDirection = -1;
                    }
                    else
                    {
                        patrolDirection = 1;
                    }
                    currentIndex += patrolDirection;
                }
            }
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
                bool reachedEndpoint = false;

                if (currentIndex >= context.Waypoints.Length - 1)
                {
                    patrolDirection = -1;
                    reachedEndpoint = true;
                }
                else if (currentIndex <= 0)
                {
                    patrolDirection = 1;
                    reachedEndpoint = true;
                }

                currentIndex += patrolDirection;

                // Al completar el tramo de patrulla, transiciona a IdleState (con Ruleta de Selección)
                if (reachedEndpoint && context.IdleState != null)
                {
                    context.StateMachine.ChangeState(context.IdleState);
                    return;
                }
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
