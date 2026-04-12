using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("이 UI가 표현하는 아이템 데이터")]
    public ItemData itemData;

    [Header("백팩 그리드 참조")]
    public InventoryGrid grid;

    [Header("플레이어 인벤토리 참조")]
    public Inventory inventory;

    private RectTransform rect;
    private Canvas canvas;

    private Vector2 originalPos;
    private Transform originalParent;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPos = rect.anchoredPosition;
        originalParent = rect.parent;

        // 드래그 중에는 Canvas 최상단으로 이동
        rect.SetParent(canvas.transform);
    }

    // 드래그 중 위치 이동
    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // 드래그 종료 → 배치 시도
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            originalParent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        // 64px = 한 칸 크기
        int gridX = Mathf.FloorToInt(localPos.x / 64f);
        int gridY = Mathf.FloorToInt(localPos.y / 64f);

        // 배치 가능 여부 검사
        if (grid.CanPlaceItem(itemData, gridX, gridY))
        {
            grid.PlaceItem(itemData, gridX, gridY);

            rect.SetParent(originalParent);
            rect.anchoredPosition = new Vector2(gridX * 64f, gridY * 64f);
        }
        else
        {
            // 배치 실패 → 원래 자리로 복귀
            rect.SetParent(originalParent);
            rect.anchoredPosition = originalPos;
        }
    }
}
