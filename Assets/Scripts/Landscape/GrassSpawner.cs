using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    [Header("Настройки травы")]
    public GameObject grassPrefab;

    [Header("Размеры площади (X, Z)")]
    public float areaSizeX = 50f;
    public float areaSizeZ = 50f;

    [Header("Расположение (Сетка)")]
    [Tooltip("Расстояние между кустиками. Чем меньше — тем гуще трава (осторожно с FPS)")]
    public float spacing = 2f;
    [Tooltip("Разброс внутри клетки. 0 — идеальная сетка, 1 — максимальный хаос")]
    [Range(0f, 1f)]
    public float positionJitter = 0.8f;

    [Header("Настройки высоты и масштаба")]
    public float spawnHeightOffset = 0.5f;
    public float grassScale = 1f;

    void Start()
    {
        if (grassPrefab == null)
        {
            Debug.LogError("Ошибка: Префаб травы не назначен в инспекторе!");
            return;
        }

        GameObject grassParent = new GameObject("Generated_Grass");
        grassParent.transform.position = transform.position;

        // Вычисляем, сколько кустиков поместится по ширине и длине
        int countX = Mathf.FloorToInt(areaSizeX / spacing);
        int countZ = Mathf.FloorToInt(areaSizeZ / spacing);

        // Находим начальную точку (левый нижний угол)
        Vector3 startPos = transform.position - new Vector3(areaSizeX / 2f, 0, areaSizeZ / 2f);

        // Проходимся по сетке
        for (int x = 0; x < countX; x++)
        {
            for (int z = 0; z < countZ; z++)
            {
                // Базовая позиция клетки
                float posX = startPos.x + (x * spacing) + (spacing / 2f);
                float posZ = startPos.z + (z * spacing) + (spacing / 2f);

                // Добавляем разброс внутри клетки
                float jitterX = Random.Range(-spacing / 2f, spacing / 2f) * positionJitter;
                float jitterZ = Random.Range(-spacing / 2f, spacing / 2f) * positionJitter;

                Vector3 spawnPos = new Vector3(posX + jitterX, startPos.y + spawnHeightOffset, posZ + jitterZ);

                // Создаем траву
                GameObject newGrass = Instantiate(grassPrefab, spawnPos, Quaternion.identity, grassParent.transform);

                newGrass.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                float randomScale = grassScale * Random.Range(0.9f, 1.1f);
                newGrass.transform.localScale = grassPrefab.transform.localScale * randomScale;
            }
        }
    }
}