using UnityEngine;

public class PingPongPatrolStrategy : IPatrolRouteStrategy
{
    private int direction = 1;

    public void Reset()
    {
        direction = 1;
    }

    public int GetInitialIndex(Transform agent, Transform[] waypoints, int closestIndex, float waypointThreshold)
    {
        if (waypoints == null || waypoints.Length == 0) return -1;

        direction = 1;
        int index = Mathf.Clamp(closestIndex, 0, waypoints.Length - 1);

        if (waypoints.Length > 1 && waypoints[index] != null && agent != null)
        {
            Vector3 toWp = waypoints[index].position - agent.position;
            toWp.y = 0f;
            if (toWp.sqrMagnitude <= waypointThreshold * waypointThreshold)
            {
                direction = index >= waypoints.Length - 1 ? -1 : 1;
                index += direction;
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

        bool reachedEndpoint = false;
        if (currentIndex >= waypoints.Length - 1)
        {
            direction = -1;
            reachedEndpoint = true;
        }
        else if (currentIndex <= 0)
        {
            direction = 1;
            reachedEndpoint = true;
        }

        bool metadataPause = HasPauseMetadata(waypoints[currentIndex]);
        int nextIndex = Mathf.Clamp(currentIndex + direction, 0, waypoints.Length - 1);
        return new PatrolStepResult(nextIndex, reachedEndpoint || metadataPause);
    }

    private static bool HasPauseMetadata(Transform waypoint)
    {
        if (waypoint == null) return false;
        PatrolPointMetadata meta = waypoint.GetComponent<PatrolPointMetadata>();
        return meta != null && meta.PauseHere;
    }
}
