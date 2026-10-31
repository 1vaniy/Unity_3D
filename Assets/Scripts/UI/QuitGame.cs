using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Выход из игры...");
        Application.Quit(); // Закрывает игру (работает только в собранном билде)

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Останавливает игру в редакторе
#endif
    }
}