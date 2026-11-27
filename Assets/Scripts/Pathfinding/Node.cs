using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public class Node : MonoBehaviour
    {
        [SerializeField] private List<Node> neighbors = new List<Node>();
        [SerializeField] private int widthPos;
        [SerializeField] private int heightPos;
        [SerializeField] private float cost = 1f;
        [SerializeField] private bool blocked;

        private Renderer rend;

        public bool Blocked
        {
            get => blocked;
            set
            {
                blocked = value;
                UpdateVisualColor();
            }
        }

        public List<Node> Neighbors => neighbors;

        public Color Color
        {
            set
            {
                if (rend == null) rend = GetComponent<Renderer>();
                if (rend != null && Application.isPlaying)
                {
                    rend.material.color = value;
                }
            }
        }

        public Vector3 Position => transform.position;

        public int x => widthPos;
        public int y => heightPos;
        public float Cost => Mathf.Max(1f, cost);

        private void Awake()
        {
            rend = GetComponent<Renderer>();
            UpdateVisualColor();
        }

        public void SetIndexes(int w, int h)
        {
            widthPos = w;
            heightPos = h;
        }

        public void SetNeighbors(List<Node> neighbors)
        {
            this.neighbors = neighbors;
        }

        public void CheckBlockedByWall(LayerMask wallMask, float checkRadius = 0.4f)
        {
            if (wallMask.value == 0) return;
            blocked = Physics.CheckSphere(transform.position, checkRadius, wallMask, QueryTriggerInteraction.Ignore);
            UpdateVisualColor();
        }

        private void UpdateVisualColor()
        {
            if (rend != null && Application.isPlaying)
            {
                Color = blocked ? Color.black : Color.Lerp(Color.white, Color.green, (Cost - 1f) / 10f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = blocked ? Color.red : Color.green;
            Gizmos.DrawSphere(transform.position, 0.2f);

            if (neighbors == null) return;
            foreach (var node in neighbors)
            {
                if (node == null) continue;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }
}

