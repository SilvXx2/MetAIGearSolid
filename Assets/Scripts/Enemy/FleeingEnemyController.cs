using UnityEngine;

public class FleeingEnemyController : EnemyController
{
    // Distancia a la que se siente a salvo para dejar de huir
    [SerializeField] private float safeDistance = 12f;

    // Overrideo el comportamiento de Deteccion para que corra al verme
    public override IState AlertState => RunAwayState;

    // le paso la distancia a la clase de la que heredo
    protected override float SafeDistance => safeDistance;
}

