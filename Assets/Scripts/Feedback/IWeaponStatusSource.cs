// Fuente opcional de estado de arma para el HUD flotante (munición y progreso de recarga).
public interface IWeaponStatusSource
{
    bool HasWeapon { get; }
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    bool IsReloading { get; }
    float ReloadProgress01 { get; }
}
