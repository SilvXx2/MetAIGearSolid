using UnityEngine;

public interface IWeapon
{
    float AttackRange { get; }
    bool CanFire { get; }

    bool TryAttack(Transform attacker, Transform target);
    void SetEquippedVisual(bool equipped);
}
