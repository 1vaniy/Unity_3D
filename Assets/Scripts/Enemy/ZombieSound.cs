using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ZombieSound : MonoBehaviour
{
    [Header("Звуки зомби (Стоны, дыхание)")]
    public AudioClip[] zombieSounds;

    [Tooltip("Громкость для каждого звука по порядку. Если пусто - используется 1")]
    public float[] soundVolumes; // Массив громкостей

    [Header("Звук смерти")]
    public AudioClip deathSound;

    [Range(0f, 1f)]
    public float deathVolume = 1f;

    [Header("Настройки воспроизведения")]
    public float minTimeBetweenSounds = 3f;
    public float maxTimeBetweenSounds = 8f;

    private AudioSource audioSource;
    private float nextSoundTime;
    private ZombieAI zombieAI;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        zombieAI = GetComponent<ZombieAI>();
        SetNextSoundTime();
    }

    void Update()
    {
        if (zombieAI != null && !zombieAI.enabled) return;

        if (Time.time >= nextSoundTime)
        {
            PlayRandomSound();
            SetNextSoundTime();
        }
    }

    void PlayRandomSound()
    {
        if (zombieSounds.Length == 0 || audioSource == null) return;

        int randomIndex = Random.Range(0, zombieSounds.Length);
        AudioClip clip = zombieSounds[randomIndex];

        // Берем громкость из массива, если он заполнен
        float vol = 1f;
        if (soundVolumes != null && randomIndex < soundVolumes.Length)
        {
            vol = soundVolumes[randomIndex];
        }

        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.PlayOneShot(clip, vol);
    }

    void SetNextSoundTime()
    {
        float randomDelay = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
        nextSoundTime = Time.time + randomDelay;
    }

    public void PlayDeathSound()
    {
        if (deathSound != null && audioSource != null)
        {
            this.enabled = false;

            // 1. Рандом по высоте тона (от 0.8 до 1.2)
            audioSource.pitch = Random.Range(0.8f, 1.2f);

            // 2. Ограничиваем громкость (например, 0.4 от текущей)
            // Или можно привязать к инспектору, как мы делали
            float finalVolume = deathVolume * 0.4f;

            audioSource.PlayOneShot(deathSound, finalVolume);
        }
    }
}