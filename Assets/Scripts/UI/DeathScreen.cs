using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DeathScreen : MonoBehaviour
{
    [Header("UI Смерти")]
    public GameObject deathMenuUI; // Сюда перетащим панель DeathMenu

    private bool isDead = false;

    void Start()
    {
        if (deathMenuUI != null) deathMenuUI.SetActive(false);
    }

    // Этот метод мы вызовем из скрипта игрока, когда он умрет
    public void ShowDeathScreen()
    {
        if (isDead) return;
        isDead = true;

        if (deathMenuUI != null) deathMenuUI.SetActive(true);

        // Останавливаем время (как в паузе)
        Time.timeScale = 0f;

        // Останавливаем звуки
        AudioListener.pause = true;

        // Показываем курсор мыши
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Отключаем управление игроком (на всякий случай)
        PlayerGoWController player = FindObjectOfType<PlayerGoWController>();
        if (player != null) player.enabled = false;

        PlayerCombat combat = FindObjectOfType<PlayerCombat>();
        if (combat != null) combat.enabled = false;
    }

    // Метод для кнопки "Главное меню"
    public void GoToMainMenu()
    {
        // Обязательно возвращаем время и звук перед перезагрузкой!
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Перезагружаем сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}