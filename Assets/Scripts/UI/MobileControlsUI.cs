// Assets/Scripts/UI/MobileControlsUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Interfaz táctil colonial en pantalla para jugar en dispositivos móviles (Android APK y WebGL móvil):
/// - Joystick virtual multitáctil a la izquierda para caminar en 360°.
/// - Botones circulares ergonómicos a la derecha: [E] Usar/Esconder, Correr (mantener), Saltar y [F] Ofrenda/Agua Bendita.
/// - Botón de Pausa [||] en la esquina superior derecha debajo del HUD.
/// - Respeta automáticamente el Safe Area (notch / bordes curvos de celulares) y se oculta durante modales de Pausa/Victoria/Derrota.
/// </summary>
public class MobileControlsUI : MonoBehaviour
{
    public static MobileControlsUI Instance { get; private set; }

    static Sprite cachedDiscSprite;
    static Sprite cachedRingSprite;

    Canvas touchCanvas;
    RectTransform safeAreaRect;
    GameObject gameplayControlsRoot;

    // Referencias dinámicas para retroalimentación contextual
    Text interactLabel;
    Image interactRim;
    Text actionFLabel;
    Image actionFRim;
    GameObject actionFButtonObj;
    Image sprintFillImg;

    PlayerMovement cachedPlayer;
    PlayerHolyWaterLauncher cachedLauncher;
    bool sceneUsesActionF;
    Rect lastSafeArea;
    Vector2Int lastScreenSize;

