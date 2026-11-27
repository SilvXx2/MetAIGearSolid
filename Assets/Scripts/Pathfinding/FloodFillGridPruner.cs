using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    // Poda por Flood Fill (BFS) desde semillas interiores para descartar todos los nodos fuera de las paredes del mapa.
    public class FloodFillGridPruner : IGridPruner
    {
        public HashSet<Node> CollectReachableNodes(IReadOnlyList<Node> nodes, IReadOnlyList<Vector3> seedPositions, IGridNodeFilter connectivityFilter)
        {
            HashSet<Node> visited = new HashSet<Node>();
            if (nodes == null || nodes.Count == 0 || seedPositions == null || seedPositions.Count == 0)
            {
                return visited;
            }

            Queue<Node> queue = new Queue<Node>();

            for (int s = 0; s < seedPositions.Count; s++)
            {
                Node startNode = FindClosestSeedNode(nodes, seedPositions[s], connectivityFilter);
                if (startNode != null && visited.Add(startNode))
                {
                    queue.Enqueue(startNode);
                }
            }

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();
                if (current == null || current.Neighbors == null) continue;

                for (int i = 0; i < current.Neighbors.Count; i++)
                {
                    Node neighbor = current.Neighbors[i];
                    if (neighbor == null || neighbor.Blocked) continue;
                    if (visited.Contains(neighbor)) continue;
                    if (connectivityFilter != null && !connectivityFilter.AreNodesConnected(current.Position, neighbor.Position))
                    {
                        continue;
                    }

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }

            return visited;
        }

        private static Node FindClosestSeedNode(IReadOnlyList<Node> nodes, Vector3 seedPos, IGridNodeFilter connectivityFilter)
        {
            Node bestVisible = null;
            float bestVisibleDistSqr = float.MaxValue;

            Node bestFallback = null;
            float bestFallbackDistSqr = float.MaxValue;

            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                if (node == null || node.Blocked) continue;

                Vector3 diff = node.Position - seedPos;
                diff.y = 0f;
                float distSqr = diff.sqrMagnitude;

                if (distSqr < bestFallbackDistSqr)
                {
                    bestFallbackDistSqr = distSqr;
                    bestFallback = node;
                }

                Vector3 seedPoint = new Vector3(seedPos.x, node.Position.y, seedPos.z);
                bool clear = connectivityFilter == null || connectivityFilter.AreNodesConnected(seedPoint, node.Position);
                if (clear && distSqr < bestVisibleDistSqr)
                {
                    bestVisibleDistSqr = distSqr;
                    bestVisible = node;
                }
            }

            return bestVisible != null ? bestVisible : bestFallback;
        }
    }
}
