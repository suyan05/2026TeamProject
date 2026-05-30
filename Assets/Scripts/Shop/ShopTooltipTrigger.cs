using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ShopTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private ShopCardUI cardUI;
    private Coroutine delayCoroutine;

    public void Setup(ShopCardUI ui)
    {
        cardUI = ui;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 1초 ~ 1.5초 사이의 대기 (팀장님 요청 사항: 약 1.2초 세팅)
        delayCoroutine = StartCoroutine(ShowTooltipAfterDelay(1.2f));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (delayCoroutine != null) StopCoroutine(delayCoroutine);
        ShopManager.Instance.HideTooltip();
    }

    private IEnumerator ShowTooltipAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (cardUI != null && cardUI.itemData != null)
        {
            ShopManager.Instance.ShowTooltip(cardUI.itemData, transform.position);
        }
    }
}