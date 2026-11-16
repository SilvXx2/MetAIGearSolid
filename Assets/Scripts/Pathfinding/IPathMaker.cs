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
    List<Node> CreatePath(Vector3 startPos, Vector3 goalPos);
    bool HasLineOfSight(Vector3 from, Vector3 to);
}

