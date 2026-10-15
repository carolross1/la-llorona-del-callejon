using UnityEngine;

public class HideSpot : MonoBehaviour
{
    [SerializeField] Transform hidePoint;          // arrastra aquí HidePoint
    [SerializeField] SpriteRenderer spriteRenderer; // opcional, se busca solo

    public SpriteRenderer Sprite => spriteRenderer;

    public Vector3 HidePosition =>
        hidePoint != null ? hidePoint.position : GetComponent<Collider2D>().bounds.center;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
}