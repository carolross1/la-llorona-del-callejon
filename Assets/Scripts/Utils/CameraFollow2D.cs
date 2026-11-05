// Assets/Scripts/Utils/CameraFollow2D.cs
using UnityEngine;

/// <summary>
/// Control de cámara 2D suave con límites ortográficos para confinar la vista dentro del mapa.
/// Tamaño ortográfico 5, Z = -10, sin Pixel Perfect Camera (según especificación).
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 1.1f, -10f);
    [SerializeField] float smoothTime = 0.25f;

    [Header("Límites del Mapa (Confiner)")]
    [SerializeField] bool useBounds = true;
    [SerializeField] Vector2 minBounds = new Vector2(-6.0f, 1.25f);
    [SerializeField] Vector2 maxBounds = new Vector2(45.0f, 3.5f);

    [Header("Zoom")]
    [SerializeField] float orthoSize = 2.85f;

    [Header("Zoom Dinámico / Panorámico")]
    [SerializeField] bool dynamicZoom = false;
    [SerializeField] float zoomMinY = 13.0f;
    [SerializeField] float zoomMaxY = 21.0f;
    [SerializeField] float minOrthoSize = 3.6f;
    [SerializeField] float maxOrthoSize = 4.7f;
    [SerializeField] float normalOffsetY = 1.1f;
    [SerializeField] float peakOffsetY = 2.6f;

    Camera cam;
    Vector3 velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            if (orthoSize > 0f)
            {
                cam.orthographicSize = orthoSize;
            }
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
        }
    }

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }

        if (target != null)
        {
            Vector3 startPos = target.position + offset;
            if (useBounds)
            {
                startPos.x = Mathf.Clamp(startPos.x, minBounds.x, maxBounds.x);
                startPos.y = Mathf.Clamp(startPos.y, minBounds.y, maxBounds.y);
            }
            startPos.z = -10f;
            transform.position = startPos;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (dynamicZoom)
        {
            float t = Mathf.Clamp01((target.position.y - zoomMinY) / Mathf.Max(0.1f, zoomMaxY - zoomMinY));
            float targetSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, t);
            if (cam == null) cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * 3f);
            }
            offset.y = Mathf.Lerp(normalOffsetY, peakOffsetY, t);
        }

        Vector3 targetPos = target.position + offset;

        if (useBounds)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
            targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
        }
        targetPos.z = -10f;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    /// <summary>
    /// Configura los límites del mapa en tiempo de diseño o ejecución.
    /// </summary>
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = true;
    }

    /// <summary>
    /// Habilita el zoom y encuadre dinámico al acercarse a zonas monumentales (como la catedral).
    /// </summary>
    public void EnableDynamicZoom(float minY, float maxY, float minSize, float maxSize, float normalOffY = 1.1f, float peakOffY = 2.6f)
    {
        dynamicZoom = true;
        zoomMinY = minY;
        zoomMaxY = maxY;
        minOrthoSize = minSize;
        maxOrthoSize = maxSize;
        normalOffsetY = normalOffY;
        peakOffsetY = peakOffY;
    }

    /// <summary>
    /// Configura el tamaño ortográfico (zoom) de la cámara.
    /// </summary>
    public void SetOrthoSize(float size)
    {
        orthoSize = size;
        if (cam == null) cam = GetComponent<Camera>();
        if (cam != null) cam.orthographicSize = size;
    }

    /// <summary>
    /// Configura el desplazamiento respecto al objetivo.
    /// </summary>
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
