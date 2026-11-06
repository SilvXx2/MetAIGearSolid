using UnityEngine;

public interface IFlockContext : IEnemyContext
{
    ISteering FlockingSteering { get; }
    Transform Leader { get; }
    float LeaderStoppingDistance { get; }
}

