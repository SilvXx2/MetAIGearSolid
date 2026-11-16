public interface IReloadableWeapon : IWeapon
{
    int CurrentAmmo { get; }
    int MaxAmmo { get; }
    float ReloadDuration { get; }
    float ReloadTimer { get; }

    void StartReload();
    bool TickReload(float deltaTime);
    void CancelReload();
    void RefillImmediate();
    void SetAmmo(int current, int max);
}
