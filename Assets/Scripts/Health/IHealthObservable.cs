using System;

public interface IHealthObservable
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    event Action<float, float> HealthChanged;
    event Action Died;
}
