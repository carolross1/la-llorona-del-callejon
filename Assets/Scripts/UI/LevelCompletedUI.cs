// Assets/Scripts/UI/LevelCompletedUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Pantalla de victoria que se despliega al alcanzar la meta del nivel.
/// Muestra tiempo transcurrido, puntaje obtenido y botones para avanzar o volver.
/// Si es el Nivel 4 (Final del juego), muestra a Carlos a salvo en el interior de su casa a pantalla completa
/// y adapta el diseño en un panel inferior compacto para admirar toda la ilustración y dirigir a Créditos.
/// </summary>
public class LevelCompletedUI : MonoBehaviour
{
    [Header("Panel Principal")]
    [SerializeField] GameObject panel;

    [Header("Ilustración Final (Carlos en su Casa - Pantalla Completa)")]
    [SerializeField] Sprite finalHouseBackgroundSprite;

    [Header("Textos de Resultados")]
    [SerializeField] Text titleText;
    [SerializeField] Text timeResultText;
    [SerializeField] Text scoreResultText;
    [SerializeField] Text highScoreText;

    [Header("Botones")]
    [SerializeField] Button nextLevelButton;
    [SerializeField] Button menuButton;

    Text narrativeFinalText;
    GameObject cardBox;
    Image fullScreenEndingImage;

    void Start()
    {
        SetupColonialVictoryStyle();

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
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

    void SetupColonialVictoryStyle()
    {
        if (panel == null)
        {
            Transform ov = transform.Find("VictoryOverlay");
            if (ov != null) panel = ov.gameObject;
        }
        if (panel == null) return;

        Transform cardTr = panel.transform.Find("VictoryPanel") ?? panel.transform.Find("Card");
        if (cardTr == null && nextLevelButton != null)
            cardTr = nextLevelButton.transform.parent;

        if (cardTr != null)
        {
            cardBox = cardTr.gameObject;
            Font font = ColonialMenuModalHelper.GetMenuFont();
            ColonialMenuModalHelper.StyleColonialModalPanel(cardBox, "¡NIVEL COMPLETADO!", new Vector2(580f, 470f), font);

            Transform sbTr = cardTr.Find("StatsBacking");
            if (sbTr != null)
            {
                RectTransform rtSb = sbTr.GetComponent<RectTransform>();
                if (rtSb != null)
                {
                    rtSb.anchorMin = rtSb.anchorMax = new Vector2(0.5f, 0.5f);
                    rtSb.pivot = new Vector2(0.5f, 0.5f);
                    rtSb.anchoredPosition = new Vector2(0f, 38f);
                    rtSb.sizeDelta = new Vector2(490f, 134f);
                }
                Image imgSb = sbTr.GetComponent<Image>();
                if (imgSb != null)
                {
                    imgSb.sprite = ColonialMenuModalHelper.GetWhiteSprite();
                    imgSb.color = new Color(0.14f, 0.09f, 0.18f, 0.85f);
                }
            }

            Vector2 btnSize = new Vector2(380f, 52f);
            nextLevelButton = ColonialMenuModalHelper.EnsureColonialButton(cardTr, "Btn_Siguiente", "Siguiente Nivel", new Vector2(0f, -74f),  btnSize, 22, true,  font);
            menuButton      = ColonialMenuModalHelper.EnsureColonialButton(cardTr, "Btn_Menu",      "Menú Principal",  new Vector2(0f, -142f), btnSize, 20, false, font);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        if (panel != null && panel.activeInHierarchy)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame ||
                    kb.numpadEnterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                {
                    OnNextLevelClicked();
                    return;
                }
                if (kb.escapeKey.wasPressedThisFrame)
                {
                    OnMenuClicked();
                    return;
                }
            }
        }
    }

    void HandleStateChanged(GameState state)
    {
        if (state == GameState.LevelCompleted)
        {
            Show();
        }
        else
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    /// <summary>
    /// Determina si el nivel actual corresponde al enfrentamiento final (Nivel 4).
    /// </summary>
    bool IsFinalLevel()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevelNumber >= 4)
            return true;

