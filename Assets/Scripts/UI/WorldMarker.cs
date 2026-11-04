// Assets/Scripts/UI/WorldMarker.cs
using UnityEngine;

/// <summary>
/// Marcador visual flotante en el mundo para indicar la Meta, Escaleras y Escondites.
/// Realiza una animación de oscilación vertical suave (bobbing) y brillo para máxima visibilidad.
/// </summary>
public class WorldMarker : MonoBehaviour
{
    [Header("Ajustes de Animación")]
    [SerializeField] float bobSpeed = 3.0f;
    [SerializeField] float bobHeight = 0.15f;
    [SerializeField] bool showOnlyWhenPlayerNear = false;
    [SerializeField] float triggerDistance = 3.5f;

    Vector3 initialPos;
    Transform player;
    SpriteRenderer sr;

    void Awake()
    {
        initialPos = transform.position;
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        // Animación de flotación suave
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = initialPos + new Vector3(0f, offset, 0f);

        // Si es condicional por proximidad (ej. [E] Esconderse)
        if (showOnlyWhenPlayerNear && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            bool isNear = dist <= triggerDistance;
            if (sr != null)
            {
                sr.enabled = isNear;
            }
            foreach (var t in GetComponentsInChildren<TextMesh>())
            {
                t.color = new Color(t.color.r, t.color.g, t.color.b, isNear ? 1f : 0f);
            }
        }
    }
}
