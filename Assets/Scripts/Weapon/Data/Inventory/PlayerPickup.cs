using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float interactDistance = 3f;     // 아이템 줍기 거리
    public LayerMask itemLayer;             // WorldItem 레이어

    public Inventory inventory;             // 플레이어 인벤토리
    public InventoryUI inventoryUI;         // 인벤토리 UI

    void Update()
    {
        // Scene 뷰에서 오른쪽 방향 Ray 시각화
        Debug.DrawRay(transform.position, transform.right * interactDistance, Color.blue);

        if (Input.GetKeyDown(KeyCode.F))
            TryPickup();
    }

    void TryPickup()
    {
        // 플레이어 기준 오른쪽 방향으로 Raycast
        Ray ray = new Ray(transform.position, transform.right);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, itemLayer))
        {
            WorldItem worldItem = hit.collider.GetComponent<WorldItem>();

            if (worldItem != null)
            {
                // 인벤토리에 아이템 추가 시도
                if (inventory.TryAddItem(worldItem.itemData))
                {
                    Destroy(worldItem.gameObject);   // 바닥 아이템 삭제
                    inventoryUI.RefreshItems();      // UI 갱신
                }
            }
        }
    }
}
