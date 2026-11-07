using UnityEngine;

public class HideSpot : MonoBehaviour
{
    public enum SpotType { Sacos, Barril, Caja, Porton, Puesto }

    public SpotType spotType = SpotType.Sacos;
    public bool canExitAnytime = true;

    [SerializeField] Transform hidePoint;           // arrastra aquí HidePoint
    [SerializeField] SpriteRenderer spriteRenderer; // opcional, se busca solo

    public SpriteRenderer Sprite => spriteRenderer;

    public Vector3 HidePosition =>
        hidePoint != null ? hidePoint.position : GetComponent<Collider2D>().bounds.center;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (hidePoint == null)
        {
            var hp = transform.Find("HidePoint");
            if (hp != null) hidePoint = hp;
        }
    }
}