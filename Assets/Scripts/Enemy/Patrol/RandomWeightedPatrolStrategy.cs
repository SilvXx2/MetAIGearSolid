using UnityEngine;

// Selecciona el próximo punto de patrulla mediante Ruleta (RouletteWheelSelection) evitando repetir el actual
// y ponderando por peso configurado (PatrolPointMetadata) + distancia relativa.
public class RandomWeightedPatrolStrategy : IPatrolRouteStrategy
{
    private readonly int pauseEveryNWaypoints;
    private int visitedCount;

    public RandomWeightedPatrolStrategy(int pauseEveryNWaypoints = 1)
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
                return SelectWeightedIndex(index, waypoints);
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
        int nextIndex = SelectWeightedIndex(currentIndex, waypoints);

        return new PatrolStepResult(nextIndex, periodicPause || metadataPause);
    }

    private static int SelectWeightedIndex(int currentIndex, Transform[] waypoints)
    {
        Transform currentWp = (currentIndex >= 0 && currentIndex < waypoints.Length) ? waypoints[currentIndex] : null;
        var roulette = new RouletteWheelSelection<int>();
        int addedCount = 0;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (i == currentIndex || waypoints[i] == null) continue;

            float baseWeight = 1f;
            PatrolPointMetadata meta = waypoints[i].GetComponent<PatrolPointMetadata>();
            if (meta != null)
            {
                baseWeight = meta.SelectionWeight;
            }

            float distanceBonus = 1f;
            if (currentWp != null)
            {
                float dist = Vector3.Distance(currentWp.position, waypoints[i].position);
                distanceBonus = Mathf.Clamp(dist / 10f, 0.5f, 2.5f);
            }

            roulette.AddItem(i, baseWeight * distanceBonus, waypoints[i].name);
            addedCount++;
        }

        if (addedCount == 0)
        {
            return (currentIndex + 1) % waypoints.Length;
        }

        return roulette.SelectItem().Value;
    }

    private static bool HasPauseMetadata(Transform waypoint)
    {
        if (waypoint == null) return false;
        PatrolPointMetadata meta = waypoint.GetComponent<PatrolPointMetadata>();
        return meta != null && meta.PauseHere;
    }
}
