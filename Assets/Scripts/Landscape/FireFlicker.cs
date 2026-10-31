using UnityEngine;

[RequireComponent(typeof(Light))]
public class FireFlicker : MonoBehaviour
{
    [Header("Настройки мерцания")]
    public float minIntensity = 1f;   // Минимальная яркость
    public float maxIntensity = 3f;   // Максимальная яркость

    public float minRange = 3f;       // Минимальный радиус света
    public float maxRange = 6f;       // Максимальный радиус света

    public float flickerSpeed = 5f;   // Скорость мерцания (чем больше, тем быстрее)

    private Light fireLight;
    private float randomSeed; // Чтобы у каждого костра было свое уникальное мерцание

    void Start()
    {
        fireLight = GetComponent<Light>();
        randomSeed = Random.Range(0f, 100f); // Уникальный сдвиг для каждого костра
    }

    void Update()
    {
        // Используем шум Перлина для плавного и естественного мерцания
        float noise = Mathf.PerlinNoise(randomSeed, Time.time * flickerSpeed);

        // Плавно интерполируем яркость и радиус
        fireLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        fireLight.range = Mathf.Lerp(minRange, maxRange, noise);

        
    }
}