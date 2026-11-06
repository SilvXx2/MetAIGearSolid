using UnityEngine;

public interface IVisionSensor
{
    bool CanSeeTarget { get; }
    Transform Target { get; }
    float VisionDistance { get; }
    float VisionAngle { get; }
    Vector3 EyeOffset { get; }
    LayerMask ObstacleMask { get; }
    bool HasLineOfSight(Vector3 from, Vector3 to);
    bool CanSeePosition(Vector3 targetPosition);
}
