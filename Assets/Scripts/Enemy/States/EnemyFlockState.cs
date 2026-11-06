using UnityEngine;

public class EnemyFlockState : IState
{
    private readonly IFlockContext context;
    private readonly Seek waypointSeek;
    private int currentWaypointIndex;

    public EnemyFlockState(IFlockContext context)
    {
        this.context = context;
        this.waypointSeek = new Seek(context.Transform);
    }

    public void Enter()
    {
        context.PathNavigator?.ClearPath();
        if (context.Waypoints != null && context.Waypoints.Length > 0)
        {
            currentWaypointIndex = context.GetClosestWaypointIndex();
        }
    }

    public void Update()
    {
        // Si detecta al jugador en su Line of Sight, pasa al estado de alerta (Huir o Perseguir)
        if (context.Vision != null && context.Vision.CanSeeTarget && context.AlertState != null)
        {
            context.StateMachine.ChangeState(context.AlertState);
            return;
        }

        Vector3 flockDir = context.FlockingSteering != null
            ? context.FlockingSteering.GetDir()
            : Vector3.zero;

        // Modo 1: Leader Following (Separation + Cohesion + Arrive al Líder)
        if (context.Leader != null)
        {
            UpdateLeaderFollowing(flockDir);
            return;
        }

        // Modo 2: Flocking Común (Separation + Cohesion + Alignment), con ruta opcional o deriva frontal
        UpdateStandardFlocking(flockDir);
    }

    private void UpdateLeaderFollowing(Vector3 flockDir)
    {
        Vector3 leaderPos = context.Leader.position;
        Vector3 toLeader = leaderPos - context.Transform.position;
        toLeader.y = 0f;
        float distanceToLeader = toLeader.magnitude;
        Vector3 dirToLeader = distanceToLeader > 0.001f ? toLeader / distanceToLeader : Vector3.zero;

        // Regla TP2: Si una pared corta la línea de visión con el líder, usa Pathfinding (A* / Theta*)
        bool hasLoSToLeader = context.PathNavigator == null || context.PathNavigator.HasLineOfSightTo(leaderPos);
        if (!hasLoSToLeader && context.PathNavigator != null)
        {
            Vector3 pathDir = context.PathNavigator.GetMoveDirection(leaderPos);
            Vector3 combinedPathAndFlock = (pathDir + flockDir * 0.35f).normalized;
            if (combinedPathAndFlock == Vector3.zero)
            {
                combinedPathAndFlock = dirToLeader;
            }

            Vector3 steeredPathDir = context.Avoidance != null
                ? context.Avoidance.GetSteeredDirection(combinedPathAndFlock)
                : combinedPathAndFlock;

            context.Mover.Move(steeredPathDir, context.PatrolSpeed);
            context.Mover.Rotate(steeredPathDir);
            return;
        }

        context.PathNavigator?.ClearPath();

        // Si ya está dentro de la distancia de parada del líder, desacelera y se detiene (Arrive)
        if (distanceToLeader <= context.LeaderStoppingDistance)
        {
            context.Mover.Move(Vector3.zero, 0f);
            context.Mover.Rotate(context.Leader.forward);
            return;
        }

        // Combinamos el vector de Flocking (Separation + Cohesion + LeaderBehaviour) con atracción directa al líder
        Vector3 desiredDir = flockDir != Vector3.zero
            ? (flockDir + dirToLeader * 0.85f).normalized
            : dirToLeader;

        if (desiredDir == Vector3.zero)
        {
            desiredDir = dirToLeader;
        }

        Vector3 moveDirection = context.Avoidance != null
            ? context.Avoidance.GetSteeredDirection(desiredDir)
            : desiredDir;

        float slowRadius = Mathf.Max(context.LeaderStoppingDistance * 2.0f, 3.0f);
        float speedFactor = distanceToLeader < slowRadius
            ? Mathf.Clamp((distanceToLeader - context.LeaderStoppingDistance * 0.5f) / slowRadius, 0.35f, 1f)
            : 1f;

        context.Mover.Move(moveDirection, context.PatrolSpeed * speedFactor);
        context.Mover.Rotate(moveDirection);
    }

    private void UpdateStandardFlocking(Vector3 flockDir)
    {
        Vector3 baseDir = Vector3.zero;

        if (context.Waypoints != null && context.Waypoints.Length > 0)
        {
            Transform wp = context.Waypoints[currentWaypointIndex];
            if (wp != null)
            {
                Vector3 toWp = wp.position - context.Transform.position;
                toWp.y = 0f;
                if (toWp.sqrMagnitude <= context.WaypointThreshold * context.WaypointThreshold)
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % context.Waypoints.Length;
                    wp = context.Waypoints[currentWaypointIndex];
                }

                if (wp != null)
                {
                    waypointSeek.Target = wp;
                    baseDir = context.PathNavigator != null
                        ? context.PathNavigator.GetMoveDirection(wp.position, waypointSeek)
                        : waypointSeek.GetDir();
                }
            }
        }
        else
        {
            baseDir = context.Transform.forward;
            baseDir.y = 0f;
            baseDir.Normalize();
        }

        Vector3 combinedDir = (flockDir + baseDir * 0.5f).normalized;
        if (combinedDir == Vector3.zero)
        {
            combinedDir = context.Transform.forward;
        }

        Vector3 moveDirection = context.Avoidance != null
            ? context.Avoidance.GetSteeredDirection(combinedDir)
            : combinedDir;

        context.Mover.Move(moveDirection, context.PatrolSpeed);
        context.Mover.Rotate(moveDirection);
    }

    public void Exit()
    {
        context.PathNavigator?.ClearPath();
        context.Mover.Move(Vector3.zero, 0f);
    }
}

