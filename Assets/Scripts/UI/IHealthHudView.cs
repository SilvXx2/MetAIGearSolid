public interface IHealthHudView
{
    void Render(float currentHealth, float maxHealth);
    void PlayDamageFlash();
}
