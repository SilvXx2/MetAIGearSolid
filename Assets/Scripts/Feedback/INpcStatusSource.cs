using UnityEngine;

// DIP: las vistas de feedback (cono de visión, flecha, HUD) dependen de esta abstracción y no de EnemyController.
public interface INpcStatusSource
{
    string StatusLabel { get; }
    NpcAlertLevel AlertLevel { get; }
    Transform Transform { get; }
}
