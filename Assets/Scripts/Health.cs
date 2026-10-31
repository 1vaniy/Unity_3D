using UnityEngine;
using UnityEngine.UI;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Slider healthSlider;

    public event Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

        PlayerRage rage = GetComponent<PlayerRage>();
        if (rage != null && rage.IsRaging) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} получил {amount} урона. Осталось HP: {currentHealth}");

        UpdateUI();

        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    private void Die()
    {
        currentHealth = 0;
        UpdateUI();
        Debug.Log($"{gameObject.name} умер!");
        OnDeath?.Invoke();
    }
}