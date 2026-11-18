using UnityEngine;

public class EnemySeekWeaponState : IState, IDescribedState
{
    private readonly IEnemyContext context;
    private readonly IWeaponHolder weaponHolder;
    private readonly Seek seekSteering;
    private readonly float defaultPickupDistance;

    public EnemySeekWeaponState(IEnemyContext context, IWeaponHolder weaponHolder, float pickupDistance = 1.5f)
    {
        this.context = context;
        this.weaponHolder = weaponHolder;
        this.seekSteering = new Seek(context.Transform);
        this.defaultPickupDistance = Mathf.Max(0.5f, pickupDistance);
    }

    public string StatusLabel => StateLabelFormatter.WithNavigation("BUSCANDO ARMA", context.PathNavigator);
    public NpcAlertLevel AlertLevel => NpcAlertLevel.Vulnerable;

    public void Enter()
    {
        context.PathNavigator?.ClearPath();
    }

    public void Update()
    {
        if (weaponHolder == null || weaponHolder.HasWeapon)
        {
            context.Mover.Move(Vector3.zero, 0f);
            return;
        }

        WeaponPickupStation targetStation = null;
        Transform targetTransform = weaponHolder.WeaponSpawnPoint;

        if (targetTransform != null)
        {
            targetStation = targetTransform.GetComponent<WeaponPickupStation>();
        }
        else
        {
            targetStation = WeaponPickupStation.FindClosestAvailable(context.Transform.position);
            if (targetStation != null)
            {
                targetTransform = targetStation.transform;
            }
        }

        // Si no hay un punto de arma asignado en la escena, equipa el arma inmediatamente como respaldo seguro
        if (targetTransform == null)
        {
            weaponHolder.EquipWeapon();
            return;
        }

        float pickupDist = targetStation != null ? targetStation.PickupRadius : defaultPickupDistance;
        Vector3 toWeapon = targetTransform.position - context.Transform.position;
        toWeapon.y = 0f;

        if (toWeapon.sqrMagnitude <= pickupDist * pickupDist)
        {
            context.Mover.Move(Vector3.zero, 0f);
            if (targetStation != null)
            {
                targetStation.TryPickup(weaponHolder);
            }
            else
            {
                weaponHolder.EquipWeapon();
            }
            return;
        }

        seekSteering.Target = targetTransform;
        Vector3 moveDir;

        // Regla TP2: Si tiene línea de visión usa Steering (Seek), si hay pared usa Pathfinding (A* / Theta*)
        if (context.PathNavigator != null)
        {
            moveDir = context.PathNavigator.GetMoveDirection(targetTransform.position, seekSteering);
        }
        else
        {
            Vector3 desiredDir = seekSteering.GetDir();
            moveDir = context.Avoidance != null
                ? context.Avoidance.GetSteeredDirection(desiredDir)
                : desiredDir;
        }

        context.Mover.Move(moveDir, context.PatrolSpeed);
        context.Mover.Rotate(moveDir);
    }

    public void Exit()
    {
        context.PathNavigator?.ClearPath();
        context.Mover.Move(Vector3.zero, 0f);
    }
}
