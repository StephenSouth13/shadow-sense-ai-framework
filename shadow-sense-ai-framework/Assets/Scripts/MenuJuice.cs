using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Professional UI "Juice" component.
/// Provides procedural sine-wave floating and interactive hover scaling for UI elements.
/// </summary>
public class MenuJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Floating Animation")]
    public bool enableFloating = false;
    public float floatAmplitude = 10f;
    public float floatFrequency = 1f;

    [Header("Hover Interaction")]
    public bool enableHoverScale = false;
    public float hoverScaleTarget = 1.1f;
    public float transitionSpeed = 10f;

    private RectTransform rectTransform;
    private Vector2 initialAnchoredPos;
    private Vector3 initialScale;
    private Vector3 targetScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialAnchoredPos = rectTransform.anchoredPosition;
        initialScale = transform.localScale;
        targetScale = initialScale;
    }

    private void Update()
    {
        if (enableFloating)
        {
            float newY = initialAnchoredPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            rectTransform.anchoredPosition = new Vector2(initialAnchoredPos.x, newY);
        }

        if (enableHoverScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enableHoverScale)
        {
            targetScale = initialScale * hoverScaleTarget;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (enableHoverScale)
        {
            targetScale = initialScale;
        }
    }
}
