using UnityEngine;

namespace Pathfinding
{
    public interface IGridNodeFilter
    {
        bool IsPositionValid(Vector3 worldPos);
        bool AreNodesConnected(Vector3 from, Vector3 to);
    }
}
