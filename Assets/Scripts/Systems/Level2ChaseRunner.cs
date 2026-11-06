// Assets/Scripts/Systems/Level2ChaseRunner.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controlador de la Persecución Dinámica del Nivel 2 (Callejón del Beso):
/// - Movimiento 100% manual con flechas / WASD.
/// - Carlos camina a paso normal y corre con [Shift] gastando su barra de estamina.
/// - La Llorona persigue detrás a paso constante y flota entre los 4 pisos.
/// - Sistema de 3 Golpes (Obstáculos):
///   - Al chocar con un obstáculo sin saltar, Carlos tropieza, la Llorona gana terreno y suma 1 golpe.
///   - Con 3 golpes o si la Llorona te alcanza a pie (0.8m), te atrapa (Game Over).
///   - Correr limpio durante 6 segundos recupera 1 golpe de distancia.
/// </summary>
public class Level2ChaseRunner : MonoBehaviour
{
    public static Level2ChaseRunner Instance { get; private set; }

    [Header("Velocidades y Distancias")]
    [SerializeField] float baseLloronaSpeed = 3.1f;
    [SerializeField] float strikeSpeedBonus = 0.35f;
    [SerializeField] float catchDistance = 0.85f;
    [SerializeField] float startDistance = 6.5f;
    [SerializeField] float recoveryInterval = 6.0f;
    [SerializeField] int maxStrikes = 3;

    [Header("Referencias")]
    [SerializeField] PlayerMovement player;
    [SerializeField] LloronaAI llorona;
    [SerializeField] CameraFollow2D cameraFollow;

    // Estado interno
    int currentStrikes = 0;
    float timeSinceLastHit = 0f;
    bool isChaseActive = false;
    bool isCaught = false;
    bool isVictory = false;

    // Efectos visuales de Carlos
    SpriteRenderer playerSr;

    // UI Runner Overlay
    GameObject runnerUIOverlay;
    Text statusText;
    Image dangerVignette;

