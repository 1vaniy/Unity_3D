public interface IDamageable
{
    void TakeDamage(float amount);
    void Heal(float amount); // Добавляем, чтобы вампиризм работал через интерфейс
    bool IsDead();           // Чтобы проверять, не бьем ли мы труп
}