    public static Sprite GetDiscSprite()
    {
        if (cachedDiscSprite != null) return cachedDiscSprite;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float maxR = center - 1.5f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(maxR - dist + 0.75f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        cachedDiscSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return cachedDiscSprite;
    }

    public static Sprite GetRingSprite()
    {
        if (cachedRingSprite != null) return cachedRingSprite;

        const int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float center = (size - 1) * 0.5f;
        float outerR = center - 1.5f;
        float innerR = outerR - 7.5f;

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float outerAlpha = Mathf.Clamp01(outerR - dist + 0.75f);
                float innerAlpha = Mathf.Clamp01(dist - innerR + 0.75f);
                float ringAlpha = outerAlpha * innerAlpha;

                // Sutil anillo secundario interior estilo colonial
                float subOuter = innerR - 4.0f;
                float subInner = subOuter - 2.0f;
                float subAlpha = Mathf.Clamp01(subOuter - dist + 0.6f) * Mathf.Clamp01(dist - subInner + 0.6f) * 0.45f;

                float finalA = Mathf.Clamp01(ringAlpha + subAlpha);
                pixels[y * size + x] = new Color(1f, 1f, 1f, finalA);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        cachedRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return cachedRingSprite;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildTouchCanvasHierarchy();
        MobileInputManager.OnVisibilityChanged += RefreshVisibility;
    }

    void Start()
    {
        DetectSceneCapabilities();
        ApplySafeArea();
        RefreshVisibility();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        MobileInputManager.OnVisibilityChanged -= RefreshVisibility;
        MobileInputManager.ResetAllInputs();
    }

    void DetectSceneCapabilities()
    {
        cachedPlayer = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (cachedPlayer != null)
            cachedLauncher = cachedPlayer.GetComponent<PlayerHolyWaterLauncher>();

        bool hasCandlesOrShrines =
            FindAnyObjectByType<Candle>(FindObjectsInactive.Include) != null ||
            FindAnyObjectByType<CandleStand>(FindObjectsInactive.Include) != null ||
            FindAnyObjectByType<ShrineNiche>(FindObjectsInactive.Include) != null;

        sceneUsesActionF = (cachedLauncher != null) || hasCandlesOrShrines;

        if (actionFButtonObj != null)
        {
            actionFButtonObj.SetActive(sceneUsesActionF);
        }

        if (cachedLauncher != null)
        {
            if (actionFLabel != null) actionFLabel.text = "AGUA\nBENDITA";
            if (actionFRim != null) actionFRim.color = new Color(0.35f, 0.90f, 1.0f, 0.92f);
        }
        else
        {
            if (actionFLabel != null) actionFLabel.text = "OFRENDA\n[F]";
            if (actionFRim != null) actionFRim.color = new Color(1.0f, 0.80f, 0.30f, 0.90f);
        }
    }

    void BuildTouchCanvasHierarchy()
    {
        Font font = ColonialMenuModalHelper.GetMenuFont();

        touchCanvas = gameObject.AddComponent<Canvas>();
        touchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        touchCanvas.sortingOrder = 12; // Encima del mundo y HUD base, pero se oculta cuando hay modales abiertos

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.dynamicPixelsPerUnit = 2.0f;

        gameObject.AddComponent<GraphicRaycaster>();

        // Contenedor adaptado al Safe Area del teléfono
        GameObject safeGo = new GameObject("SafeAreaRoot", typeof(RectTransform));
        safeGo.transform.SetParent(transform, false);
        safeAreaRect = safeGo.GetComponent<RectTransform>();
        safeAreaRect.anchorMin = Vector2.zero;
        safeAreaRect.anchorMax = Vector2.one;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;

        gameplayControlsRoot = new GameObject("GameplayControls", typeof(RectTransform));
        gameplayControlsRoot.transform.SetParent(safeGo.transform, false);
        RectTransform gpRt = gameplayControlsRoot.GetComponent<RectTransform>();
        gpRt.anchorMin = Vector2.zero;
        gpRt.anchorMax = Vector2.one;
        gpRt.offsetMin = Vector2.zero;
        gpRt.offsetMax = Vector2.zero;

        // 1. JOYSTICK VIRTUAL MULTITÁCTIL (Esquina inferior izquierda)
        BuildVirtualJoystick(gameplayControlsRoot.transform, font);

        // 2. ARCO ERGONÓMICO DE BOTONES DE ACCIÓN (Esquina inferior derecha)
        // [E] Usar / Esconderse (Botón principal grande)
        GameObject interactBtn = CreateRoundActionButton(
            gameplayControlsRoot.transform, "Btn_MobileInteract", "USAR /\nOCULTAR",
            new Vector2(1f, 0f), new Vector2(-124f, 224f), 124f, 16,
            new Color(0.16f, 0.11f, 0.22f, 0.68f),
            new Color(0.95f, 0.78f, 0.32f, 0.92f),
            font, out interactLabel, out interactRim, out _);
        var tapInteract = interactBtn.AddComponent<MobileTapButton>();
        tapInteract.OnTap = MobileInputManager.TriggerInteract;

        // Correr (Mantener presionado)
        GameObject sprintBtn = CreateRoundActionButton(
            gameplayControlsRoot.transform, "Btn_MobileSprint", "CORRER",
            new Vector2(1f, 0f), new Vector2(-264f, 110f), 110f, 16,
            new Color(0.24f, 0.13f, 0.08f, 0.68f),
            new Color(1.0f, 0.76f, 0.26f, 0.90f),
            font, out _, out _, out sprintFillImg);
        var holdSprint = sprintBtn.AddComponent<MobileHoldButton>();
        holdSprint.OnHoldChanged = MobileInputManager.SetSprintHeld;
        holdSprint.TargetFill = sprintFillImg;
        holdSprint.IdleColor = new Color(0.24f, 0.13f, 0.08f, 0.68f);
        holdSprint.PressedColor = new Color(0.58f, 0.32f, 0.10f, 0.92f);

        // Saltar (Toque rápido)
        GameObject jumpBtn = CreateRoundActionButton(
            gameplayControlsRoot.transform, "Btn_MobileJump", "SALTAR",
            new Vector2(1f, 0f), new Vector2(-114f, 88f), 102f, 16,
            new Color(0.14f, 0.10f, 0.20f, 0.66f),
            new Color(0.84f, 0.68f, 0.30f, 0.88f),
            font, out _, out _, out _);
        var tapJump = jumpBtn.AddComponent<MobileTapButton>();
        tapJump.OnTap = MobileInputManager.TriggerJump;

        // [F] Ofrenda / Agua Bendita (Contextual en Niveles 3 y 4)
        actionFButtonObj = CreateRoundActionButton(
            gameplayControlsRoot.transform, "Btn_MobileActionF", "OFRENDA\n[F]",
            new Vector2(1f, 0f), new Vector2(-258f, 246f), 110f, 15,
            new Color(0.22f, 0.12f, 0.16f, 0.68f),
            new Color(1.0f, 0.80f, 0.30f, 0.90f),
            font, out actionFLabel, out actionFRim, out _);
        var tapActionF = actionFButtonObj.AddComponent<MobileTapButton>();
        tapActionF.OnTap = MobileInputManager.TriggerActionF;

        // 3. BOTÓN TÁCTIL DE PAUSA (Debajo de la placa superior derecha del HUD)
        GameObject pauseBtn = CreateRoundActionButton(
            gameplayControlsRoot.transform, "Btn_MobilePause", "| |",
            new Vector2(1f, 1f), new Vector2(-58f, -196f), 68f, 22,
            new Color(0.10f, 0.07f, 0.15f, 0.78f),
            new Color(0.92f, 0.74f, 0.30f, 0.92f),
            font, out _, out _, out _);
        var tapPause = pauseBtn.AddComponent<MobileTapButton>();
        tapPause.OnTap = MobileInputManager.TriggerPause;
    }

    void BuildVirtualJoystick(Transform parent, Font font)
    {
        // Zona táctil generosa en esquina inferior izquierda
        GameObject zoneGo = new GameObject("JoystickTouchZone", typeof(RectTransform));
        zoneGo.transform.SetParent(parent, false);
        RectTransform zoneRt = zoneGo.GetComponent<RectTransform>();
        zoneRt.anchorMin = zoneRt.anchorMax = new Vector2(0f, 0f);
        zoneRt.pivot = new Vector2(0.5f, 0.5f);
        zoneRt.anchoredPosition = new Vector2(188f, 178f);
        zoneRt.sizeDelta = new Vector2(280f, 280f);

        Image hitImg = zoneGo.AddComponent<Image>();
        hitImg.sprite = GetDiscSprite();
        hitImg.color = new Color(0f, 0f, 0f, 0.01f); // Transparente pero recibe toques
        hitImg.raycastTarget = true;

        // Base circular visible del joystick
        GameObject bgGo = new GameObject("JoystickBase", typeof(RectTransform));
        bgGo.transform.SetParent(zoneGo.transform, false);
        RectTransform bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = bgRt.anchorMax = new Vector2(0.5f, 0.5f);
        bgRt.pivot = new Vector2(0.5f, 0.5f);
        bgRt.anchoredPosition = Vector2.zero;
        bgRt.sizeDelta = new Vector2(206f, 206f);

        Image bgDisc = bgGo.AddComponent<Image>();
        bgDisc.sprite = GetDiscSprite();
        bgDisc.color = new Color(0.06f, 0.04f, 0.10f, 0.52f);
        bgDisc.raycastTarget = false;

        GameObject rimGo = new GameObject("JoystickRim", typeof(RectTransform));
        rimGo.transform.SetParent(bgGo.transform, false);
        RectTransform rimRt = rimGo.GetComponent<RectTransform>();
        rimRt.anchorMin = Vector2.zero;
        rimRt.anchorMax = Vector2.one;
        rimRt.offsetMin = Vector2.zero;
        rimRt.offsetMax = Vector2.zero;

        Image rimImg = rimGo.AddComponent<Image>();
        rimImg.sprite = GetRingSprite();
        rimImg.color = new Color(0.88f, 0.70f, 0.28f, 0.78f);
        rimImg.raycastTarget = false;

        // Puntos cardinales sutiles
        CreateCardinalDot(bgGo.transform, new Vector2(0f, 82f));
        CreateCardinalDot(bgGo.transform, new Vector2(0f, -82f));
        CreateCardinalDot(bgGo.transform, new Vector2(-82f, 0f));
        CreateCardinalDot(bgGo.transform, new Vector2(82f, 0f));

        // Palanca central móvil (Knob)
        GameObject knobGo = new GameObject("JoystickKnob", typeof(RectTransform));
        knobGo.transform.SetParent(bgGo.transform, false);
        RectTransform knobRt = knobGo.GetComponent<RectTransform>();
        knobRt.anchorMin = knobRt.anchorMax = new Vector2(0.5f, 0.5f);
        knobRt.pivot = new Vector2(0.5f, 0.5f);
        knobRt.anchoredPosition = Vector2.zero;
        knobRt.sizeDelta = new Vector2(84f, 84f);

        Image knobImg = knobGo.AddComponent<Image>();
        knobImg.sprite = GetDiscSprite();
        knobImg.color = new Color(0.95f, 0.78f, 0.30f, 0.88f);
        knobImg.raycastTarget = false;

        GameObject knobCore = new GameObject("KnobCore", typeof(RectTransform));
        knobCore.transform.SetParent(knobGo.transform, false);
        RectTransform kcRt = knobCore.GetComponent<RectTransform>();
        kcRt.anchorMin = kcRt.anchorMax = new Vector2(0.5f, 0.5f);
        kcRt.sizeDelta = new Vector2(46f, 46f);
        Image kcImg = knobCore.AddComponent<Image>();
        kcImg.sprite = GetDiscSprite();
        kcImg.color = new Color(1f, 0.94f, 0.72f, 0.95f);
        kcImg.raycastTarget = false;

        MobileVirtualJoystick joy = zoneGo.AddComponent<MobileVirtualJoystick>();
        joy.BaseRect = bgRt;
        joy.KnobRect = knobRt;
        joy.RimImage = rimImg;
        joy.MaxRadius = 72f;
    }

    void CreateCardinalDot(Transform parent, Vector2 pos)
    {
        GameObject dot = new GameObject("Dot", typeof(RectTransform));
        dot.transform.SetParent(parent, false);
        RectTransform rt = dot.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(8f, 8f);
        Image img = dot.AddComponent<Image>();
        img.sprite = GetDiscSprite();
        img.color = new Color(1f, 0.88f, 0.48f, 0.45f);
        img.raycastTarget = false;
    }

    GameObject CreateRoundActionButton(Transform parent, string name, string text,
        Vector2 anchor, Vector2 anchoredPos, float diameter, int fontSize,
        Color fillColor, Color rimColor, Font font,
        out Text labelOut, out Image rimOut, out Image fillOut)
    {
        GameObject btnGo = new GameObject(name, typeof(RectTransform));
        btnGo.transform.SetParent(parent, false);
        RectTransform rt = btnGo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(diameter, diameter);

        fillOut = btnGo.AddComponent<Image>();
        fillOut.sprite = GetDiscSprite();
        fillOut.color = fillColor;
        fillOut.raycastTarget = true;

        GameObject rimGo = new GameObject("Rim", typeof(RectTransform));
        rimGo.transform.SetParent(btnGo.transform, false);
        RectTransform rimRt = rimGo.GetComponent<RectTransform>();
        rimRt.anchorMin = Vector2.zero;
        rimRt.anchorMax = Vector2.one;
        rimRt.offsetMin = Vector2.zero;
        rimRt.offsetMax = Vector2.zero;

        rimOut = rimGo.AddComponent<Image>();
        rimOut.sprite = GetRingSprite();
        rimOut.color = rimColor;
        rimOut.raycastTarget = false;

        GameObject txtGo = ColonialMenuModalHelper.CreateColonialText(
            btnGo.transform, "Label", text, fontSize,
            new Color(1f, 0.96f, 0.84f, 0.98f), TextAnchor.MiddleCenter, font);
        RectTransform txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = new Vector2(6f, 6f);
        txtRt.offsetMax = new Vector2(-6f, -6f);
        labelOut = txtGo.GetComponent<Text>();
        if (labelOut != null) labelOut.lineSpacing = 0.92f;

        return btnGo;
    }

    void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
        {
            ApplySafeArea();
        }

        RefreshVisibility();

        if (gameplayControlsRoot != null && gameplayControlsRoot.activeSelf)
        {
            UpdateContextualButtonLabels();
        }
    }

