using UnityEngine;

public interface ISteeringModifier
{
    Vector3 ModifyDirection(Vector3 desiredDirection);
}
