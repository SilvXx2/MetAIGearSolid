using System;

public interface IHealthObservable
{
    event Action<float, float> HealthChanged;
    event Action Died;
}
