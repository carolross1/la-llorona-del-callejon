// Assets/Scripts/Systems/StrayDogAI.cs
using UnityEngine;

/// <summary>
/// Enemigo secundario: Perro callejero territorial (según especificación Sección 8 de PROMPT_VISUAL.md).
/// Duerme en una esquina del mercado o callejón.
/// Estados:
///   - Dormido: escala (1, 0.7), tinte oscuro, respiración leve oscilando en Y, texto 'Z'.
///   - Alerta / Ladrando: Carlos corre a < 4 u o pasa a < 2.5 u sin sigilo. Sacudida horizontal,
///     alerta durante 4 s a La Llorona enviándola a ese punto exacto.
///   - Redormido: tras 8 s sin ruido cerca o si Carlos se esconde con [E].
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class StrayDogAI : MonoBehaviour
{
    [Header("Rangos de Detección (Sección 8)")]
    [SerializeField] float runWakeRadius = 4.0f;     // Carlos corre a menos de 4 u
    [SerializeField] float walkWakeRadius = 2.5f;    // Pasa a menos de 2.5 u a velocidad mayor que caminar
    [SerializeField] float alertLloronaRadius = 25.0f;
    [SerializeField] float barkDuration = 4.0f;      // Ladrido dura 4 s
    [SerializeField] float returnToSleepTime = 8.0f; // Vuelve a dormir tras 8 s sin ruido

    [Header("Sprites de Referencia (4 Vistas de perro.png)")]
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Sprite lateralView;
    [SerializeField] Sprite frontView;
    [SerializeField] Sprite backView;

    [Header("Visual Dormido / Alerta")]
    [SerializeField] Color sleepColor = new Color(0.98f, 0.92f, 0.82f, 1f); // Tono cálido natural para no perderse en el suelo
    [SerializeField] Color alertColor = new Color(1.15f, 1.10f, 1.0f, 1f);

    [Header("Audio SFX (Nivel 1)")]
    [SerializeField] AudioClip barkSfx;
    [Range(0f, 1f)] [SerializeField] float barkVolume = 0.85f;
    [SerializeField] AudioSource audioSource;

    enum DogState { Sleeping, Barking }
    DogState currentState = DogState.Sleeping;

    PlayerMovement player;
    float stateTimer = 0f;
    float lastDisturbanceTime = -99f;
    Vector3 basePosition;
    TextMesh zTextMesh;
    GameObject alertIconGo;

    void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        EnsureAudioSource();

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
        }
        transform.rotation = Quaternion.identity;
        basePosition = transform.position;

        CreateFloatingZ();
        CreateAlertExclamation();

        SetSleepingVisuals();
    }

    void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0.5f; // Paneo estéreo con volumen claro en toda la zona
        audioSource.minDistance = 3.0f;
        audioSource.maxDistance = 25.0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    void Start()
    {
        player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (player == null)
        {
            player = UnityEngine.Object.FindAnyObjectByType<PlayerMovement>();
            if (player == null) return;
        }

        float dist = Vector2.Distance(transform.position, player.transform.position);
        bool playerHidden = player.IsHidden;
        bool playerRunning = player.IsRunning;

        // Orientar el sprite hacia Carlos
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (player.transform.position.x < transform.position.x);
        }

        switch (currentState)
        {
            case DogState.Sleeping:
                UpdateSleeping(dist, playerHidden, playerRunning);
                break;

            case DogState.Barking:
                UpdateBarking(dist, playerHidden, playerRunning);
                break;
        }
    }

    void UpdateSleeping(float dist, bool playerHidden, bool playerRunning)
    {
        // Animación procedural de respiración durmiendo con presencia visual sólida
        float breatheY = 1.05f + Mathf.Sin(Time.time * 2.2f) * 0.05f;
        transform.localScale = new Vector3(1.35f, breatheY, 1.0f);

        // Animación de texto flotante 'Z z z' luminoso y claro
        if (zTextMesh != null)
        {
            zTextMesh.gameObject.SetActive(true);
            float zY = 1.15f + Mathf.PingPong(Time.time * 0.5f, 0.35f);
            zTextMesh.transform.localPosition = new Vector3(0.2f, zY, 0f);
        }

        if (alertIconGo != null) alertIconGo.SetActive(false);

        // Despertar si Carlos corre a < 4 u o pasa a < 2.5 u sin sigilo (no escondido)
        if (!playerHidden)
        {
            if ((playerRunning && dist <= runWakeRadius) || (dist <= walkWakeRadius))
            {
                WakeUpAndBark();
            }
        }
    }

    void UpdateBarking(float dist, bool playerHidden, bool playerRunning)
    {
        stateTimer += Time.deltaTime;

        // Sacudida horizontal rápida ladrando
        float shakeX = Mathf.Sin(Time.time * 28f) * 0.08f;
        transform.position = basePosition + new Vector3(shakeX, 0f, 0f);
        transform.localScale = new Vector3(1.35f, 1.35f, 1.0f);

        if (zTextMesh != null) zTextMesh.gameObject.SetActive(false);
        if (alertIconGo != null) alertIconGo.SetActive(true);

        if (playerRunning || (dist <= runWakeRadius && !playerHidden))
        {
            lastDisturbanceTime = Time.time;
        }

        // Si Carlos se esconde con [E], lo despista
        if (playerHidden)
        {
            ReturnToSleep();
            return;
        }

        // Tras 8 s sin ruido cerca, vuelve a dormir
        if (stateTimer >= barkDuration && (Time.time - lastDisturbanceTime >= returnToSleepTime))
        {
            ReturnToSleep();
        }
    }

    void WakeUpAndBark()
    {
        currentState = DogState.Barking;
        stateTimer = 0f;
        lastDisturbanceTime = Time.time;

        if (spriteRenderer != null)
        {
            if (lateralView != null) spriteRenderer.sprite = lateralView;
            spriteRenderer.color = alertColor;
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡El perro callejero despertó y comenzó a ladrar!");
        }

        // Reproducir audio de perro
        if (barkSfx == null && AudioManager.Instance != null)
        {
            barkSfx = AudioManager.Instance.sonidoPerro ?? AudioManager.Instance.perrosAullando;
        }
        if (barkSfx == null)
        {
            barkSfx = Resources.Load<AudioClip>("Audio/SFX/Nivel1/perro");
        }

        if (audioSource != null && barkSfx != null)
        {
            audioSource.clip = barkSfx;
            audioSource.volume = barkVolume;
            audioSource.loop = true;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        // Atenuar música para que el aullido resalte con impacto
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.DuckMusic(4.0f, 0.40f);
        }

        // Alertar a La Llorona hacia este punto exacto
        LloronaAI llorona = UnityEngine.Object.FindAnyObjectByType<LloronaAI>();
        if (llorona != null)
        {
            float distToLlorona = Vector2.Distance(transform.position, llorona.transform.position);
            if (distToLlorona <= alertLloronaRadius)
            {
                llorona.AlertNoise(transform.position, 6.0f);
            }
        }
    }

    void ReturnToSleep()
    {
        currentState = DogState.Sleeping;
        transform.position = basePosition;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        SetSleepingVisuals();
    }

    void OnDisable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    void SetSleepingVisuals()
    {
        if (spriteRenderer != null)
        {
            if (lateralView != null) spriteRenderer.sprite = lateralView;
            spriteRenderer.color = sleepColor;
        }
        transform.localScale = new Vector3(1.35f, 1.05f, 1.0f);
        if (alertIconGo != null) alertIconGo.SetActive(false);
        if (zTextMesh != null) zTextMesh.gameObject.SetActive(true);
    }

    void CreateFloatingZ()
    {
        GameObject zGo = new GameObject("TextoZ");
        zGo.transform.SetParent(transform, false);
        zGo.transform.localPosition = new Vector3(0.2f, 1.15f, 0f);
        zTextMesh = zGo.AddComponent<TextMesh>();
        zTextMesh.text = "Z z z";
        zTextMesh.fontSize = 32;
        zTextMesh.characterSize = 0.12f;
        zTextMesh.anchor = TextAnchor.MiddleCenter;
        zTextMesh.color = new Color(0.55f, 0.90f, 1.0f, 0.95f); // Celeste brillante luminoso
        var mr = zGo.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 15;
    }

    void CreateAlertExclamation()
    {
        alertIconGo = new GameObject("IconoAlerta");
        alertIconGo.transform.SetParent(transform, false);
        alertIconGo.transform.localPosition = new Vector3(0f, 1.45f, 0f);
        TextMesh tm = alertIconGo.AddComponent<TextMesh>();
        tm.text = "!";
        tm.fontSize = 46;
        tm.characterSize = 0.16f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(1.0f, 0.15f, 0.15f, 1.0f);
        var mr = alertIconGo.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = 15;
        alertIconGo.SetActive(false);
    }
}
