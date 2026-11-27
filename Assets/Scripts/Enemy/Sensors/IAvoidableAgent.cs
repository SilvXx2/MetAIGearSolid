using UnityEngine;

public interface IAvoidableAgent
{
    Vector3 Position { get; }
    float AvoidanceRadius { get; }
    Transform AgentTransform { get; }
}
