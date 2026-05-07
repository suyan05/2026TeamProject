using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public ItemTooltipUI tooltipPrefab;
    private ItemTooltipUI currentTooltip;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowTooltip(ItemData data, Vector2 position)
    {
        if (currentTooltip == null)
            currentTooltip = Instantiate(tooltipPrefab, transform);

        currentTooltip.gameObject.SetActive(true);
        currentTooltip.SetData(data);

        // 전달받은 위치에 표시
        currentTooltip.transform.position = position;
    }

    public void HideTooltip()
    {
        if (currentTooltip != null)
            currentTooltip.gameObject.SetActive(false);
    }

}
