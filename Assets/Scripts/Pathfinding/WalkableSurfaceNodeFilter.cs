using UnityEngine;

namespace Pathfinding
{
    // Filtra posiciones sin piso caminable o bloqueadas por muros sólidos, ignorando puertas (Door) para no aislar habitaciones.
    public class WalkableSurfaceNodeFilter : IGridNodeFilter
    {
        private readonly LayerMask wallMask;
        private readonly LayerMask floorMask;
        private readonly float wallCheckRadius;
        private readonly bool requireFloorBeneath;
        private readonly float floorCheckDistance;

        public WalkableSurfaceNodeFilter(
            LayerMask wallMask,
            LayerMask floorMask,
            float wallCheckRadius,
            bool requireFloorBeneath = true,
            float floorCheckDistance = 5f)
        {
            this.wallMask = wallMask;
            this.floorMask = floorMask;
            this.wallCheckRadius = Mathf.Max(0.1f, wallCheckRadius);
            this.requireFloorBeneath = requireFloorBeneath;
            this.floorCheckDistance = Mathf.Max(1f, floorCheckDistance);
        }

        public bool IsPositionValid(Vector3 worldPos)
        {
            if (requireFloorBeneath && !HasFloorBeneath(worldPos))
            {
                return false;
            }

            if (wallMask.value != 0 && HitsSolidWall(worldPos))
            {
                return false;
            }

            return true;
        }

        public bool AreNodesConnected(Vector3 from, Vector3 to)
        {
            if (wallMask.value == 0) return true;

            Vector3 origin = new Vector3(from.x, from.y + 0.35f, from.z);
            Vector3 dest = new Vector3(to.x, to.y + 0.35f, to.z);
            Vector3 delta = dest - origin;
            float dist = delta.magnitude;
            if (dist <= 0.001f) return true;

            RaycastHit[] hits = Physics.RaycastAll(origin, delta / dist, dist, wallMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i].collider;
                if (col == null) continue;
                if (IsDoorCollider(col)) continue;
                return false;
            }

            return true;
        }

        private bool HasFloorBeneath(Vector3 worldPos)
        {
            int mask = floorMask.value != 0 ? floorMask.value : (1 << 0);
            Vector3 rayOrigin = worldPos + Vector3.up * 1.5f;
            return Physics.Raycast(rayOrigin, Vector3.down, floorCheckDistance, mask, QueryTriggerInteraction.Ignore);
        }

        private bool HitsSolidWall(Vector3 worldPos)
        {
            Collider[] overlaps = Physics.OverlapSphere(worldPos, wallCheckRadius, wallMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < overlaps.Length; i++)
            {
                Collider col = overlaps[i];
                if (col == null) continue;
                if (IsDoorCollider(col)) continue;
                return true;
            }

            return false;
        }

        private static bool IsDoorCollider(Collider col)
        {
            return col.GetComponentInParent<Door>() != null;
        }
    }
}