    public int CurrentStrikes => currentStrikes;
    public bool IsActive => isChaseActive;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != "Level2_Callejon")
        {
            enabled = false;
            return;
        }

        InitChase();
    }

    void InitChase()
    {
        if (player == null) player = FindAnyObjectByType<PlayerMovement>();
        if (llorona == null) llorona = FindAnyObjectByType<LloronaAI>();

        if (player != null)
        {
            playerSr = player.GetComponent<SpriteRenderer>();
        }

        // Calibrar cámara con seguimiento suave
        Camera cam = Camera.main;
        if (cam != null)
        {
            cameraFollow = cam.GetComponent<CameraFollow2D>();
            if (cameraFollow != null)
            {
                cameraFollow.SetBounds(new Vector2(-8.0f, 1.40f), new Vector2(58.0f, 10.50f));
            }
        }

        // Posicionar a la Llorona detrás de Carlos
        if (llorona != null && player != null)
        {
            llorona.transform.position = new Vector3(player.transform.position.x - startDistance, player.transform.position.y, 0f);
        }

        currentStrikes = 0;
        timeSinceLastHit = 0f;
        isChaseActive = true;
        isCaught = false;
        isVictory = false;

        CreateRunnerUI();

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡LA LLORONA ESTÁ EN EL CALLEJÓN! ¡HUYE HACIA EL TÚNEL!", 3.5f);
            HUD.Instance.ShowTutorial("[Flechas / WASD] Caminar | [Shift] Correr con Estamina | [Espacio] Saltar | [W/S] Escaleras", 7.0f);
            HUD.Instance.SetEyeAlert(true);
        }
    }

    void CreateRunnerUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        runnerUIOverlay = new GameObject("Runner_Chase_UI_Overlay");
        runnerUIOverlay.transform.SetParent(canvas.transform, false);

        // Viñeta roja de peligro
        GameObject vigObj = new GameObject("Vignette_Danger");
        vigObj.transform.SetParent(runnerUIOverlay.transform, false);
        RectTransform vigRect = vigObj.AddComponent<RectTransform>();
        vigRect.anchorMin = Vector2.zero;
        vigRect.anchorMax = Vector2.one;
        vigRect.sizeDelta = Vector2.zero;

        dangerVignette = vigObj.AddComponent<Image>();
        dangerVignette.color = new Color(0.8f, 0f, 0f, 0f);
        dangerVignette.raycastTarget = false;

        // Banner superior central con indicador de distancia y golpes
        GameObject bannerObj = new GameObject("Banner_Distancia_Llorona");
        bannerObj.transform.SetParent(runnerUIOverlay.transform, false);
        RectTransform bRect = bannerObj.AddComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.5f, 1f);
        bRect.anchorMax = new Vector2(0.5f, 1f);
        bRect.pivot = new Vector2(0.5f, 1f);
        bRect.anchoredPosition = new Vector2(0f, -85f);
        bRect.sizeDelta = new Vector2(430f, 38f);

        Image bBg = bannerObj.AddComponent<Image>();
        bBg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);

        GameObject textObj = new GameObject("Texto_Status");
        textObj.transform.SetParent(bannerObj.transform, false);
        RectTransform tRect = textObj.AddComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.sizeDelta = Vector2.zero;

        statusText = textObj.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statusText.fontSize = 17;
        statusText.fontStyle = FontStyle.Bold;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = new Color(1.0f, 0.85f, 0.2f);
        statusText.text = "DISTANCIA LLORONA: 6.5m  |  GOLPES: 0 / 3";
    }

    void Update()
    {
        if (!isChaseActive || isCaught || isVictory) return;

        if (player == null)
        {
            player = FindAnyObjectByType<PlayerMovement>();
            if (player == null) return;
        }

        // Meta alcanzada: llegada a la Bóveda Colonial segura (X >= 54.5)
        if (player.transform.position.x >= 54.5f)
        {
            TriggerVictory();
            return;
        }

        // Tiempo limpio sin chocar: recupera 1 golpe cada 6 segundos
        timeSinceLastHit += Time.deltaTime;
        if (currentStrikes > 0 && timeSinceLastHit >= recoveryInterval)
        {
            timeSinceLastHit = 0f;
            currentStrikes = Mathf.Max(0, currentStrikes - 1);

            if (HUD.Instance != null)
            {
                HUD.Instance.ShowEventNotice("¡Has ganado distancia con la Llorona!", 2.0f);
            }
        }

        // Avance de la Llorona en persecución continua
        if (llorona != null)
        {
            Vector3 pPos = player.transform.position;

            // La velocidad de la Llorona escala si Carlos acumula golpes
            float currentLloronaSpeed = baseLloronaSpeed + (currentStrikes * strikeSpeedBonus);

            // Si Carlos se aleja demasiado (> 8.5m), la Llorona acelera ligeramente para mantener la tensión
            float currentDist = pPos.x - llorona.transform.position.x;
            if (currentDist > 8.5f)
            {
                currentLloronaSpeed *= 1.35f;
            }

            // Movimiento horizontal continuo hacia Carlos
            float stepX = currentLloronaSpeed * Time.deltaTime;
            float newX = Mathf.MoveTowards(llorona.transform.position.x, pPos.x, stepX);

            // Flotación vertical suave que sigue el piso/escalera en que esté Carlos
            float newY = Mathf.MoveTowards(llorona.transform.position.y, pPos.y, Time.deltaTime * 6.5f);

            llorona.transform.position = new Vector3(newX, newY, 0f);

            // Verificación de distancia de captura
            float realDist = Vector2.Distance(llorona.transform.position, pPos);
            if (realDist <= catchDistance || currentStrikes >= maxStrikes)
            {
                CatchPlayer();
                return;
            }

            UpdateHUD(realDist);
        }
    }

    public void OnObstacleHit(string obstacleName, int strikesToAdd = 1, float stumbleTime = 0.50f)
    {
        if (!isChaseActive || isCaught || isVictory) return;

        currentStrikes = Mathf.Min(maxStrikes, currentStrikes + strikesToAdd);
        timeSinceLastHit = 0f;

        // La Llorona aprovecha el tropiezo y avanza 1.5 metros hacia Carlos
        if (llorona != null && player != null)
        {
            float surgeX = Mathf.Min(player.transform.position.x - 1.2f, llorona.transform.position.x + 1.5f);
            llorona.transform.position = new Vector3(surgeX, llorona.transform.position.y, 0f);
        }

        // Ralentización momentánea de Carlos (tropiezo)
        if (player != null)
        {
            StartCoroutine(StumbleCoroutine(stumbleTime));
        }

        if (currentStrikes >= maxStrikes)
        {
            CatchPlayer();
            return;
        }

        // Alerta en HUD
        string warnMsg = currentStrikes == 1
            ? "¡TROPEZASTE! La Llorona se acerca (1er Golpe)"
            : "¡CUIDADO! ¡La Llorona está a tu espalda! (2do Golpe)";

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice(warnMsg, 2.0f);
        }
    }

    IEnumerator StumbleCoroutine(float duration)
    {
        if (player == null) yield break;

        player.SpeedMultiplier = 0.45f;

        // Parpadeo rojizo en el sprite de Carlos
        if (playerSr != null)
        {
            for (float t = 0; t < duration; t += 0.1f)
            {
                playerSr.color = new Color(1f, 0.35f, 0.35f, 1f);
                yield return new WaitForSeconds(0.05f);
                playerSr.color = Color.white;
                yield return new WaitForSeconds(0.05f);
            }
            playerSr.color = Color.white;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        if (player != null)
        {
            player.SpeedMultiplier = 1.0f;
        }
    }

    void CatchPlayer()
    {
        if (isCaught) return;
        isCaught = true;
        isChaseActive = false;

        if (statusText != null)
        {
            statusText.text = "¡LA LLORONA TE ATRAPÓ!";
            statusText.color = Color.red;
        }

        if (dangerVignette != null)
        {
            dangerVignette.color = new Color(0.9f, 0f, 0f, 0.65f);
        }

        if (llorona != null)
        {
            AudioClip scream = Resources.Load<AudioClip>("Audio/SFX/Nivel1/llorona_nivel1.mpeg");
            if (scream != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(scream, 1.0f);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerCaught();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void TriggerVictory()
    {
        if (isVictory) return;
        isVictory = true;
        isChaseActive = false;

        if (statusText != null)
        {
            statusText.text = "¡ESCAPASTE AL ATRIO!";
            statusText.color = new Color(0.2f, 1.0f, 0.4f);
        }

        if (dangerVignette != null)
        {
            dangerVignette.color = Color.clear;
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡Escapaste de la Llorona por el túnel!", 3.0f);
            HUD.Instance.SetEyeAlert(false);
        }
    }

    void UpdateHUD(float dist)
    {
        if (statusText == null) return;

        string strikeSymbols = "";
        for (int i = 0; i < maxStrikes; i++)
        {
            strikeSymbols += (i < currentStrikes) ? "[X] " : "[ ] ";
        }

        float displayDist = Mathf.Max(0.5f, dist);

        if (currentStrikes == 0 && displayDist > 4.5f)
        {
            statusText.text = $"LLORONA: {displayDist:F1}m  |  GOLPES: {strikeSymbols}";
            statusText.color = new Color(0.4f, 1.0f, 0.4f); // Verde
            if (dangerVignette != null) dangerVignette.color = Color.clear;
        }
        else if (currentStrikes == 1 || (displayDist <= 4.5f && displayDist > 2.5f))
        {
            statusText.text = $"LLORONA: {displayDist:F1}m  |  GOLPES: {strikeSymbols}";
            statusText.color = new Color(1.0f, 0.85f, 0.2f); // Amarillo
            if (dangerVignette != null) dangerVignette.color = new Color(0.8f, 0.3f, 0f, 0.15f);
        }
        else
        {
            float pulse = Mathf.PingPong(Time.time * 4.0f, 1.0f);
            statusText.text = $"¡¡¡PELIGRO CRÍTICO!!! {displayDist:F1}m  |  {strikeSymbols}";
            statusText.color = Color.Lerp(Color.red, Color.yellow, pulse); // Rojo parpadeante
            if (dangerVignette != null) dangerVignette.color = new Color(0.9f, 0f, 0f, 0.25f + pulse * 0.20f);
        }
    }

    void OnDisable()
    {
        if (runnerUIOverlay != null)
        {
            Destroy(runnerUIOverlay);
        }
    }
}
