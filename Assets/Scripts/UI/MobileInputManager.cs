// Assets/Scripts/UI/MobileInputManager.cs
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum MobileControlsMode
{
    Auto = 0,
    AlwaysShow = 1,
    AlwaysHide = 2
}

/// <summary>
/// Gestor centralizado de entrada táctil para dispositivos móviles (Android, iOS y navegadores WebGL móviles).
/// Se inicializa automáticamente sin necesidad de modificar archivos de escena (.unity),
/// sincroniza pulsaciones de 1 frame antes de todos los scripts de juego y administra la visibilidad del HUD táctil.
/// </summary>
[DefaultExecutionOrder(-1000)]
public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }

    public static event Action OnVisibilityChanged;

    // Estado continuo (Joystick y Sprint mantenido)
    public static Vector2 JoystickDirection { get; private set; }
    public static bool SprintHeld { get; private set; }

    // Pulsaciones de un solo frame (equivalentes a wasPressedThisFrame)
    public static bool JumpPressedThisFrame { get; private set; }
    public static bool InteractPressedThisFrame { get; private set; }
    public static bool ActionFPressedThisFrame { get; private set; }
    public static bool PausePressedThisFrame { get; private set; }

    static bool pendingJump;
    static bool pendingInteract;
    static bool pendingActionF;
    static bool pendingPause;
    static bool hasDetectedTouchInput;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoBootstrap()
    {
        EnsureInstance();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterFirstSceneLoaded()
    {
        EnsureInstance();
        if (Instance != null)
            Instance.EnsureMobileHUDForActiveScene();
    }

    public static void EnsureInstance()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("MobileInputManager");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<MobileInputManager>();
    }

    /// <summary>
    /// Determina si el dispositivo actual es móvil o ha recibido entrada táctil real (ideal para WebGL en celular/tablet).
    /// </summary>
    public static bool IsMobileOrTouchDevice
    {
        get
        {
            if (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld)
                return true;

            return hasDetectedTouchInput;
        }
    }

    /// <summary>
    /// Determina si los controles táctiles en pantalla deben mostrarse según la preferencia en Opciones.
    /// </summary>
    public static bool ShouldShowTouchControls
    {
        get
        {
            MobileControlsMode mode = (MobileControlsMode)SaveSystem.GetMobileControlsMode();
            switch (mode)
            {
                case MobileControlsMode.AlwaysShow:
                    return true;
                case MobileControlsMode.AlwaysHide:
                    return false;
                default:
                    return IsMobileOrTouchDevice;
            }
        }
    }

    public static string GetCurrentModeDisplayLabel()
    {
        MobileControlsMode mode = (MobileControlsMode)SaveSystem.GetMobileControlsMode();
        switch (mode)
        {
            case MobileControlsMode.AlwaysShow:
                return "SIEMPRE VISIBLES";
            case MobileControlsMode.AlwaysHide:
                return "OCULTOS";
            default:
                return "AUTOMÁTICO";
        }
    }

    public static void CycleControlsMode()
    {
        int next = (SaveSystem.GetMobileControlsMode() + 1) % 3;
        SaveSystem.SetMobileControlsMode(next);
    }

    public static void NotifyVisibilityModeChanged()
    {
        EnsureInstance();
        if (Instance != null)
            Instance.EnsureMobileHUDForActiveScene();
        OnVisibilityChanged?.Invoke();
    }

    // Métodos llamados desde los controles UI táctiles
    public static void SetJoystickDirection(Vector2 dir)
    {
        JoystickDirection = Vector2.ClampMagnitude(dir, 1f);
    }

    public static void SetSprintHeld(bool held)
    {
        SprintHeld = held;
    }

    public static void TriggerJump()
    {
        pendingJump = true;
    }

    public static void TriggerInteract()
    {
        pendingInteract = true;
    }

    public static void TriggerActionF()
    {
        pendingActionF = true;
    }

    public static void TriggerPause()
    {
        pendingPause = true;
    }

    public static void ResetAllInputs()
    {
        JoystickDirection = Vector2.zero;
        SprintHeld = false;
        pendingJump = false;
        pendingInteract = false;
        pendingActionF = false;
        pendingPause = false;
        JumpPressedThisFrame = false;
        InteractPressedThisFrame = false;
        ActionFPressedThisFrame = false;
        PausePressedThisFrame = false;
    }

    /// <summary>
    /// Verifica si una posición en pantalla o toque está encima de un botón/joystick de la interfaz UI.
    /// Evita que tocar botones táctiles dispare proyectiles por accidente en el Nivel 4.
    /// </summary>
    public static bool IsScreenPointOverUI(Vector2 screenPos, int pointerId = -1)
    {
        if (EventSystem.current == null) return false;

        if (pointerId >= 0 && EventSystem.current.IsPointerOverGameObject(pointerId))
            return true;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPos
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Forzar orientación horizontal automática cuando corre en dispositivo móvil nativo
        if (Application.isMobilePlatform)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetAllInputs();
        EnsureMobileHUDForActiveScene();
    }

    void EnsureMobileHUDForActiveScene()
    {
        // Solo crear el HUD táctil en escenas jugables donde exista Carlos (PlayerMovement)
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Include);
        if (player == null) return;

        if (MobileControlsUI.Instance == null)
        {
            GameObject uiGo = new GameObject("MobileControlsUI");
            uiGo.AddComponent<MobileControlsUI>();
        }
    }

    void Update()
    {
        // Detectar primer toque real en pantalla (indispensable para navegadores móviles en Itch.io / WebGL)
        if (!hasDetectedTouchInput && Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press.isPressed)
            {
                hasDetectedTouchInput = true;
                EnsureMobileHUDForActiveScene();
                OnVisibilityChanged?.Invoke();
            }
        }

        // Publicar pulsaciones de un solo frame para que TODOS los scripts las lean en este mismo ciclo Update()
        JumpPressedThisFrame = pendingJump;
        InteractPressedThisFrame = pendingInteract;
        ActionFPressedThisFrame = pendingActionF;
        PausePressedThisFrame = pendingPause;

        pendingJump = false;
        pendingInteract = false;
        pendingActionF = false;
        pendingPause = false;
    }
}
