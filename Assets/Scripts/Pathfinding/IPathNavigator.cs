using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public interface IPathNavigator
{
    bool IsFollowingPath { get; }
    IReadOnlyList<Node> CurrentPath { get; }
    bool HasLineOfSightTo(Vector3 targetPosition);
    Vector3 GetMoveDirection(Vector3 targetPosition, ISteering directSteering = null);
    void ClearPath();
}

