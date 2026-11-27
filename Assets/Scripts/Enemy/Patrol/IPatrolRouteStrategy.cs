using UnityEngine;

public interface IPatrolRouteStrategy
{
    int GetInitialIndex(Transform agent, Transform[] waypoints, int closestIndex, float waypointThreshold);
    PatrolStepResult GetNextWaypoint(int currentIndex, Transform[] waypoints);
    void Reset();
}
