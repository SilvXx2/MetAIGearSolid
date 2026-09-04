using UnityEngine;

public interface IVisionSensor
{
    bool CanSeeTarget { get; }
    Transform Target { get; }
    float VisionDistance { get; }
    float VisionAngle { get; }
}
