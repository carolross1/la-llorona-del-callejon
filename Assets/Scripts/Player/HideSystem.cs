using UnityEngine;
using UnityEngine.InputSystem;

public class HideSystem : MonoBehaviour
{
    PlayerMovement player;
    SpriteRenderer sr;
    Rigidbody2D rb;
    HideSpot current;

    Vector3 savedPos;
    int savedLayerId;
    int savedOrder;

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;

        if (player.IsHidden) SetHidden(false);
        else if (current != null) SetHidden(true);
    }

    void SetHidden(bool hidden)
    {
        if (hidden)
        {
            savedPos = transform.position;
            savedLayerId = sr.sortingLayerID;
            savedOrder = sr.sortingOrder;

            // Dibujar a Carlos justo debajo del escondite
            var spotSr = current.Sprite;
            if (spotSr != null)
            {
                sr.sortingLayerID = spotSr.sortingLayerID;
                sr.sortingOrder = spotSr.sortingOrder - 1;
            }

            player.IsHidden = true;
            rb.linearVelocity = Vector2.zero;
            Teleport(current.HidePosition);
        }
        else
        {
            player.IsHidden = false;
            sr.sortingLayerID = savedLayerId;
            sr.sortingOrder = savedOrder;
            Teleport(savedPos);
        }
    }

    void Teleport(Vector3 pos)
    {
        pos.z = 0;
        rb.position = pos;
        transform.position = pos;
    }

    void OnTriggerEnter2D(Collider2D c)
    {
        var h = c.GetComponentInParent<HideSpot>();
        if (h != null) current = h;
    }

    void OnTriggerExit2D(Collider2D c)
    {
        if (player.IsHidden) return; // escondido: no salir por accidente
        if (c.GetComponentInParent<HideSpot>() != null) current = null;
    }
}