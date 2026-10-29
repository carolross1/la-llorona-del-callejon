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
    public AudioClip musicaNivel1;

    [Header("Clips de Efectos (SFX)")]
    public AudioClip lloronaLlanto;
    public AudioClip pasos1;
    public AudioClip escondite;
    public AudioClip vela;
    public AudioClip clicBoton;
    public AudioClip gritoFinal;
    public AudioClip campanaLejana;

    [Header("Clips Específicos Nivel 1")]
    public AudioClip pasosLentos;
    public AudioClip pasosRapidos;
    public AudioClip perrosAullando;
    public AudioClip sonidoPerro;
    public AudioClip lloronaGrito;
    public AudioClip sonidoVidrio;

    [Header("Ajustes de Mezcla y Volumen")]
    [Range(0f, 1f)] [SerializeField] float musicMasterMix = 0.38f; // Nivel calibrado para evitar empalme con SFX
    [Range(0f, 1f)] [SerializeField] float sfxMasterMix = 1.0f;

    [Header("Canales de Audio")]
    [SerializeField] AudioSource musicSourceA;
    [SerializeField] AudioSource musicSourceB;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource llorona3DSource;

    bool activeIsA = true;
    Coroutine crossfadeCoroutine;
    Coroutine duckCoroutine;
    float currentMusicTargetVol = 0.38f;

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
        LoadDefaultClipsFromResources();
    }

    void LoadDefaultClipsFromResources()
    {
        if (musicaNivel == null) musicaNivel = Resources.Load<AudioClip>("Audio/Music/Nivel1/Breath_Behind_the_Stall");
        if (musicaNivel1 == null) musicaNivel1 = musicaNivel;
        if (pasosLentos == null) pasosLentos = Resources.Load<AudioClip>("Audio/SFX/Nivel1/pasos_lentos");
        if (pasosRapidos == null) pasosRapidos = Resources.Load<AudioClip>("Audio/SFX/Nivel1/pasos_rapidos");
        if (sonidoPerro == null) sonidoPerro = Resources.Load<AudioClip>("Audio/SFX/Nivel1/perro");
        if (sonidoVidrio == null) sonidoVidrio = Resources.Load<AudioClip>("Audio/SFX/Nivel1/vidrio");
        if (lloronaGrito == null) lloronaGrito = Resources.Load<AudioClip>("Audio/SFX/Nivel1/llorona_nivel1.mpeg");
        if (lloronaGrito == null) lloronaGrito = Resources.Load<AudioClip>("Audio/SFX/Nivel1/llorona_nivel1.mpeg.wav");
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
        currentMusicTargetVol = volume * musicMasterMix;

        if (audioMixer != null)
        {
            // Conversión de lineal (0-1) a escala logarítmica dB (-80 a 0)
            float db = currentMusicTargetVol > 0.001f ? Mathf.Log10(currentMusicTargetVol) * 20f : -80f;
            audioMixer.SetFloat("MusicVol", db);
        }
        else
        {
            AudioSource activeSource = activeIsA ? musicSourceA : musicSourceB;
            if (activeSource != null) activeSource.volume = currentMusicTargetVol;
        }
    }

    public void SetSfxVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        float scaledSfx = volume * sfxMasterMix;

        if (audioMixer != null)
        {
            float db = scaledSfx > 0.001f ? Mathf.Log10(scaledSfx) * 20f : -80f;
            audioMixer.SetFloat("SfxVol", db);
        }
        else
        {
            if (sfxSource != null) sfxSource.volume = scaledSfx;
            if (llorona3DSource != null) llorona3DSource.volume = scaledSfx;
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
        float targetVol = currentMusicTargetVol;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float ratio = Mathf.Clamp01(t / duration);
            from.volume = Mathf.Lerp(startVol, 0f, ratio);
            to.volume = Mathf.Lerp(0f, targetVol, ratio);
            yield return null;
        }

        from.Stop();
        from.volume = startVol;
        to.volume = targetVol;
    }

    /// <summary>
    /// Atenúa temporalmente la música de fondo (ducking) cuando ocurre un evento sonoro intenso
    /// (ladrido de perros, grito de la Llorona) para evitar que se empalmen los audios.
    /// </summary>
    public void DuckMusic(float duration = 3.5f, float duckVolumeRatio = 0.45f)
    {
        if (duckCoroutine != null)
            StopCoroutine(duckCoroutine);

        duckCoroutine = StartCoroutine(DuckMusicRoutine(duration, duckVolumeRatio));
    }

    IEnumerator DuckMusicRoutine(float duration, float duckVolumeRatio)
    {
        AudioSource activeSource = activeIsA ? musicSourceA : musicSourceB;
        if (activeSource == null || !activeSource.isPlaying) yield break;

        float normalVol = currentMusicTargetVol;
        float duckedVol = normalVol * Mathf.Clamp01(duckVolumeRatio);

        // Bajada suave
        float t = 0f;
        float fadeTime = 0.3f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            activeSource.volume = Mathf.Lerp(normalVol, duckedVol, t / fadeTime);
            yield return null;
        }
        activeSource.volume = duckedVol;

        // Mantener mientras dura el efecto
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, duration - fadeTime * 2));

        // Subida suave de regreso al volumen normal
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            activeSource.volume = Mathf.Lerp(duckedVol, normalVol, t / fadeTime);
            yield return null;
        }
        activeSource.volume = normalVol;
    }

    /// <summary>
    /// Reproduce la música de Nivel 1 (Breath_Behind_the_Stall) con volumen calibrado sin empalmarse.
    /// </summary>
    public void PlayLevelMusic(AudioClip overrideClip = null)
    {
        AudioClip clipToPlay = overrideClip ?? musicaNivel1 ?? musicaNivel;
        if (clipToPlay != null)
        {
            PlayMusicWithCrossfade(clipToPlay, 1.2f);
        }
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
    public void PlayGlassBreak(float volume = 1.0f) => PlaySFX(sonidoVidrio, volume);
    public void PlayDogBark(float volume = 0.85f) => PlaySFX(sonidoPerro, volume);

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
