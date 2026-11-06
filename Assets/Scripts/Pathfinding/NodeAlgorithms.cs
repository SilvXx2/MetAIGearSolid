using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class NodeAlgorithms : MonoBehaviour
    {
        public static List<Node> BFS(Node start, Node goal)
        {
            if (start == null || goal == null) return new List<Node>();

            Queue<Node> frontier = new Queue<Node>();
            frontier.Enqueue(start);
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            cameFrom.Add(start, null);

            while (frontier.Count > 0)
            {
                Node current = frontier.Dequeue();
                current.Color = Color.blue;
                if (current == goal) break;

                foreach (var next in current.Neighbors)
                {
                    if (next == null || cameFrom.ContainsKey(next)) continue;
                    cameFrom[next] = current;
                    frontier.Enqueue(next);
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<Node>();
            var newCurrent = goal;
            List<Node> path = new List<Node>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        public static List<Node> DFS(Node start, Node goal)
        {
            if (start == null || goal == null) return new List<Node>();

            Stack<Node> frontier = new Stack<Node>();
            frontier.Push(start);
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            cameFrom.Add(start, null);

            while (frontier.Count > 0)
            {
                Node current = frontier.Pop();
                current.Color = Color.blue;
                if (current == goal) break;

                foreach (var next in current.Neighbors)
                {
                    if (next == null || cameFrom.ContainsKey(next)) continue;
                    cameFrom[next] = current;
                    frontier.Push(next);
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<Node>();
            var newCurrent = goal;
            List<Node> path = new List<Node>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        public static List<Node> Dijkstra(Node start, Node goal)
        {
            if (start == null || goal == null) return new List<Node>();

            PriorityQueue<Node> frontier = new PriorityQueue<Node>();
            frontier.Enqueue(start, 0);
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            cameFrom.Add(start, null);
            Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();
            costSoFar.Add(start, 0);

            while (!frontier.IsEmpty)
            {
                Node current = frontier.Dequeue();
                current.Color = Color.blue;
                if (current == goal) break;

                foreach (var next in current.Neighbors)
                {
                    if (next == null) continue;
                    var newCost = costSoFar[current] + next.Cost;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost;
                        cameFrom[next] = current;
                        frontier.Enqueue(next, priority);
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<Node>();
            var newCurrent = goal;
            List<Node> path = new List<Node>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        private static float ManhattanDistance(Node a, Node b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        private static float SqrDistance(Node a, Node b)
        {
            return (a.Position - b.Position).sqrMagnitude;
        }

        private static float Distance(Node a, Node b)
        {
            return Vector3.Distance(a.Position, b.Position);
        }

        public static List<Node> Greedy(Node start, Node goal)
        {
            if (start == null || goal == null) return new List<Node>();

            PriorityQueue<Node> frontier = new PriorityQueue<Node>();
            frontier.Enqueue(start, 0);
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            cameFrom.Add(start, null);

            while (!frontier.IsEmpty)
            {
                Node current = frontier.Dequeue();
                if (current == goal) break;

                foreach (var next in current.Neighbors)
                {
                    if (next == null || next.Blocked || cameFrom.ContainsKey(next)) continue;
                    next.Color = Color.blue;

                    var priority = Distance(next, goal);
                    cameFrom[next] = current;
                    frontier.Enqueue(next, priority);
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<Node>();
            var newCurrent = goal;
            List<Node> path = new List<Node>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        public static List<Node> Astar(Node start, Node goal, LayerMask mask)
        {
            if (start == null || goal == null) return new List<Node>();

            PriorityQueue<Node> frontier = new PriorityQueue<Node>();
            frontier.Enqueue(start, 0);
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            cameFrom.Add(start, null);
            Dictionary<Node, float> costSoFar = new Dictionary<Node, float>();
            costSoFar.Add(start, 0);

            while (!frontier.IsEmpty)
            {
                Node current = frontier.Dequeue();
                if (current == goal) break;

                foreach (var next in current.Neighbors)
                {
                    if (next == null || next.Blocked || !LineOfSight(current.Position, next.Position, mask)) continue;
                    var newCost = costSoFar[current] + Distance(current, next) * next.Cost;
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + ManhattanDistance(next, goal);
                        cameFrom[next] = current;
                        frontier.Enqueue(next, priority);

                        next.Color = Color.blue;
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<Node>();
            var newCurrent = goal;
            List<Node> path = new List<Node>();
            while (newCurrent != null)
            {
                path.Add(newCurrent);
                newCurrent = cameFrom[newCurrent];
            }
            path.Reverse();
            return path;
        }

        public static List<Node> PostSmoothPath(List<Node> path, LayerMask mask)
        {
            if (path == null || path.Count <= 2) return path ?? new List<Node>();

            int current = 0;
            List<Node> smoothPath = new List<Node>();
            smoothPath.Add(path[0]);

            for (int i = 1; i < path.Count - 1; i++)
            {
                if (!LineOfSight(smoothPath[current].Position, path[i + 1].Position, mask))
                {
                    current++;
                    smoothPath.Add(path[i]);
                }
            }
            smoothPath.Add(path[path.Count - 1]);
            return smoothPath;
        }

        private static bool LineOfSight(Vector3 a, Vector3 b, LayerMask mask)
        {
            return !Physics.Linecast(a, b, mask, QueryTriggerInteraction.Ignore);
        }

        public static List<Node> ThetaStar(Node start, Node goal, LayerMask mask)
        {
            if (start == null || goal == null) return new List<Node>();
            if (start == goal) return new List<Node> { start };

            var frontier = new PriorityQueue<Node>();
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            Dictionary<Node, float> costSoFar = new Dictionary<Node, float> { { start, 0 } };
            HashSet<Node> visited = new HashSet<Node>();

            frontier.Enqueue(start, 0);
            while (!frontier.IsEmpty)
            {
                Node current = frontier.Dequeue();
                if (current == goal) break;

                visited.Add(current);
                foreach (var next in current.Neighbors)
                {
                    if (next == null || next.Blocked || !LineOfSight(current.Position, next.Position, mask)) continue;
                    // InView
                    var realParent = current;
                    if (cameFrom.ContainsKey(current) && cameFrom[current] != null &&
                        LineOfSight(cameFrom[current].Position, next.Position, mask))
                    {
                        realParent = cameFrom[current];
                    }
                    float newCost = costSoFar[realParent] + Distance(realParent, next);
                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        frontier.Enqueue(next, newCost + Distance(goal, next));
                        cameFrom[next] = realParent;
                        next.Color = Color.blue;
                    }
                }
            }

            if (!cameFrom.ContainsKey(goal)) return new List<Node>();

            // Creación del Path
            Node currentInPath = goal;
            List<Node> path = new List<Node>();
            int safety = 2000;
            while (currentInPath != start && currentInPath != null && safety-- > 0)
            {
                path.Add(currentInPath);
                currentInPath = cameFrom.ContainsKey(currentInPath) ? cameFrom[currentInPath] : null;
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
    }
}

