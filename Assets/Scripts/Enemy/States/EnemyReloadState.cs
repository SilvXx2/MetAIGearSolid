using UnityEngine;

public class EnemyReloadState : IState, IDescribedState
{
    private readonly IEnemyContext context;
    private readonly IWeaponHolder weaponHolder;

    public EnemyReloadState(IEnemyContext context, IWeaponHolder weaponHolder)
    {
        this.context = context;
        this.weaponHolder = weaponHolder;
    }

    public string StatusLabel => "RECARGANDO · A CUBIERTO";
    public NpcAlertLevel AlertLevel => NpcAlertLevel.Vulnerable;

    public void Enter()
    {
        weaponHolder?.EquippedWeapon?.StartReload();
        context.RunAwayState?.Enter();
    }

    public void Update()
    {
        // Mientras recarga, ejecuta el comportamiento táctico de huida / cobertura (Hide + Evade + Arrive)
        context.RunAwayState?.Update();

        if (weaponHolder?.EquippedWeapon != null)
        {
            weaponHolder.EquippedWeapon.TickReload(Time.deltaTime);
        }
    }

    public void Exit()
    {
        context.RunAwayState?.Exit();
    }
}
