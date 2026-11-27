using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
    public interface IGridPruner
    {
        HashSet<Node> CollectReachableNodes(IReadOnlyList<Node> nodes, IReadOnlyList<Vector3> seedPositions, IGridNodeFilter connectivityFilter);
    }
}
