using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
                                IPointerEnterHandler, IPointerExitHandler
{
    public ItemInstance itemInstance;
    public Inventory inventory;
    public InventoryUI inventoryUI;

    public Image icon;

    public int originalX;
    public int originalY;

    Transform originalParent;
    EquipmentSlotUI fromEquipmentSlot;
    Coroutine tooltipRoutine;

    // 장비칸에 드롭되었는지 체크
    public bool droppedOnEquipment = false;

    public void SetItem(ItemInstance instance)
    {
        itemInstance = instance;
        icon.sprite = instance.data.icon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedOnEquipment = false;
        originalParent = transform.parent;

        // 장비 슬롯에서 드래그 시작했는지 체크
        fromEquipmentSlot = originalParent.GetComponent<EquipmentSlotUI>();

        // 인벤토리에서 좌표 찾기
        for (int y = 0; y < inventory.grid.gridHeight; y++)
        {
            for (int x = 0; x < inventory.grid.gridWidth; x++)
            {
                var slot = inventory.grid.slots[x, y];
                if (slot != null && slot.item.uniqueID == itemInstance.uniqueID)
                {
                    originalX = x;
                    originalY = y;
                    goto Found;
                }
            }
        }
    Found:

        transform.SetParent(transform.root);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        // 장비창에 드롭된 경우
        if (droppedOnEquipment)
        {
            Destroy(gameObject);
            return;
        }

        // 인벤토리 영역 밖 → 바닥에 드롭
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryUI.inventoryArea, Input.mousePosition, eventData.pressEventCamera))
        {
            DropItemToGround();
            return;
        }

        // 장비창에서 인벤토리로 돌아오는 경우
        if (fromEquipmentSlot != null)
        {
            Vector2Int slot = inventory.FindClosestEmptySlot(Input.mousePosition, inventoryUI);

            if (slot.x != -1)
            {
                inventory.grid.PlaceItem(itemInstance, slot.x, slot.y);
                inventory.items.Add(itemInstance);
            }
            else
            {
                fromEquipmentSlot.UnequipToInventory();
            }

            transform.SetParent(originalParent);
            inventoryUI.RefreshItems();
            Destroy(gameObject);
            return;
        }

        // -----------------------------
        // 인벤토리 내부 이동 처리 (핵심 수정)
        // -----------------------------

        // 1) 원래 슬롯 제거
        inventory.grid.RemoveItem(itemInstance);

        // 2) 가장 가까운 빈 슬롯 찾기
        Vector2Int newSlot = inventory.FindClosestEmptySlot(Input.mousePosition, inventoryUI);

        if (newSlot.x != -1)
        {
            // 3) 새 슬롯에 배치
            inventory.grid.PlaceItem(itemInstance, newSlot.x, newSlot.y);
        }
        else
        {
            // 4) 빈 슬롯 없으면 원래 자리로 복구
            inventory.grid.PlaceItem(itemInstance, originalX, originalY);
        }

        // 5) UI 갱신
        transform.SetParent(originalParent);
        inventoryUI.RefreshItems();

        // 6) 드래그 UI 제거
        Destroy(gameObject);
    }


    void DropItemToGround()
    {
        if (itemInstance.data.worldPrefab != null)
        {
            Vector3 dropPos = PlayerMovement.Instance.transform.position + new Vector3(1f, 0.5f, 0f);
            Instantiate(itemInstance.data.worldPrefab, dropPos, Quaternion.identity);
        }

        inventory.RemoveItem(itemInstance);
        inventoryUI.RefreshItems();

        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipRoutine = StartCoroutine(ShowTooltipDelayed());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipRoutine != null)
            StopCoroutine(tooltipRoutine);

        TooltipManager.Instance.HideTooltip();
    }

    IEnumerator ShowTooltipDelayed()
    {
        yield return new WaitForSeconds(1.5f);

        RectTransform rt = GetComponent<RectTransform>();
        Vector3 worldPos = rt.transform.position;

        Vector2 tooltipPos = new Vector2(
            worldPos.x + rt.rect.width * rt.lossyScale.x + 100f,
            worldPos.y
        );

        TooltipManager.Instance.ShowTooltip(itemInstance.data, tooltipPos);
    }
}
