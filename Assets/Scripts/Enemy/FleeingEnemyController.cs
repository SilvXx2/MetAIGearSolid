using UnityEngine;

public class FleeingEnemyController : EnemyController
{
    [SerializeField] private float safeDistance = 12f;

    public override IState AlertState => RunAwayState;

    protected override void InitializeStates()
    {
        base.InitializeStates();
        LayerMask mask = (Avoidance != null) ? Avoidance.ObstacleMask : LayerMask.GetMask("Default");
        RunAwayState = new EnemyRunAwayState(this, safeDistance, mask);
    }
}
