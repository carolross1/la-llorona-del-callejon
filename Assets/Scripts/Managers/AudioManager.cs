// Assets/Scripts/Managers/AudioManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Administrador centralizado de audio con soporte para música en bucle con crossfade,
/// efectos de sonido (SFX 2D) y llanto posicional 3D para La Llorona con paneo estéreo.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mezclador y Grupos")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] AudioMixerGroup musicGroup;
    [SerializeField] AudioMixerGroup sfxGroup;

    [Header("Clips de Música")]
    public AudioClip musicaMenu;
    public AudioClip musicaNivel;

    [Header("Clips de Efectos (SFX)")]
    public AudioClip lloronaLlanto;
    public AudioClip pasos1;
    public AudioClip escondite;
    public AudioClip vela;
    public AudioClip clicBoton;
    public AudioClip gritoFinal;
    public AudioClip campanaLejana;

    [Header("Canales de Audio")]
    [SerializeField] AudioSource musicSourceA;
    [SerializeField] AudioSource musicSourceB;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource llorona3DSource;

    bool activeIsA = true;
    Coroutine crossfadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSources();
    }

    void Start()
    {
        ApplySavedVolumes();
    }

    void EnsureAudioSources()
    {
        if (musicSourceA == null)
        {
            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceA.loop = true;
            musicSourceA.playOnAwake = false;
        }
        if (musicSourceB == null)
        {
            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceB.loop = true;
            musicSourceB.playOnAwake = false;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (llorona3DSource == null)
        {
            llorona3DSource = gameObject.AddComponent<AudioSource>();
            llorona3DSource.loop = true;
            llorona3DSource.spatialBlend = 1.0f; // Audio 3D posicional
            llorona3DSource.minDistance = 2.0f;
            llorona3DSource.maxDistance = 18.0f;
            llorona3DSource.rolloffMode = AudioRolloffMode.Linear;
            llorona3DSource.playOnAwake = false;
        }
    }

    public void ApplySavedVolumes()
    {
        SetMusicVolume(SaveSystem.GetMusicVolume());
        SetSfxVolume(SaveSystem.GetSfxVolume());
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (audioMixer != null)
        {
            // Conversión de lineal (0-1) a escala logarítmica dB (-80 a 0)
            float db = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat("MusicVol", db);
        }
        else
        {
            if (musicSourceA != null) musicSourceA.volume = volume;
            if (musicSourceB != null) musicSourceB.volume = volume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        if (audioMixer != null)
        {
            float db = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
            audioMixer.SetFloat("SfxVol", db);
        }
        else
        {
            if (sfxSource != null) sfxSource.volume = volume;
            if (llorona3DSource != null) llorona3DSource.volume = volume;
        }
    }

    /// <summary>
    /// Cambia la música ambiental con transición suave (crossfade) de volumen.
    /// </summary>
    public void PlayMusicWithCrossfade(AudioClip newClip, float duration = 1.2f)
    {
        if (newClip == null) return;

        AudioSource current = activeIsA ? musicSourceA : musicSourceB;
        AudioSource target = activeIsA ? musicSourceB : musicSourceA;

        if (current.clip == newClip && current.isPlaying) return;

        if (crossfadeCoroutine != null)
            StopCoroutine(crossfadeCoroutine);

        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(current, target, newClip, duration));
        activeIsA = !activeIsA;
    }

    IEnumerator CrossfadeRoutine(AudioSource from, AudioSource to, AudioClip nextClip, float duration)
    {
        to.clip = nextClip;
        to.volume = 0f;
        to.Play();

        float startVol = from.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(t / duration);
            from.volume = Mathf.Lerp(startVol, 0f, ratio);
            to.volume = Mathf.Lerp(0f, 1f, ratio);
            yield return null;
        }

        from.Stop();
        from.volume = startVol;
        to.volume = 1f;
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void PlayButtonSound() => PlaySFX(clicBoton, 0.8f);
    public void PlayFootstep() => PlaySFX(pasos1, 0.4f);
    public void PlayHideSound() => PlaySFX(escondite, 0.9f);
    public void PlayCandleSound() => PlaySFX(vela, 1.0f);
    public void PlayFinalScream() => PlaySFX(gritoFinal, 1.0f);

    /// <summary>
    /// Inicia el llanto 3D posicional en las coordenadas de La Llorona.
    /// </summary>
    public void StartGhostCry(Transform lloronaTransform)
    {
        if (llorona3DSource == null || lloronaLlanto == null) return;

        llorona3DSource.transform.position = lloronaTransform.position;
        llorona3DSource.clip = lloronaLlanto;
        if (!llorona3DSource.isPlaying)
            llorona3DSource.Play();
    }

    /// <summary>
    /// Actualiza la posición del emisor 3D de llanto.
    /// </summary>
    public void UpdateGhostCryPosition(Vector3 position)
    {
        if (llorona3DSource != null)
            llorona3DSource.transform.position = position;
    }

    public void StopGhostCry()
    {
        if (llorona3DSource != null && llorona3DSource.isPlaying)
            llorona3DSource.Stop();
    }
}
