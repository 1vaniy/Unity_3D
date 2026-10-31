using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Паузы")]
    public GameObject pauseMenuUI; // Панель с кнопками паузы

    [Header("Состояние")]
    public bool isPaused = false;

    void Start()
    {
        // При старте панель паузы скрыта
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // Слушаем нажатие Esc
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Проверяем, можно ли вообще ставить паузу (если игра еще в меню — нельзя)
            if (CanPause())
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    // Метод проверки: мы в игре или в меню?
    private bool CanPause()
    {
        // Ищем игрока и его скрипт движения
        PlayerGoWController player = FindObjectOfType<PlayerGoWController>();

        // Если игрок есть и его скрипт включен (значит мы в игре, а не в меню) — можно паузить
        if (player != null && player.enabled)
        {
            return true;
        }

        return false; // В меню (или игрок мертв) — пауза запрещена
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

        // Пауза для звука
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;

        // Возобновляем звук
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false; // <--- ВОЗВРАЩАЕМ ЗВУК

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}