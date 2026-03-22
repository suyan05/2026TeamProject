// UI 아이템 드래그 앤 드롭
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas canvas;
    private RectTransform rect;
    private CanvasGroup group;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        group.alpha = 0.6f;
        group.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.alpha = 1f;
        group.blocksRaycasts = true;
    }
}