    void UpdateContextualButtonLabels()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindAnyObjectByType<PlayerMovement>();

        if (cachedPlayer != null && interactLabel != null)
        {
            if (cachedPlayer.IsHidden)
            {
                interactLabel.text = "SALIR\n[E]";
                if (interactRim != null) interactRim.color = new Color(0.40f, 0.92f, 1.0f, 0.96f);
            }
            else
            {
                interactLabel.text = "USAR /\nOCULTAR";
                if (interactRim != null) interactRim.color = new Color(0.95f, 0.78f, 0.32f, 0.92f);
            }
        }
    }

    void RefreshVisibility()
    {
        if (gameplayControlsRoot == null) return;

        bool isPlayingState = GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing;
        bool shouldShow = MobileInputManager.ShouldShowTouchControls && isPlayingState;

        if (gameplayControlsRoot.activeSelf != shouldShow)
        {
            gameplayControlsRoot.SetActive(shouldShow);
            if (!shouldShow)
            {
                MobileInputManager.SetJoystickDirection(Vector2.zero);
                MobileInputManager.SetSprintHeld(false);
            }
        }
    }

    void ApplySafeArea()
    {
        if (safeAreaRect == null || Screen.width <= 0 || Screen.height <= 0) return;

        Rect safe = Screen.safeArea;
        lastSafeArea = safe;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
    }
}

