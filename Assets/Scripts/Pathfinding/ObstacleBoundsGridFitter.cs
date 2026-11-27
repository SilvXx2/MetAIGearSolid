using UnityEngine;

namespace Pathfinding
{
    // Calcula automáticamente el origen, ancho y alto de la grilla ajustados al bounding box de las paredes del mapa.
    public class ObstacleBoundsGridFitter : IGridBoundsFitter
    {
        public GridBoundsFitResult ComputeFit(float cellDistance, float nodeHeightY, LayerMask wallMask, float insetPadding)
        {
            float safeDistance = Mathf.Max(0.5f, cellDistance);
            if (wallMask.value == 0)
            {
                return new GridBoundsFitResult(Vector3.zero, 0, 0, false);
            }

            Collider[] allColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            bool hasBounds = false;
            Bounds mapBounds = default;

            for (int i = 0; i < allColliders.Length; i++)
            {
                Collider col = allColliders[i];
                if (col == null || !col.enabled || col.isTrigger) continue;
                if ((wallMask.value & (1 << col.gameObject.layer)) == 0) continue;
                if (col.GetComponentInParent<Door>() != null) continue;

                if (!hasBounds)
                {
                    mapBounds = col.bounds;
                    hasBounds = true;
                }
                else
                {
                    mapBounds.Encapsulate(col.bounds);
                }
            }

            if (!hasBounds)
            {
                return new GridBoundsFitResult(Vector3.zero, 0, 0, false);
            }

            float minX = mapBounds.min.x + insetPadding;
            float maxX = mapBounds.max.x - insetPadding;
            float minZ = mapBounds.min.z + insetPadding;
            float maxZ = mapBounds.max.z - insetPadding;

            if (maxX <= minX || maxZ <= minZ)
            {
                minX = mapBounds.min.x;
                maxX = mapBounds.max.x;
                minZ = mapBounds.min.z;
                maxZ = mapBounds.max.z;
            }

            int width = Mathf.Max(1, Mathf.CeilToInt((maxX - minX) / safeDistance) + 1);
            int height = Mathf.Max(1, Mathf.CeilToInt((maxZ - minZ) / safeDistance) + 1);
            Vector3 origin = new Vector3(minX, nodeHeightY, minZ);

            return new GridBoundsFitResult(origin, width, height, true);
        }
    }
}
