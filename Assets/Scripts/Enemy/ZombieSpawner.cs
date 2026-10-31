using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Префаб зомби")]
    public GameObject zombiePrefab;

    [Header("Настройки волн")]
    public int zombiesPerWave = 3;
    public float timeBetweenWaves = 10f;
    public float delayBetweenSpawns = 0.3f;

    [Header("Усложнение (Прогрессия)")]
    [Tooltip("Во сколько раз увеличивается количество зомби за волну (1.1 = +10%)")]
    public float zombiesPerWaveMultiplier = 1.1f;

    // ВАЖНО: Храним дробное число, чтобы накапливать прогресс
    private float currentZombiesFloat;

    [Header("Лимит зомби")]
    public int maxZombiesAlive = 15;

    [Header("Настройки зоны спавна (Кольцо)")]
    public float minSpawnRadius = 15f;
    public float maxSpawnRadius = 25f;

    [Header("Безопасная зона игрока")]
    public float safeRadiusFromPlayer = 10f;
    public Transform player;

    [Header("Проверка препятствий")]
    public LayerMask obstacleLayer;
    public float checkRadius = 0.5f;

    private float waveTimer = 0f;
    private bool isSpawningWave = false;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
        }

        waveTimer = timeBetweenWaves;
        currentZombiesFloat = zombiesPerWave; // Записываем базовое значение в float
        isSpawningWave = false;
    }

    void OnEnable()
    {
        waveTimer = timeBetweenWaves;
        currentZombiesFloat = zombiesPerWave;
        isSpawningWave = false;
    }

    void Update()
    {
        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0f && !isSpawningWave)
        {
            StartCoroutine(SpawnWaveWithDelay());
            waveTimer = timeBetweenWaves;
        }
    }

    IEnumerator SpawnWaveWithDelay()
    {
        isSpawningWave = true;

        int currentAlive = CountAliveZombies();

        if (currentAlive >= maxZombiesAlive)
        {
            Debug.Log($"Лимит зомби достигнут ({currentAlive}/{maxZombiesAlive}). Волна пропущена.");
            isSpawningWave = false;
            yield break;
        }

        // ОКРУГЛЯЕМ ДЛЯ СПАВНА (если 3.6 -> спавним 4)
        int zombiesToSpawn = Mathf.RoundToInt(currentZombiesFloat);

        // Не спавним больше, чем позволяет лимит
        if (zombiesToSpawn > maxZombiesAlive - currentAlive)
            zombiesToSpawn = maxZombiesAlive - currentAlive;

        Debug.Log($"Волна! Планируется зомби: {zombiesToSpawn} (накоплено {currentZombiesFloat}). Живых на арене: {currentAlive}/{maxZombiesAlive}");

        for (int i = 0; i < zombiesToSpawn; i++)
        {
            if (CountAliveZombies() >= maxZombiesAlive)
            {
                Debug.Log("Достигнут лимит зомби, остаток волны отменен.");
                break;
            }

            SpawnSingleZombie();
            yield return new WaitForSeconds(delayBetweenSpawns);
        }

        // УВЕЛИЧИВАЕМ ДРОБНОЕ ЗНАЧЕНИЕ (накапливаем)
        currentZombiesFloat *= zombiesPerWaveMultiplier;

        isSpawningWave = false;
    }

    int CountAliveZombies()
    {
        ZombieAI[] zombies = FindObjectsOfType<ZombieAI>();
        int aliveCount = 0;
        foreach (var zombie in zombies)
        {
            if (zombie.enabled) aliveCount++;
        }
        return aliveCount;
    }

    void SpawnSingleZombie()
    {
        if (zombiePrefab == null) return;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle) * distance, 0f, Mathf.Sin(angle) * distance);

            if (player != null && Vector3.Distance(spawnPos, player.position) < safeRadiusFromPlayer)
                continue;

            if (Physics.CheckSphere(spawnPos, checkRadius, obstacleLayer))
                continue;

            RaycastHit hit;
            if (Physics.Raycast(spawnPos + Vector3.up * 5f, Vector3.down, out hit, 10f))
            {
                spawnPos = hit.point;
            }

            GameObject newZombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);

            ZombieAI zombieAI = newZombie.GetComponent<ZombieAI>();
            if (zombieAI != null)
            {
                zombieAI.player = player;
            }
            return;
        }
    }
}