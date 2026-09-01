using UnityEngine;

public interface IMover
{
    void Move(Vector3 direction, float speed);
    void Rotate(Vector3 direction);
}
