using UnityEngine;

public class FleeingEnemyController : EnemyController
{
    [SerializeField] private float safeDistance = 12f;

    public override IState AlertState => RunAwayState;

    protected override void InitializeStates()
    {
        base.InitializeStates();
        RunAwayState = new EnemyRunAwayState(this, safeDistance);
    }
}
