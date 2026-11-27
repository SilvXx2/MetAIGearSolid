using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [ExecuteInEditMode]
    public class Grid : MonoBehaviour
    {
        public Node prefab;
        public Node[] nodeGrid;
        public int width = 36;
        public int height = 49;
        public float distance = 4f;

        [Header("Detección Automática de Paredes y Piso")]
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private LayerMask floorMask = 1;
        [SerializeField] private float wallCheckRadius = 0.45f;
        [SerializeField] private bool requireFloorBeneath = true;

        [Header("Ajuste al Mapa y Poda de Nodos Exteriores")]
        [SerializeField] private bool autoFitToWallsOnGenerate = true;
        [SerializeField] private float wallInsetPadding = 1.5f;
        [SerializeField] private bool pruneBlockedNodes = true;
        [SerializeField] private bool pruneUnreachableExteriorNodes = true;
        [SerializeField] private Transform[] playableSeeds;

        private void Awake()
        {
            if (nodeGrid == null || nodeGrid.Length == 0)
            {
                nodeGrid = GetComponentsInChildren<Node>();
            }

            if (Application.isPlaying && wallMask.value != 0)
            {
                RefreshBlockedNodes();
            }
        }

        [ContextMenu("Ajustar Grilla al Mapa")]
        public void FitGridToMapBounds()
        {
            IGridBoundsFitter fitter = new ObstacleBoundsGridFitter();
            GridBoundsFitResult fit = fitter.ComputeFit(distance, transform.position.y, wallMask, wallInsetPadding);
            if (!fit.Success)
            {
                Debug.LogWarning("[Grid] No se encontraron paredes en wallMask para ajustar la grilla automáticamente.", this);
                return;
            }

            transform.position = fit.Origin;
            width = fit.Width;
            height = fit.Height;
            Debug.Log($"[Grid] Grilla ajustada a paredes: Origin={fit.Origin}, Width={width}, Height={height}, Distance={distance}m.", this);
        }

        [ContextMenu("Crear Nodos")]
        public void SetNodeGrid()
        {
            DeleteNodes();

            if (autoFitToWallsOnGenerate && wallMask.value != 0)
            {
                FitGridToMapBounds();
            }

            IGridNodeFilter nodeFilter = new WalkableSurfaceNodeFilter(
                wallMask: wallMask,
                floorMask: floorMask,
                wallCheckRadius: wallCheckRadius,
                requireFloorBeneath: requireFloorBeneath);

            Node[,] matrix = new Node[width, height];
            List<Node> createdNodes = new List<Node>(width * height / 2);
            int discardedByFilter = 0;

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Vector3 spawnPos = transform.position + new Vector3(w * distance, 0f, h * distance);
                    bool valid = nodeFilter.IsPositionValid(spawnPos);

                    if (pruneBlockedNodes && !valid)
                    {
                        discardedByFilter++;
                        continue;
                    }

                    Node newNode = SpawnNode(w, h, spawnPos);
                    if (!valid)
                    {
                        newNode.Blocked = true;
                    }

                    matrix[w, h] = newNode;
                    createdNodes.Add(newNode);
                }
            }

            ConnectValidNeighbors(matrix, nodeFilter);

            int discardedExterior = 0;
            if (pruneUnreachableExteriorNodes && createdNodes.Count > 0)
            {
                List<Vector3> seeds = ResolvePlayableSeeds();
                if (seeds.Count > 0)
                {
                    IGridPruner pruner = new FloodFillGridPruner();
                    HashSet<Node> reachable = pruner.CollectReachableNodes(createdNodes, seeds, nodeFilter);

                    if (reachable.Count > 0)
                    {
                        List<Node> keptNodes = new List<Node>(reachable.Count);
                        for (int i = 0; i < createdNodes.Count; i++)
                        {
                            Node node = createdNodes[i];
                            if (node == null) continue;

                            if (reachable.Contains(node))
                            {
                                PruneNeighborReferences(node, reachable);
                                keptNodes.Add(node);
                            }
                            else
                            {
                                discardedExterior++;
                                DestroyImmediate(node.gameObject);
                            }
                        }

                        createdNodes = keptNodes;
                    }
                    else
                    {
                        Debug.LogWarning("[Grid] FloodFillGridPruner no encontró nodos alcanzables desde las semillas; se conservan todos los nodos creados.", this);
                    }
                }
                else
                {
                    Debug.LogWarning("[Grid] No hay semillas jugables (ni Player en escena) para podar nodos exteriores.", this);
                }
            }

            nodeGrid = createdNodes.ToArray();
            Debug.Log($"[Grid] Grilla generada: {nodeGrid.Length} nodos interiores caminables (descartados {discardedByFilter} por muro/vacío y {discardedExterior} exteriores, distance={distance}m).", this);
        }

        private Node SpawnNode(int w, int h, Vector3 spawnPos)
        {
            Node newNode;
            if (prefab != null)
            {
                newNode = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
            }
            else
            {
                GameObject go = new GameObject($"Node_{w}_{h}");
                go.transform.SetParent(transform);
                go.transform.position = spawnPos;
                newNode = go.AddComponent<Node>();
            }

            newNode.name = $"Node ({w}, {h})";
            newNode.SetIndexes(w, h);
            return newNode;
        }

        private void ConnectValidNeighbors(Node[,] matrix, IGridNodeFilter connectivityFilter)
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Node current = matrix[w, h];
                    if (current == null) continue;

                    List<Node> neigh = new List<Node>(4);
                    TryAddNeighbor(current, h < height - 1 ? matrix[w, h + 1] : null, connectivityFilter, neigh);
                    TryAddNeighbor(current, w < width - 1 ? matrix[w + 1, h] : null, connectivityFilter, neigh);
                    TryAddNeighbor(current, h > 0 ? matrix[w, h - 1] : null, connectivityFilter, neigh);
                    TryAddNeighbor(current, w > 0 ? matrix[w - 1, h] : null, connectivityFilter, neigh);
                    current.SetNeighbors(neigh);
                }
            }
        }

        private static void TryAddNeighbor(Node current, Node candidate, IGridNodeFilter connectivityFilter, List<Node> list)
        {
            if (candidate == null) return;
            if (connectivityFilter != null && !connectivityFilter.AreNodesConnected(current.Position, candidate.Position))
            {
                return;
            }

            list.Add(candidate);
        }

        private static void PruneNeighborReferences(Node node, HashSet<Node> validNodes)
        {
            if (node == null || node.Neighbors == null) return;

            List<Node> cleanNeighbors = new List<Node>(node.Neighbors.Count);
            for (int i = 0; i < node.Neighbors.Count; i++)
            {
                Node n = node.Neighbors[i];
                if (n != null && validNodes.Contains(n))
                {
                    cleanNeighbors.Add(n);
                }
            }

            node.SetNeighbors(cleanNeighbors);
        }

        private List<Vector3> ResolvePlayableSeeds()
        {
            List<Vector3> seeds = new List<Vector3>();

            if (playableSeeds != null)
            {
                for (int i = 0; i < playableSeeds.Length; i++)
                {
                    if (playableSeeds[i] != null)
                    {
                        seeds.Add(playableSeeds[i].position);
                    }
                }
            }

            if (seeds.Count == 0)
            {
                PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
                if (player != null)
                {
                    seeds.Add(player.transform.position);
                }
            }

            return seeds;
        }

        [ContextMenu("Actualizar Nodos Bloqueados")]
        public void RefreshBlockedNodes()
        {
            if (nodeGrid == null) return;
            IGridNodeFilter filter = new WalkableSurfaceNodeFilter(wallMask, floorMask, wallCheckRadius, requireFloorBeneath: false);

            for (int i = 0; i < nodeGrid.Length; i++)
            {
                if (nodeGrid[i] != null)
                {
                    nodeGrid[i].Blocked = !filter.IsPositionValid(nodeGrid[i].Position);
                }
            }
        }

        [ContextMenu("Eliminar Nodos")]
        public void DeleteNodes()
        {
            Node[] existingChildren = GetComponentsInChildren<Node>(true);
            for (int i = 0; i < existingChildren.Length; i++)
            {
                if (existingChildren[i] != null)
                {
                    DestroyImmediate(existingChildren[i].gameObject);
                }
            }

            if (nodeGrid != null)
            {
                for (int i = 0; i < nodeGrid.Length; i++)
                {
                    if (nodeGrid[i] != null)
                    {
                        DestroyImmediate(nodeGrid[i].gameObject);
                    }
                }
            }

            nodeGrid = new Node[0];
        }

        public Node GetNodeAt(int x, int y)
        {
            if (nodeGrid == null || x < 0 || y < 0 || x >= width || y >= height) return null;

            for (int i = 0; i < nodeGrid.Length; i++)
            {
                Node node = nodeGrid[i];
                if (node != null && node.x == x && node.y == y)
                {
                    return node;
                }
            }

            return null;
        }
    }
}
