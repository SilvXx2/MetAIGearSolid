using UnityEngine;

public interface IWeaponHolder
{
    bool HasWeapon { get; }
    IReloadableWeapon EquippedWeapon { get; }
    Transform WeaponSpawnPoint { get; }

    void EquipWeapon();
}
