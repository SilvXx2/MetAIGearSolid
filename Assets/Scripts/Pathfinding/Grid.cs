using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    [ExecuteInEditMode]
    public class Grid : MonoBehaviour
    {
        public Node prefab;
        public Node[] nodeGrid;
        public int width = 10;
        public int height = 10;
        public float distance = 2f;

        [Header("Detección Automática de Paredes")]
        [SerializeField] private LayerMask wallMask;
        [SerializeField] private float wallCheckRadius = 0.45f;

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

        [ContextMenu("Crear Nodos")]
        public void SetNodeGrid()
        {
            DeleteNodes();

            nodeGrid = new Node[width * height];
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    Node newNode;
                    Vector3 spawnPos = transform.position + new Vector3(w * distance, 0f, h * distance);

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
                    if (wallMask.value != 0)
                    {
                        newNode.CheckBlockedByWall(wallMask, wallCheckRadius);
                    }
                    nodeGrid[w + h * width] = newNode;
                }
            }

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    List<Node> neigh = new List<Node>();
                    if (h < height - 1) neigh.Add(nodeGrid[w + (h + 1) * width]); // up
                    if (w < width - 1) neigh.Add(nodeGrid[(w + 1) + h * width]); // right
                    if (h > 0) neigh.Add(nodeGrid[w + (h - 1) * width]); // down
                    if (w > 0) neigh.Add(nodeGrid[(w - 1) + h * width]); // left
                    nodeGrid[w + h * width].SetNeighbors(neigh);
                }
            }
        }

        [ContextMenu("Actualizar Nodos Bloqueados")]
        public void RefreshBlockedNodes()
        {
            if (nodeGrid == null) return;
            for (int i = 0; i < nodeGrid.Length; i++)
            {
                if (nodeGrid[i] != null)
                {
                    nodeGrid[i].CheckBlockedByWall(wallMask, wallCheckRadius);
                }
            }
        }

        [ContextMenu("Eliminar Nodos")]
        public void DeleteNodes()
        {
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
            if (x < 0 || y < 0 || x >= width || y >= height) return null;
            return nodeGrid[x + y * width];
        }
    }
}

