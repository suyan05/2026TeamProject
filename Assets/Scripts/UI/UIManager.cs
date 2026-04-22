using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("인벤토리 UI")]
    public GameObject inventoryUI;

    public static UIManager Instance;

    [Header("화살 충전 게이지")]
    public RectTransform chargeGaugeRoot; // 게이지 전체 오브젝트
    public Image chargeFill;

    [Header("플레이어 왼쪽 오프셋")]
    public Vector2 offset = new Vector2(-80f, 40f);

    private void Awake()
    {
        Instance = this;
    }

    private void LateUpdate()
    {
        FollowPlayer();
    }

    void FollowPlayer()
    {
        if (chargeGaugeRoot == null || PlayerMovement.Instance == null)
            return;

        Vector3 worldPos = PlayerMovement.Instance.transform.position;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        chargeGaugeRoot.position = screenPos + (Vector3)offset;
    }

    public void UpdateChargeGauge(float current, float max)
    {
        if (chargeFill == null) return;

        chargeFill.fillAmount = current / max;

        chargeGaugeRoot.gameObject.SetActive(current > 0);
    }

    public void ToggleInventory()
    {
        if (inventoryUI == null) return;

        bool isActive = inventoryUI.activeSelf;
        inventoryUI.SetActive(!isActive);
    }

}