/// <summary>
/// Componente de Joystick Virtual multitáctil con zona muerta calibrada.
/// </summary>
public class MobileVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform BaseRect;
    public RectTransform KnobRect;
    public Image RimImage;
    public float MaxRadius = 72f;
    public float DeadZone = 0.12f;

    int activePointerId = int.MinValue;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue) return;
        activePointerId = eventData.pointerId;
        if (RimImage != null) RimImage.color = new Color(1f, 0.86f, 0.36f, 0.98f);
        UpdateStick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        UpdateStick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        ResetStick();
    }

    void OnDisable()
    {
        ResetStick();
    }

    void UpdateStick(PointerEventData eventData)
    {
        if (BaseRect == null || KnobRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            BaseRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Vector2 clamped = Vector2.ClampMagnitude(localPoint, MaxRadius);
            KnobRect.anchoredPosition = clamped;

            Vector2 normalized = clamped / Mathf.Max(1f, MaxRadius);
            if (normalized.magnitude < DeadZone)
                normalized = Vector2.zero;

            MobileInputManager.SetJoystickDirection(normalized);
        }
    }

    void ResetStick()
    {
        activePointerId = int.MinValue;
        if (KnobRect != null) KnobRect.anchoredPosition = Vector2.zero;
        if (RimImage != null) RimImage.color = new Color(0.88f, 0.70f, 0.28f, 0.78f);
        MobileInputManager.SetJoystickDirection(Vector2.zero);
    }
}

