// Assets/Scripts/UI/GameOverUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pantalla de derrota desplegada cuando Carlos pierde sus 3 vidas.
/// Permite reintentar la partida reiniciando vidas o volver al Menú Principal.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] GameObject panel;

    [Header("Botones")]
    [SerializeField] Button retryButton;
    [SerializeField] Button menuButton;

    void Start()
    {
        SetupColonialGameOverStyle();

        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnRetryClicked);
        }
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(OnMenuClicked);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += HandleStateChanged;

        if (panel != null)
            panel.SetActive(false);
    }

    void SetupColonialGameOverStyle()
    {
        if (panel == null)
        {
            Transform ov = transform.Find("GameOverOverlay");
            if (ov != null) panel = ov.gameObject;
        }
        if (panel == null) return;

        Transform cardTr = panel.transform.Find("GameOverPanel") ?? panel.transform.Find("Card");
        if (cardTr == null && retryButton != null)
            cardTr = retryButton.transform.parent;

        if (cardTr != null)
        {
            Font font = ColonialMenuModalHelper.GetMenuFont();
            ColonialMenuModalHelper.StyleColonialModalPanel(cardTr.gameObject, "¡HAS SIDO ATRAPADO!", new Vector2(580f, 440f), font);

            Transform subTr = cardTr.Find("SubBacking");
            if (subTr != null)
            {
                RectTransform rtSub = subTr.GetComponent<RectTransform>();
                if (rtSub != null)
                {
                    rtSub.anchorMin = rtSub.anchorMax = new Vector2(0.5f, 0.5f);
                    rtSub.pivot = new Vector2(0.5f, 0.5f);
                    rtSub.anchoredPosition = new Vector2(0f, 38f);
                    rtSub.sizeDelta = new Vector2(490f, 84f);
                }
                Image imgSub = subTr.GetComponent<Image>();
                if (imgSub != null)
                {
                    imgSub.sprite = ColonialMenuModalHelper.GetWhiteSprite();
                    imgSub.color = new Color(0.14f, 0.08f, 0.16f, 0.85f);
                }
            }

            Vector2 btnSize = new Vector2(380f, 52f);
            retryButton = ColonialMenuModalHelper.EnsureColonialButton(cardTr, "Btn_Reintentar", "Reintentar",     new Vector2(0f, -64f),  btnSize, 22, true,  font);
            menuButton  = ColonialMenuModalHelper.EnsureColonialButton(cardTr, "Btn_Menu",       "Menú Principal", new Vector2(0f, -132f), btnSize, 20, false, font);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState state)
    {
        if (panel != null)
            panel.SetActive(state == GameState.GameOver);
    }

    void OnRetryClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (GameManager.Instance != null)
            GameManager.Instance.RetryLevel();
    }

    void OnMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }
}
