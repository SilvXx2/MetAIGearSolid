using UnityEngine;

public class LoopPatrolStrategy : IPatrolRouteStrategy
{
    private readonly int pauseEveryNWaypoints;
    private int visitedCount;

    public LoopPatrolStrategy(int pauseEveryNWaypoints = 2)
    {
        this.pauseEveryNWaypoints = Mathf.Max(0, pauseEveryNWaypoints);
    }

    public void Reset()
    {
        visitedCount = 0;
    }

    public int GetInitialIndex(Transform agent, Transform[] waypoints, int closestIndex, float waypointThreshold)
    {
        if (waypoints == null || waypoints.Length == 0) return -1;

        int index = Mathf.Clamp(closestIndex, 0, waypoints.Length - 1);
        if (waypoints.Length > 1 && waypoints[index] != null && agent != null)
        {
            Vector3 toWp = waypoints[index].position - agent.position;
            toWp.y = 0f;
            if (toWp.sqrMagnitude <= waypointThreshold * waypointThreshold)
            {
                index = (index + 1) % waypoints.Length;
            }
        }

        return index;
    }

    public PatrolStepResult GetNextWaypoint(int currentIndex, Transform[] waypoints)
    {
        if (waypoints == null || waypoints.Length <= 1)
        {
            return new PatrolStepResult(Mathf.Max(0, currentIndex), false);
        }

        visitedCount++;
        bool periodicPause = pauseEveryNWaypoints > 0 && (visitedCount % pauseEveryNWaypoints == 0);
        bool metadataPause = HasPauseMetadata(waypoints[currentIndex]);
        int nextIndex = (currentIndex + 1) % waypoints.Length;

        return new PatrolStepResult(nextIndex, periodicPause || metadataPause);
    }

    private static bool HasPauseMetadata(Transform waypoint)
    {
        if (waypoint == null) return false;
        PatrolPointMetadata meta = waypoint.GetComponent<PatrolPointMetadata>();
        return meta != null && meta.PauseHere;
    }
}
