using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public enum PathfindingAlgorithm
{
    GenericAStar,
    AStarPostSmoothed,
    ThetaStar
}

public interface IPathMaker
{
    PathfindingAlgorithm Algorithm { get; }
    LayerMask WallMask { get; }
    List<Node> CreatePath(Vector3 startPos, Vector3 goalPos, Func<Node, bool> itFulfills = null);
    List<Node> CreatePath(Node start, Node target, Func<Node, bool> itFulfills);
    bool HasLineOfSight(Vector3 from, Vector3 to);
}

