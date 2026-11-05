// Assets/Scripts/UI/CreditsUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de la escena de Créditos. Garantiza la navegación de regreso al Menú Principal.
/// </summary>
public class CreditsUI : MonoBehaviour
{
    [SerializeField] Button backButton;

    void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    public void OnBackClicked()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.clicBoton != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.clicBoton);
        }

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadScene("MainMenu");
        else
            SceneManager.LoadScene("MainMenu");
    }
}
