using UnityEngine;

public interface IVisionSensor
{
    bool CanSeeTarget { get; }
    Transform Target { get; }
}
