using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    [Header("Звуки шагов (нарежь и закинь сюда)")]
    public AudioClip[] footstepClips;

    [Header("Настройки Audio Source")]
    private AudioSource audioSource;

    [Header("Настройки шагов")]
    [Range(0f, 1f)]
    public float stepThreshold = 0.1f; // Чувствительность (чем меньше, тем чаще шаги)
    public float minTimeBetweenSteps = 0.3f; // Минимальное время между шагами (для бега)

    private float lastStepTime = 0f;
    private Animator animator;
    private PlayerGoWController movement;
    private float lastNormalizedTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerGoWController>();

        // Добавляем Audio Source, если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D звук (шаги слышны всегда одинаково)
            audioSource.volume = 0.5f;
        }
    }

    void Update()
    {
        if (animator == null || movement == null) return;

        // --- ПРОВЕРКА НА АТАКИ И ДЕЙСТВИЯ ---
        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null)
        {
            // Если мы бьем, кидаем топор, получаем урон или мертвы — звуков шагов НЕТ
            if (combat.isThrowing || combat.isHit || combat.isDead)
            {
                lastNormalizedTime = 0f;
                return;
            }
        }

        // Проверяем, движемся ли мы (если стоим — шагов нет)
        bool isMoving = animator.GetBool("IsMoving");
        if (!isMoving)
        {
            lastNormalizedTime = 0f;
            return;
        }

        // Получаем текущую анимацию ходьбы/бега
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Дополнительная страховка: если аниматор сейчас в стейте атаки (например, застрял), не играем шаги
        if (stateInfo.IsName("Throw") || stateInfo.IsName("Hit") || stateInfo.IsName("Rage"))
        {
            return;
        }

        float currentTime = stateInfo.normalizedTime % 1f; // от 0 до 1

        // Если анимация зациклилась (перескочила с 1 на 0), сбрасываем
        if (currentTime < lastNormalizedTime)
        {
            lastNormalizedTime = currentTime;
        }

        // Проверяем пересечение порога (например, на 0.25 и 0.75)
        if (Mathf.Abs(currentTime - 0.25f) < stepThreshold || Mathf.Abs(currentTime - 0.75f) < stepThreshold)
        {
            if (Time.time - lastStepTime > minTimeBetweenSteps)
            {
                PlayFootstep();
                lastStepTime = Time.time;
            }
        }

        lastNormalizedTime = currentTime;
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        // Выбираем случайный звук из массива
        int randomIndex = Random.Range(0, footstepClips.Length);
        AudioClip clip = footstepClips[randomIndex];

        // Небольшой рандом по высоте тона (чтобы звуки не были одинаковыми)
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }
}