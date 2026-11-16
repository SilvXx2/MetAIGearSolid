using System;
using System.Collections.Generic;

namespace Pathfinding
{
    public static class GenericSearchAlgorithms
    {
        public static List<T> Astar<T>(T start, Func<T, bool> itFulfills, Func<T, List<T>> getNeighbors,
             Func<T, T, float> getCost, Func<T, float> heuristic, Func<T, T, bool> isReacheable, int watchdog = 3500)
        {
            if (EqualityComparer<T>.Default.Equals(start, default)) return new List<T>();

            PriorityQueue<T> frontier = new PriorityQueue<T>();
            frontier.Enqueue(start, 0);
            Dictionary<T, T> cameFrom = new Dictionary<T, T>();
            cameFrom.Add(start, default);
            Dictionary<T, float> costSoFar = new Dictionary<T, float>();
            costSoFar.Add(start, 0);
            T fulfilled = default;

            while (!frontier.IsEmpty && watchdog > 0)
            {
                watchdog--;
                T current = frontier.Dequeue();
                if (itFulfills(current))
                {
                    fulfilled = current;
                    break;
                }
                foreach (var next in getNeighbors(current))
                {
                    if (!isReacheable(current, next)) continue;
                    var newCost = costSoFar[current] + getCost(current, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + heuristic(next);
                        cameFrom[next] = current;
                        frontier.Enqueue(next, priority);
                    }
                }
            }

            if (EqualityComparer<T>.Default.Equals(fulfilled, default)) return new List<T>();
            var newCurrent = fulfilled;
            List<T> path = new List<T>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }
    }
}

