// Assets/Scripts/Player/PlayerAnimation.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour
{
    [Header("Audio de Pasos (Nivel 1)")]
    [SerializeField] AudioClip pasosLentos;
    [SerializeField] AudioClip pasosRapidos;
    [Range(0f, 1f)] [SerializeField] float footstepVolume = 0.65f;
    [SerializeField] AudioSource footstepSource;

    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sr;
    PlayerMovement movement;
    Vector2 lastDir = Vector2.down;

    void Awake()
    {
        EnsureAnimator();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();

        EnsureFootstepSource();
    }

    public void EnsureAnimator()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        if (anim == null)
            anim = gameObject.AddComponent<Animator>();

        if (anim.runtimeAnimatorController == null)
        {
            anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("PlayerController");
            if (anim.runtimeAnimatorController == null)
                anim.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/Player/PlayerController");

#if UNITY_EDITOR
            if (anim.runtimeAnimatorController == null)
                anim.runtimeAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Player/PlayerController.controller");
#endif
        }
    }

    void EnsureFootstepSource()
    {
        if (footstepSource == null)
        {
            footstepSource = GetComponent<AudioSource>();
            if (footstepSource == null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
            }
        }
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.spatialBlend = 0f; // Audio 2D nitido para el jugador
    }

    void Update()
    {
        Vector2 inputDir = (movement != null) ? movement.MoveInput : Vector2.zero;
        Vector2 vel = (rb != null) ? rb.linearVelocity : Vector2.zero;

        bool isMoving = inputDir.sqrMagnitude > 0.01f || vel.sqrMagnitude > 0.01f;
        Vector2 activeDir = (inputDir.sqrMagnitude > 0.01f) ? inputDir : vel;

        if (isMoving)
        {
            lastDir = Mathf.Abs(activeDir.x) > Mathf.Abs(activeDir.y)
                ? new Vector2(Mathf.Sign(activeDir.x), 0)
                : new Vector2(0, Mathf.Sign(activeDir.y));
        }

        // Carlos cuenta con sprites nativos para todas las direcciones (fila 2: izquierda, fila 3: derecha).
        // flipX debe permanecer siempre en false para no invertir ni quebrar la animacion.
        if (sr != null && sr.flipX)
        {
            sr.flipX = false;
        }

        if (anim == null || anim.runtimeAnimatorController == null)
        {
            EnsureAnimator();
        }

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            float speed = isMoving ? Mathf.Max(vel.magnitude, 1.0f) : 0f;

            anim.SetFloat("MoveX", lastDir.x);
            anim.SetFloat("MoveY", lastDir.y);
            anim.SetFloat("Speed", speed);
        }

        UpdateFootstepAudio(isMoving, vel);
    }

    void UpdateFootstepAudio(bool isMoving, Vector2 vel)
    {
        if (footstepSource == null) return;

        bool isHidden = (movement != null && movement.IsHidden);
        bool canPlay = isMoving && !isHidden && (GameManager.Instance == null || GameManager.Instance.CurrentState == GameState.Playing);

        if (!canPlay)
        {
            if (footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        // Si pisa el lodo (SpeedMultiplier < 0.9f) va lento -> pasos lentos
        // Si corre a alta velocidad sin impedimento -> pasos rapidos
        bool inMud = (movement != null && movement.SpeedMultiplier < 0.9f);
        bool isRunningFast = (movement != null && movement.IsRunning && !inMud && vel.magnitude > 3.2f);

        if (pasosLentos == null && AudioManager.Instance != null) pasosLentos = AudioManager.Instance.pasosLentos;
        if (pasosRapidos == null && AudioManager.Instance != null) pasosRapidos = AudioManager.Instance.pasosRapidos;

        if (pasosLentos == null) pasosLentos = Resources.Load<AudioClip>("Audio/SFX/Nivel1/pasos_lentos");
        if (pasosRapidos == null) pasosRapidos = Resources.Load<AudioClip>("Audio/SFX/Nivel1/pasos_rapidos");

        AudioClip targetClip = isRunningFast ? pasosRapidos : pasosLentos;
        if (targetClip == null) return;

        float currentPitch = inMud ? 0.92f : 1.0f;

        if (footstepSource.clip != targetClip)
        {
            footstepSource.clip = targetClip;
            footstepSource.loop = true;
            footstepSource.volume = footstepVolume;
            footstepSource.pitch = currentPitch;
            footstepSource.Play();
        }
        else
        {
            footstepSource.volume = footstepVolume;
            footstepSource.pitch = currentPitch;
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
    }
}
