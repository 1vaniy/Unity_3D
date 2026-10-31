using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerRage : MonoBehaviour
{
    [Header("Шкала ярости")]
    public float currentRage = 0f;
    public float maxRage = 100f;
    public float rageOnHit = 15f;
    public float rageOnTakeDamage = 15f;

    [Header("Режим Берсерка")]
    public float rageDuration = 10f;
    public float rageStartDelay = 1.5f;
    public float speedMultiplier = 2f; // Ускорение в ярости

    [Header("Настройки взрыва")]
    public float explosionRadius = 5f;
    public float explosionDamage = 100f;
    public GameObject explosionPrefab;

    [Header("Вампиризм")]
    public float healPerHit = 15f;

    [Header("UI и Визуал")]
    public Slider rageSlider;
    public CanvasGroup rageVignetteGroup;

    [Header("Звуки ярости")]
    public AudioClip rageCrySound;
    public AudioSource voiceAudio;

    // Переменные состояния
    public bool IsRaging { get; private set; } = false;
    private bool showVignette = false;

    private Animator animator;
    private PlayerGoWController movement;
    private PlayerCombat combat;
    private float rageEndTime;
    private bool isTimerActive = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerGoWController>();
        combat = GetComponent<PlayerCombat>();
    }

    void Update()
    {
        // --- ВИЗУАЛЬНЫЙ ЭФФЕКТ ВИНЬЕТКИ ---
        if (rageVignetteGroup != null)
        {
            if (showVignette && IsRaging)
            {
                rageVignetteGroup.alpha = Mathf.Lerp(rageVignetteGroup.alpha, 1f, Time.deltaTime * 5f);
            }
            else
            {
                rageVignetteGroup.alpha = Mathf.Lerp(rageVignetteGroup.alpha, 0f, Time.deltaTime * 2f);
            }
        }

        // --- ЛОГИКА ТАЙМЕРА ---
        if (IsRaging)
        {
            if (isTimerActive)
            {
                float timeLeft = rageEndTime - Time.time;

                // Обновляем шкалу
                currentRage = Mathf.Lerp(0f, maxRage, Mathf.Clamp01(timeLeft / rageDuration));
                if (rageSlider != null) rageSlider.value = currentRage / maxRage;

                if (timeLeft <= 0f)
                {
                    EndRage();
                }
            }
            return;
        }

        // Активация
        if (currentRage >= maxRage && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ActivateRage();
        }
    }

    public void AddRage(float amount)
    {
        if (IsRaging) return;
        currentRage = Mathf.Clamp(currentRage + amount, 0f, maxRage);
        if (rageSlider != null) rageSlider.value = currentRage / maxRage;
    }

    private void ActivateRage()
    {
        IsRaging = true;
        isTimerActive = false;
        showVignette = false; // Сбрасываем виньетку до ивента
        currentRage = maxRage;
        if (rageSlider != null) rageSlider.value = 1f;

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsRunning", false);
        }

        animator.SetTrigger("Rage");
        if (movement != null) movement.enabled = false;

        // Запускаем таймер с задержкой (пока идет анимация)
        Invoke(nameof(StartRageTimer), rageStartDelay);
    }

    private void StartRageTimer()
    {
        if (!IsRaging) return;

        isTimerActive = true;
        rageEndTime = Time.time + rageDuration;

        if (movement != null)
        {
            movement.enabled = true;
            movement.EnableSpeedBoost(speedMultiplier); // Ускоряем бег
        }
        if (animator != null) animator.speed = speedMultiplier; // Ускоряем анимации

        Debug.Log("Таймер ярости запущен!");
    }

    // ВЫЗЫВАЕТСЯ ЧЕРЕЗ ANIMATION EVENT (в середине анимации)
    public void RageExplosion()
    {
        showVignette = true; // Включаем виньетку

        // 1. Крик
        if (voiceAudio != null && rageCrySound != null)
        {
            voiceAudio.pitch = Random.Range(0.95f, 1.05f);
            voiceAudio.PlayOneShot(rageCrySound);
        }

        // 2. Частицы взрыва
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 3. Урон по зомби
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            Health targetHealth = hitCollider.GetComponent<Health>();
            if (targetHealth != null && targetHealth.gameObject != gameObject)
            {
                targetHealth.TakeDamage(explosionDamage);
            }
        }
    }

    private void EndRage()
    {
        IsRaging = false;
        isTimerActive = false;
        showVignette = false; // Выключаем виньетку
        currentRage = 0f;
        if (rageSlider != null) rageSlider.value = 0f;

        CancelInvoke(nameof(StartRageTimer));

        // Возвращаем скорость и управление
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetBool("IsMoving", false);
            animator.SetFloat("MoveX", 0f);
            animator.SetFloat("MoveY", 0f);
            animator.SetBool("IsRunning", false);
        }

        if (movement != null)
        {
            movement.DisableSpeedBoost();
            movement.enabled = true;
        }

        // Сбрасываем атаки, чтобы не застревать
        if (combat != null)
        {
            combat.isRunAttacking = false;
            combat.isAttacking = false;
        }

        Debug.Log("Ярость закончилась.");
    }
}