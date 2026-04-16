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

    // Init 메서드로 안전하게 초기화
    public void Init(ItemData itemData, InventoryGrid grid, Inventory inventory)
    {
        this.itemData = itemData;
        this.grid = grid;
        this.inventory = inventory;

        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (itemData != null && rect != null)
        {
            rect.sizeDelta = new Vector2(
                Mathf.Max(1, itemData.width) * 64f,
                Mathf.Max(1, itemData.height) * 64f
            );
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        originalPos = rect.anchoredPosition;
        originalParent = rect.parent;

        rect.SetParent(canvas.transform);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null || canvas == null) return;
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 끝 -> 원래 부모로 복귀
        rect.SetParent(originalParent);

        RectTransform parentRT = originalParent.GetComponent<RectTransform>();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRT,
            eventData.position,
            eventData.pressEventCamera,
            out localPos
        );

        // GridParent pivot = (0,1) 기준으로 좌표 보정
        // localPos.y는 위가 0, 아래로 갈수록 음수
        // localPos.x는 왼쪽이 0, 오른쪽으로 증가
        localPos.x = Mathf.Clamp(localPos.x, 0, parentRT.sizeDelta.x);
        localPos.y = Mathf.Clamp(localPos.y, -parentRT.sizeDelta.y, 0);

        // 그리드 좌표 변환
        int gridX = Mathf.FloorToInt(localPos.x / 64f);
        int gridY = Mathf.FloorToInt(-localPos.y / 64f);

        // 기존 위치 제거
        grid.RemoveItem(itemData);

        // 배치 가능하면 배치
        if (grid.CanPlaceItem(itemData, gridX, gridY))
        {
            grid.PlaceItem(itemData, gridX, gridY);

            rect.anchoredPosition = new Vector2(
                gridX * 64f,
                -gridY * 64f
            );
        }
        else
        {
            // 실패 ->원래 자리로 복귀
            rect.anchoredPosition = originalPos;

            int ox = Mathf.RoundToInt(originalPos.x / 64f);
            int oy = Mathf.RoundToInt(-originalPos.y / 64f);
            grid.PlaceItem(itemData, ox, oy);
        }
    }

}
