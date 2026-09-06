using UnityEngine;

public interface IAvoidanceSensor
{
    Vector3 GetSteeredDirection(Vector3 desiredDirection);
}