/// <summary>
/// Botón circular táctil de acción instantánea (Salto, Interactuar, Ofrenda, Pausa).
/// </summary>
public class MobileTapButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public System.Action OnTap;
    Vector3 normalScale = Vector3.one;

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = normalScale * 0.92f;
        OnTap?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = normalScale;
    }

    void OnDisable()
    {
        transform.localScale = normalScale;
    }
}

/// <summary>
/// Botón circular táctil de mantener presionado (Correr con estamina).
/// </summary>
public class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public System.Action<bool> OnHoldChanged;
    public Image TargetFill;
    public Color IdleColor = new Color(0.24f, 0.13f, 0.08f, 0.68f);
    public Color PressedColor = new Color(0.58f, 0.32f, 0.10f, 0.92f);

    int activePointerId = int.MinValue;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (activePointerId != int.MinValue) return;
        activePointerId = eventData.pointerId;
        transform.localScale = Vector3.one * 0.93f;
        if (TargetFill != null) TargetFill.color = PressedColor;
        OnHoldChanged?.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;
        ReleaseHold();
    }

    void OnDisable()
    {
        ReleaseHold();
    }

    void ReleaseHold()
    {
        activePointerId = int.MinValue;
        transform.localScale = Vector3.one;
        if (TargetFill != null) TargetFill.color = IdleColor;
        OnHoldChanged?.Invoke(false);
    }
}
