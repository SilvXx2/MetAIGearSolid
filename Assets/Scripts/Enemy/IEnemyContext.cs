using UnityEngine;

public interface IEnemyContext
{
    StateMachine StateMachine { get; }
    IMover Mover { get; }
    IVisionSensor Vision { get; }
    IState PatrolState { get; }
    IState ChaseState { get; }
    Transform[] Waypoints { get; }
    float PatrolSpeed { get; }
    float ChaseSpeed { get; }
    float WaypointThreshold { get; }
    float LoseTargetTime { get; }
    Transform Transform { get; }
    int GetClosestWaypointIndex();
}
