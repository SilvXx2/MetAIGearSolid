using System.Collections;
using UnityEngine;

public class AttackVisualEffect : MonoBehaviour, IAttackEffect
{
    [SerializeField] private GameObject visualObject;
    [SerializeField] private float duration = 0.15f;

    private Coroutine activeCoroutine;

    private void Awake()
    {
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
    }

    public void TriggerEffect()
    {
        if (visualObject == null) return;

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        visualObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        visualObject.SetActive(false);
        activeCoroutine = null;
    }
}