        string currentScene = SceneManager.GetActiveScene().name;
        return currentScene.Contains("Level4") || currentScene.Contains("Cementerio");
    }

    /// <summary>
    /// Despliega el panel de victoria con estadísticas actualizadas y ambientación especial si es el final del juego.
    /// </summary>
    public void Show()
    {
        if (panel != null) panel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        bool isFinal = IsFinalLevel();

        // En el final del juego, ocultar el HUD superior para que la escena de Carlos en su casa luzca limpia a pantalla completa
        if (isFinal && HUD.Instance != null)
        {
            HUD.Instance.gameObject.SetActive(false);
        }

        // 1. Configurar título dinámico
        ResolveTitleText(isFinal);

        // 2. Configurar narrativa, ilustración a pantalla completa y botones para el final del juego
        if (isFinal)
        {
            SetupGameCompletionTheme();
        }

        // 3. Estadísticas de la partida
        if (GameManager.Instance != null)
        {
            float t = GameManager.Instance.LevelTimer;
            int min = Mathf.FloorToInt(t / 60f);
            int sec = Mathf.FloorToInt(t % 60f);
            if (timeResultText != null)
                timeResultText.text = $"Tiempo: {min:00}:{sec:00}";

            int score = GameManager.Instance.LevelScore;
            if (scoreResultText != null)
                scoreResultText.text = isFinal ? $"Puntos: {score}" : $"Puntos Obtenidos: {score}";

            int high = SaveSystem.GetHighScore(GameManager.Instance.CurrentLevelNumber);
            if (highScoreText != null)
                highScoreText.text = $"Récord: {high}";
        }
    }

    void ResolveTitleText(bool isFinal)
    {
        if (titleText == null && panel != null)
        {
            Transform t = panel.transform.Find("VictoryPanel/TituloModal");
            if (t == null) t = panel.transform.Find("Card/TituloModal");
            if (t == null) t = panel.transform.Find("TituloModal");
            if (t == null)
            {
                foreach (var txt in panel.GetComponentsInChildren<Text>(true))
                {
                    if (txt.gameObject.name.Contains("Titulo") || txt.text.Contains("NIVEL") || txt.text.Contains("FINAL"))
                    {
                        titleText = txt;
                        break;
                    }
                }
            }
            else
            {
                titleText = t.GetComponent<Text>();
            }
        }

        if (titleText != null)
        {
            if (isFinal)
            {
                titleText.text = "✦ ¡A SALVO EN CASA! FIN DEL JUEGO ✦";
                titleText.color = new Color(1.0f, 0.88f, 0.32f, 1f);
                titleText.fontSize = 23;
            }
            else
            {
                titleText.text = "¡NIVEL COMPLETADO!";
                titleText.fontSize = 30;
            }
        }
    }

    Sprite LoadFinalHouseSprite()
    {
        if (finalHouseBackgroundSprite != null) return finalHouseBackgroundSprite;

        Sprite res = Resources.Load<Sprite>("UI/final_casa_carlos")
                  ?? Resources.Load<Sprite>("Sprites/UI/final_casa_carlos")
                  ?? Resources.Load<Sprite>("Sprites/Nivel4/final_casa_carlos");
        if (res != null) return res;

        Texture2D tex = Resources.Load<Texture2D>("UI/final_casa_carlos");
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

#if UNITY_EDITOR
        string[] candidatePaths = new string[]
        {
            "Assets/Sprites/UI/final_casa_carlos.png",
            "Assets/Resources/UI/final_casa_carlos.png",
            "Assets/Sprites/UI/final_casa_carlos.jpg",
            "Assets/Sprites/Nivel4/final_casa_carlos.png",
            "Assets/Sprites/Nivel4/final_casa_carlos.jpg"
        };
        foreach (string p in candidatePaths)
        {
            Sprite sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (sp != null) return sp;

            Texture2D editorTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            if (editorTex != null)
            {
                return Sprite.Create(editorTex, new Rect(0, 0, editorTex.width, editorTex.height), new Vector2(0.5f, 0.5f), 100f);
            }
        }
#endif
        return null;
    }

    void SetupGameCompletionTheme()
    {
        Font font = ColonialMenuModalHelper.GetMenuFont();
        Transform card = cardBox != null
            ? cardBox.transform
            : (panel.transform.Find("VictoryPanel") ?? panel.transform.Find("Card") ?? panel.transform);

        // 1. Fondo ilustrado a pantalla completa (1920x1080) del interior de la casa de Carlos
        Sprite houseSprite = LoadFinalHouseSprite();
        if (panel != null && houseSprite != null)
        {
            Transform bgTr = panel.transform.Find("FondoCasaCarlos_FullScreen");
            GameObject bgGo;
            if (bgTr == null)
            {
                bgGo = new GameObject("FondoCasaCarlos_FullScreen", typeof(RectTransform));
                bgGo.transform.SetParent(panel.transform, false);
                bgGo.transform.SetAsFirstSibling();
            }
            else
            {
                bgGo = bgTr.gameObject;
            }

            RectTransform bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            fullScreenEndingImage = bgGo.GetComponent<Image>();
            if (fullScreenEndingImage == null) fullScreenEndingImage = bgGo.AddComponent<Image>();
            fullScreenEndingImage.sprite = houseSprite;
            fullScreenEndingImage.type = Image.Type.Simple;
            fullScreenEndingImage.preserveAspect = false;
            fullScreenEndingImage.color = Color.white;

            // Quitar opacidad oscura al overlay para que la escena de Carlos en su casa luzca nítida en toda la pantalla
            Image overlayImg = panel.GetComponent<Image>();
            if (overlayImg != null)
                overlayImg.color = new Color(0f, 0f, 0f, 0.10f);
        }

        // 2. Acomodar el modal final en la parte inferior central (700 x 270 px) para no tapar a Carlos, la ventana ni el altar
        if (card != null && card != panel.transform)
        {
            ColonialMenuModalHelper.StyleColonialModalPanel(card.gameObject,
                "✦ ¡A SALVO EN CASA! FIN DEL JUEGO ✦",
                new Vector2(700f, 275f), font);

            RectTransform cardRt = card.GetComponent<RectTransform>();
            if (cardRt != null)
            {
                cardRt.anchoredPosition = new Vector2(20f, -355f);
            }

            // Hacer el interior ligeramente translúcido (82%) para integrarse con la habitación cálida
            Transform opq = card.Find("FondoOpacoModal");
            if (opq != null && opq.GetComponent<Image>() != null)
                opq.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.09f, 0.84f);

            Transform inBrd = card.Find("MarcoInternoDorado");
            if (inBrd != null && inBrd.GetComponent<Image>() != null)
                inBrd.GetComponent<Image>().color = new Color(0.10f, 0.07f, 0.14f, 0.84f);

            // Ajustar título y línea divisoria más compactos arriba
            Transform titleTr = card.Find("TituloModal");
            if (titleTr != null)
            {
                RectTransform tRt = titleTr.GetComponent<RectTransform>();
                if (tRt != null)
                {
                    tRt.anchoredPosition = new Vector2(0f, -14f);
                    tRt.sizeDelta = new Vector2(640f, 34f);
                }
                Text tComp = titleTr.GetComponent<Text>();
                if (tComp != null) tComp.fontSize = 22;
            }

            Transform divTr = card.Find("DivisorTitulo");
            if (divTr != null)
            {
                RectTransform dRt = divTr.GetComponent<RectTransform>();
                if (dRt != null)
                {
                    dRt.anchoredPosition = new Vector2(0f, -52f);
                    dRt.sizeDelta = new Vector2(620f, 2f);
                }
            }

            // Acomodar las 3 estadísticas (Tiempo, Puntos, Récord) en una sola franja horizontal limpia
            Transform sbTr = card.Find("StatsBacking");
            if (sbTr != null)
            {
                RectTransform rtSb = sbTr.GetComponent<RectTransform>();
                if (rtSb != null)
                {
                    rtSb.anchoredPosition = new Vector2(0f, -18f);
                    rtSb.sizeDelta = new Vector2(630f, 40f);
                }

                if (timeResultText != null)
                {
                    timeResultText.fontSize = 18;
                    RectTransform rtT = timeResultText.GetComponent<RectTransform>();
                    if (rtT != null)
                    {
                        rtT.anchorMin = rtT.anchorMax = new Vector2(0.5f, 0.5f);
                        rtT.pivot = new Vector2(0.5f, 0.5f);
                        rtT.anchoredPosition = new Vector2(-200f, 0f);
                        rtT.sizeDelta = new Vector2(200f, 32f);
                    }
                }
                if (scoreResultText != null)
                {
                    scoreResultText.fontSize = 18;
                    RectTransform rtS = scoreResultText.GetComponent<RectTransform>();
                    if (rtS != null)
                    {
                        rtS.anchorMin = rtS.anchorMax = new Vector2(0.5f, 0.5f);
                        rtS.pivot = new Vector2(0.5f, 0.5f);
                        rtS.anchoredPosition = new Vector2(0f, 0f);
                        rtS.sizeDelta = new Vector2(220f, 32f);
                    }
                }
                if (highScoreText != null)
                {
                    highScoreText.fontSize = 18;
                    RectTransform rtH = highScoreText.GetComponent<RectTransform>();
                    if (rtH != null)
                    {
                        rtH.anchorMin = rtH.anchorMax = new Vector2(0.5f, 0.5f);
                        rtH.pivot = new Vector2(0.5f, 0.5f);
                        rtH.anchoredPosition = new Vector2(200f, 0f);
                        rtH.sizeDelta = new Vector2(200f, 32f);
                    }
                }
            }

            // Botones lado a lado en la base de la tarjeta
            Vector2 btnSize = new Vector2(295f, 46f);
            menuButton      = ColonialMenuModalHelper.EnsureColonialButton(card, "Btn_Menu",      "Menú Principal",  new Vector2(-160f, -92f), btnSize, 19, false, font);
            nextLevelButton = ColonialMenuModalHelper.EnsureColonialButton(card, "Btn_Siguiente", "Ver Créditos ➔", new Vector2( 160f, -92f), btnSize, 20, true,  font);
        }

        // 3. Crear o actualizar texto de narrativa final en la tarjeta
        if (card != null && narrativeFinalText == null)
        {
            Transform existing = card.Find("TextoNarrativaFinal");
            if (existing != null)
            {
                narrativeFinalText = existing.GetComponent<Text>();
            }
            else
            {
                GameObject storyObj = ColonialMenuModalHelper.CreateColonialText(card, "TextoNarrativaFinal",
                    "", 15, new Color(0.98f, 0.93f, 0.78f, 1f), TextAnchor.MiddleCenter, font);
                narrativeFinalText = storyObj.GetComponent<Text>();
            }
        }

        if (narrativeFinalText != null)
        {
            RectTransform rt = narrativeFinalText.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -58f);
                rt.sizeDelta = new Vector2(630f, 44f);
            }
            narrativeFinalText.fontSize = 15;
            narrativeFinalText.text = "¡El tormento de La Llorona ha terminado! Carlos logró llegar a salvo a su hogar y Guanajuato descansa en paz.";
        }

        // Tocar campana solemne de victoria si existe en AudioManager
        if (AudioManager.Instance != null && AudioManager.Instance.campanaLejana != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.campanaLejana, 1.0f);
        }
    }

    void OnNextLevelClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        if (IsFinalLevel())
        {
            Debug.Log("[LevelCompletedUI] ¡Fin del juego! Cargando escena de Créditos...");
            Time.timeScale = 1f;

            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadScene("Credits");
            else
                SceneManager.LoadScene("Credits");
        }
        else
        {
            Debug.Log("[LevelCompletedUI] Avanzando al siguiente nivel...");
            if (GameManager.Instance != null)
                GameManager.Instance.NextLevel();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    void OnMenuClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonSound();
        Debug.Log("[LevelCompletedUI] Regresando al menú principal...");
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }
}
