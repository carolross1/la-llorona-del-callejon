// Assets/Scripts/Systems/SacredOfferingPickup.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ítem interactivo en el Panteón (Nivel 4 Clímax):
/// - Representa las Flores Sagradas de Cempasúchil y Veladoras consagradas.
/// - Carlos debe buscar y recoger esta ofrenda con [E] para llevarla a la Tumba de La Llorona.
/// - Cuenta con levitación suave y una columna de luz dorada visible.
/// </summary>
public class SacredOfferingPickup : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] float interactRadius = 2.6f;
    [SerializeField] AudioClip pickupSfx;
    [SerializeField] GameObject promptObject;
    [SerializeField] TextMesh promptTextMesh;
    [SerializeField] Light2D itemLight;

    Transform playerTransform;
    float startY;
    bool isPickedUp = false;

    void Start()
    {
        startY = transform.position.y;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (promptObject != null) promptObject.SetActive(false);
        if (promptTextMesh != null)
        {
            promptTextMesh.text = "✦ Ofrenda Sagrada de Cempasúchil ✦\n[E] Tomar Flores y Veladoras";
            promptTextMesh.color = new Color(1.0f, 0.88f, 0.25f);
        }
    }

    void Update()
    {
        if (isPickedUp) return;

        // Levitación flotante suave
        float newY = startY + Mathf.Sin(Time.time * 3.0f) * 0.15f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool inRange = dist <= interactRadius;

        if (promptObject != null && promptObject.activeSelf != inRange)
        {
            promptObject.SetActive(inRange);
        }

        if (inRange)
        {
            var kb = Keyboard.current;
            if (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            {
                TakeOffering();
            }
        }
    }

    void TakeOffering()
    {
        if (isPickedUp) return;
        isPickedUp = true;

        if (pickupSfx != null)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(pickupSfx, 1.0f);
            else AudioSource.PlayClipAtPoint(pickupSfx, transform.position, 1.0f);
        }

        if (LloronaGraveOffering.Instance != null)
        {
            LloronaGraveOffering.Instance.PickUpOffering();
        }

        if (HUD.Instance != null)
        {
            HUD.Instance.ShowEventNotice("¡CONSEGUISTE LA OFRENDA SAGRADA!\nLlévala a la Tumba de La Llorona para concederle el descanso eterno.", 5.5f);
        }

        gameObject.SetActive(false);
    }
}
