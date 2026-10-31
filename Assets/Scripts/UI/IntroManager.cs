using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    public GameObject playButton;
    public GameObject quitButton;       // <--- НОВАЯ КНОПКА ВЫХОДА
    public GameObject gameplayUI;       // <--- Сюда перетащим Canvas или панель со шкалами
    public AxeShake axeShake;
    public AxeController axeController;
    public IntroCamera introCamera;
    public PlayerGoWController playerMovement;
    public PlayerCombat playerCombat;
    public CameraOrbit cameraOrbit;
    public ZombieSpawner zombieSpawner;

    [Header("Настройки")]
    public float shakeDuration = 1f;

    void Start()
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (playerCombat != null) playerCombat.enabled = false;

        if (axeShake != null) axeShake.enabled = false;
        if (introCamera != null) introCamera.enabled = false;
        if (cameraOrbit != null) cameraOrbit.enabled = false;

        if (zombieSpawner != null) zombieSpawner.enabled = false;

        // --- ПРЯЧЕМ UI ИГРЫ НА СТАРТЕ ---
        if (gameplayUI != null) gameplayUI.SetActive(false);

        // Отвязываем топор от руки, чтобы он остался лежать на пеньке в меню
        if (axeController != null)
        {
            axeController.transform.SetParent(null);
        }
    }

    public void OnPlayButtonClicked()
    {
        if (playButton != null) playButton.SetActive(false);
        if (quitButton != null) quitButton.SetActive(false); // Прячем кнопку выхода
        StartCoroutine(StartIntroSequence());
    }

    IEnumerator StartIntroSequence()
    {
        if (axeShake != null) axeShake.enabled = true;

        yield return new WaitForSeconds(shakeDuration);

        if (axeShake != null) axeShake.enabled = false;

        if (axeController != null)
        {
            axeController.enabled = true;
            axeController.StartMenuFlight();
        }

        yield return new WaitForSeconds(0.2f);
        if (introCamera != null) introCamera.StartTransition();

        yield return new WaitForSeconds(introCamera.transitionDuration);

        // --- ВКЛЮЧАЕМ ВСЁ УПРАВЛЕНИЕ, СПАВНЕР И UI ---
        if (cameraOrbit != null) cameraOrbit.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (playerCombat != null) playerCombat.enabled = true;
        if (zombieSpawner != null) zombieSpawner.enabled = true;

        // Показываем игровой UI (шкалы)
        if (gameplayUI != null) gameplayUI.SetActive(true);
    }
}