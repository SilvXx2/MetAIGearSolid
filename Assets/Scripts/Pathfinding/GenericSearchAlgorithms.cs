using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class GenericSearchAlgorithms : MonoBehaviour
    {
        public static List<T> BFS<T>(T start, T goal, Func<T, List<T>> getNeighbors, int watchdog = 500)
        {
            if (EqualityComparer<T>.Default.Equals(start, default) || EqualityComparer<T>.Default.Equals(goal, default))
                return new List<T>();

            Queue<T> frontier = new Queue<T>();
            frontier.Enqueue(start);
            Dictionary<T, T> cameFrom = new Dictionary<T, T>();
            cameFrom.Add(start, default);

            while (frontier.Count > 0 && watchdog > 0)
            {
                watchdog--;
                T current = frontier.Dequeue();
                if (current.Equals(goal)) break;

                foreach (var next in getNeighbors(current))
                {
                    if (cameFrom.ContainsKey(next)) continue;
                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<T>();
            var newCurrent = goal;
            List<T> path = new List<T>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        public static List<T> Dijkstra<T>(T start, T goal, Func<T, List<T>> getNeighbors,
            Func<T, T, float> getCost, int watchdog = 1500)
        {
            if (EqualityComparer<T>.Default.Equals(start, default) || EqualityComparer<T>.Default.Equals(goal, default))
                return new List<T>();

            PriorityQueue<T> frontier = new PriorityQueue<T>();
            frontier.Enqueue(start, 0);
            Dictionary<T, T> cameFrom = new Dictionary<T, T>();
            cameFrom.Add(start, default);
            Dictionary<T, float> costSoFar = new Dictionary<T, float>();
            costSoFar.Add(start, 0);

            while (!frontier.IsEmpty && watchdog > 0)
            {
                watchdog--;
                T current = frontier.Dequeue();
                if (current.Equals(goal)) break;

                foreach (var next in getNeighbors(current))
                {
                    var newCost = costSoFar[current] + getCost(current, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost;
                        cameFrom[next] = current;
                        frontier.Enqueue(next, priority);
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<T>();
            var newCurrent = goal;
            List<T> path = new List<T>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        public static List<T> Greedy<T>(T start, T goal, Func<T, List<T>> getNeighbors,
            Func<T, float> heuristic, int watchdog = 500)
        {
            if (EqualityComparer<T>.Default.Equals(start, default) || EqualityComparer<T>.Default.Equals(goal, default))
                return new List<T>();

            PriorityQueue<T> frontier = new PriorityQueue<T>();
            frontier.Enqueue(start, 0);
            Dictionary<T, T> cameFrom = new Dictionary<T, T>();
            cameFrom.Add(start, default);

            while (!frontier.IsEmpty && watchdog > 0)
            {
                watchdog--;
                T current = frontier.Dequeue();
                if (current.Equals(goal)) break;

                foreach (var next in getNeighbors(current))
                {
                    if (cameFrom.ContainsKey(next)) continue;
                    var priority = heuristic(next);
                    cameFrom[next] = current;
                    frontier.Enqueue(next, priority);
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<T>();
            var newCurrent = goal;
            List<T> path = new List<T>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

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

