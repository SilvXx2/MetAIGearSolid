using UnityEngine;

// Componente opcional (ISP) para enriquecer waypoints individuales con pausa de vigilancia, peso de ruleta u orientación.
public class PatrolPointMetadata : MonoBehaviour
{
    [SerializeField] private bool pauseHere = true;
    [SerializeField, Min(0.1f)] private float selectionWeight = 1f;
    [SerializeField] private bool orientToWaypointForward = false;

    public bool PauseHere => pauseHere;
    public float SelectionWeight => Mathf.Max(0.1f, selectionWeight);
    public bool OrientToWaypointForward => orientToWaypointForward;
}
