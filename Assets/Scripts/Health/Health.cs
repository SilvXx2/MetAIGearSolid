using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable, IHealthObservable
{
    [Header("Vida")]
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f || !IsAlive) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth > 0f) return;

        Died?.Invoke();
    }
}
