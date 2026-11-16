using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public static class NodeAlgorithms
    {
        private const float NodeLoSHeightOffset = 0.5f;
        private const float NodeCornerClearanceRadius = 0.3f;

        private static float ManhattanDistance(Node a, Node b) => Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        private static float Distance(Node a, Node b) => Vector3.Distance(a.Position, b.Position);

        public static List<Node> Astar(Node start, Node goal, LayerMask mask)
        {
            if (start == null || goal == null) return new List<Node>();

            PriorityQueue<Node> frontier = new PriorityQueue<Node>();
            frontier.Enqueue(start, 0);
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node> { { start, null } };
            Dictionary<Node, float> costSoFar = new Dictionary<Node, float> { { start, 0 } };

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

            return ReconstructPath(cameFrom, start, goal);
        }

        public static List<Node> PostSmoothPath(List<Node> path, LayerMask mask)
        {
            if (path == null || path.Count <= 2) return path ?? new List<Node>();

            int current = 0;
            List<Node> smoothPath = new List<Node> { path[0] };

            for (int i = 1; i < path.Count - 1; i++)
            {
                if (!LineOfSight(smoothPath[current].Position, path[i + 1].Position, mask, true))
                {
                    current++;
                    smoothPath.Add(path[i]);
                }
            }
            smoothPath.Add(path[path.Count - 1]);
            return smoothPath;
        }

        public static List<Node> ThetaStar(Node start, Node goal, LayerMask mask)
        {
            if (start == null || goal == null) return new List<Node>();
            if (start == goal) return new List<Node> { start };

            var frontier = new PriorityQueue<Node>();
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node> { { start, null } };
            Dictionary<Node, float> costSoFar = new Dictionary<Node, float> { { start, 0 } };

            frontier.Enqueue(start, 0);
            while (!frontier.IsEmpty)
            {
                Node current = frontier.Dequeue();
                if (current == goal) break;

                foreach (var next in current.Neighbors)
                {
                    if (next == null || next.Blocked || !LineOfSight(current.Position, next.Position, mask, false)) continue;
                    // InView (con margen de esquina para que Theta* no corte esquinas de paredes en diagonal)
                    var realParent = current;
                    if (cameFrom.ContainsKey(current) && cameFrom[current] != null &&
                        LineOfSight(cameFrom[current].Position, next.Position, mask, true))
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

            return ReconstructPath(cameFrom, start, goal);
        }

        private static List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node start, Node goal)
        {
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

        private static bool LineOfSight(Vector3 a, Vector3 b, LayerMask mask, bool checkCornerClearance = false)
        {
            Vector3 origin = a + Vector3.up * NodeLoSHeightOffset;
            Vector3 dest = b + Vector3.up * NodeLoSHeightOffset;

            if (Physics.Linecast(origin, dest, mask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (checkCornerClearance)
            {
                Vector3 toDest = dest - origin;
                float dist = toDest.magnitude;
                if (dist > 0.001f &&
                    Physics.SphereCast(origin, NodeCornerClearanceRadius, toDest / dist, out _, dist, mask, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

