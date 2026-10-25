// Assets/Scripts/UI/ButtonHoverEffect.cs
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Proporciona retroalimentación interactiva suave a los botones de la interfaz:
/// Escala 1.05x al pasar el cursor (hover) y reproduce sonido de clic si AudioManager está disponible.
/// </summary>
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] float hoverScale = 1.05f;
    Vector3 originalScale = Vector3.one;
    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null) originalScale = rt.localScale;
    }

    void OnDisable()
    {
        if (rt != null) rt.localScale = originalScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rt != null) rt.localScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rt != null) rt.localScale = originalScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonSound();
        }
    }
}
