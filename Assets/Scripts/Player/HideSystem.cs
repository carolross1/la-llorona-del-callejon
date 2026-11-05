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
    RigidbodyConstraints2D savedConstraints;

    public bool HasHideSpotNearby => current != null;

    void Awake()
    {
        player = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
        }
        savedConstraints = RigidbodyConstraints2D.FreezeRotation;
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Tecla [E] estándar para esconderse o salir del escondite en todos los niveles (también acepta [C])
        bool hideTriggered = kb.eKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame;

        if (hideTriggered)
        {
            if (player != null && player.IsHidden)
            {
                SetHidden(false);
            }
            else if (current != null)
            {
                SetHidden(true);
            }
        }
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
            rb.constraints = RigidbodyConstraints2D.FreezeAll;   // quieto mientras esta escondido

            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("[E] Salir del escondite", 3.0f);
        }
        else
        {
            rb.constraints = savedConstraints;
            player.IsHidden = false;
            sr.sortingLayerID = savedLayerId;
            sr.sortingOrder = savedOrder;
            Teleport(savedPos);
            transform.rotation = Quaternion.identity;

            if (HUD.Instance != null)
                HUD.Instance.ShowEventNotice("Has salido del escondite", 1.8f);
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
        if (h != null)
        {
            current = h;
            if (player != null && !player.IsHidden && HUD.Instance != null)
            {
                HUD.Instance.ShowEventNotice("[E] Esconderse", 2.0f);
            }
        }
    }

    void OnTriggerExit2D(Collider2D c)
    {
        if (player != null && player.IsHidden) return; // escondido: no salir por accidente
        if (c.GetComponentInParent<HideSpot>() != null)
        {
            current = null;
        }
    }
}
