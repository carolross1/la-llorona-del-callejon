// Assets/Scripts/Systems/LightFlicker.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Simula el parpadeo tenue e irregular de faroles coloniales y velas
/// mediante ruido de Perlin y caídas estocásticas de intensidad en Light2D.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class LightFlicker : MonoBehaviour
{
    [Header("Intensidad")]
    [SerializeField] float baseIntensity = 1.2f;
    [SerializeField] float flickerRange = 0.25f;
    [SerializeField] float flickerSpeed = 8.0f;

    [Header("Parpadeo Acentuado (Apagón Breve)")]
    [SerializeField] bool enableDramaticDrops = true;
    [Range(0f, 0.2f)]
    [SerializeField] float dropProbability = 0.03f;
    [SerializeField] float dropDuration = 0.12f;

    Light2D light2d;
    float seed;
    bool isDropping;

    void Awake()
    {
        light2d = GetComponent<Light2D>();
        seed = Random.value * 100f;
        if (light2d != null && baseIntensity <= 0f)
            baseIntensity = light2d.intensity;
    }

    void Update()
    {
        if (light2d == null || isDropping) return;

        // Oscilación suave con Perlin Noise
        float noise = Mathf.PerlinNoise(seed, Time.time * flickerSpeed);
        float current = baseIntensity + (noise - 0.5f) * 2f * flickerRange;
        light2d.intensity = Mathf.Max(0.1f, current);

        // Ocasional caída abrupta de intensidad (efecto de viento o falla eléctrica colonial)
        if (enableDramaticDrops && Random.value < dropProbability * Time.deltaTime * 60f)
        {
            StartCoroutine(DramaticDropRoutine());
        }
    }

    IEnumerator DramaticDropRoutine()
    {
        isDropping = true;
        float original = light2d.intensity;
        light2d.intensity = original * 0.2f;

        yield return new WaitForSeconds(dropDuration);

        light2d.intensity = original;
        isDropping = false;
    }
}
