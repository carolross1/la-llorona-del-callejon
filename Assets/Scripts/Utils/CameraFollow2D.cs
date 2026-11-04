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
}
