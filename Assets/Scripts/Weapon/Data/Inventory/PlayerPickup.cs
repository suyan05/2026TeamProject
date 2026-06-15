using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask itemLayer;

    public Inventory inventory;
    public InventoryUI inventoryUI;

    bool isFindingUI = true;

    private void Start()
    {
        // Inventory는 보통 바로 찾힘
        if (inventory == null)
            inventory = Object.FindFirstObjectByType<Inventory>(FindObjectsInactive.Include);

        TryFindUI();
    }

    void Update()
    {
        // InventoryUI 못 찾았으면 계속 찾기
        if (isFindingUI)
            TryFindUI();

        if (inventory == null || inventoryUI == null)
            return;

        Debug.DrawRay(transform.position, transform.right * interactDistance, Color.blue);

        if (Input.GetKeyDown(KeyCode.F))
            TryPickup();
    }

    void TryFindUI()
    {
        if (inventoryUI != null)
        {
            isFindingUI = false;
            return;
        }

        // 1) 씬에서 찾기
        inventoryUI = Object.FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);

        // 2) 못 찾으면 DontDestroyOnLoad 영역까지 포함해서 전부 검색
        if (inventoryUI == null)
        {
            var all = Resources.FindObjectsOfTypeAll<InventoryUI>();
            if (all != null && all.Length > 0)
                inventoryUI = all[0];
        }

        // 3) 찾았으면 탐색 종료
        if (inventoryUI != null)
        {
            isFindingUI = false;
            Debug.Log("<color=lime>[PlayerPickup] InventoryUI 자동 참조 성공!</color>");
        }
    }

    void TryPickup()
    {
        Ray ray = new Ray(transform.position, transform.right);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, itemLayer))
        {
            WorldItem worldItem = hit.collider.GetComponent<WorldItem>();

            if (worldItem != null)
            {
                if (inventory.TryAddItem(worldItem.itemData))
                {
                    Destroy(worldItem.gameObject);
                    inventoryUI.RefreshItems();
                }
            }
        }
    }
}
