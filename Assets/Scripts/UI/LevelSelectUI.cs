// Assets/Scripts/UI/LevelSelectUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de Selección de Nivel.
/// Muestra los 4 niveles del juego, habilitando únicamente los que hayan sido desbloqueados.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [System.Serializable]
    public struct LevelEntry
    {
        public int levelNumber;
        public string sceneName;
        public Button button;
        public Text labelText;
        public GameObject lockIcon;
    }

    [Header("Niveles")]
    [SerializeField] LevelEntry[] levelEntries;

    [Header("Navegación")]
    [SerializeField] Button backButton;

    void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        int unlocked = SaveSystem.GetUnlockedLevel();

        for (int i = 0; i < levelEntries.Length; i++)
        {
            var entry = levelEntries[i];
            bool isUnlocked = entry.levelNumber <= unlocked;

            if (entry.button != null)
            {
                entry.button.interactable = isUnlocked;
                string sceneToLoad = entry.sceneName;
                entry.button.onClick.AddListener(() => OnLevelSelected(sceneToLoad));
            }

            if (entry.lockIcon != null)
                entry.lockIcon.SetActive(!isUnlocked);
        }
    }

    void OnLevelSelected(string sceneName)
    {
        // Restablecer vidas para la partida seleccionada
        if (FindAnyObjectByType<GameSession>() != null)
            FindAnyObjectByType<GameSession>().ResetSession();

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene(sceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    void OnBackClicked()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainMenu");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
