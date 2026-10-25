// Assets/Scripts/Systems/Offering.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Representa una ofrenda (vela encendida) colocada en el suelo para distraer a La Llorona.
/// Si La Llorona entra en su radio de visión/detección, se acerca y se detiene 3 segundos.
/// </summary>
public class Offering : MonoBehaviour
{
    [Header("Configuración de Distracción")]
    [SerializeField] float attractionRadius = 6.0f;
    [SerializeField] float distractionDuration = 3.0f;
    [SerializeField] float lifetimeAfterDistraction = 3.5f;

    [Header("Componentes Visuales")]
    [SerializeField] Light2D candleLight;
    [SerializeField] SpriteRenderer spriteRenderer;

    bool hasTriggered;

    void Start()
    {
        if (candleLight == null) candleLight = GetComponentInChildren<Light2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (hasTriggered) return;

        LloronaAI llorona = FindAnyObjectByType<LloronaAI>();
        if (llorona == null) return;

        float dist = Vector2.Distance(transform.position, llorona.transform.position);
        if (dist <= attractionRadius)
        {
            hasTriggered = true;
            llorona.Distract(transform.position, distractionDuration);
            StartCoroutine(ExtinguishRoutine());
        }
    }

    IEnumerator ExtinguishRoutine()
    {
        yield return new WaitForSeconds(distractionDuration);

        // Apagar luz gradualmente
        if (candleLight != null)
        {
            float startInt = candleLight.intensity;
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                candleLight.intensity = Mathf.Lerp(startInt, 0f, t / 0.5f);
                yield return null;
            }
        }

        Destroy(gameObject, Mathf.Max(0.5f, lifetimeAfterDistraction - distractionDuration));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
