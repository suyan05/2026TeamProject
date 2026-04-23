using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("인벤토리 UI")]
    public GameObject inventoryUI;

    [Header("화살 충전 게이지")]
    public RectTransform chargeGaugeRoot; // 게이지 전체 오브젝트
    public Image chargeFill;

    [Header("플레이어 왼쪽 오프셋")]
    public Vector2 offset = new Vector2(-80f, 40f);

    [Header("페이드 커튼")]
    public Image fadeCurtain;

    Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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


    /// <summary>
    /// 페이드 커튼의 알파값 변경. duration이 0보다 크면 점진적으로 변경됨.
    /// </summary>
    public void SetCurtainToggle(bool isActive, float duration = 0f)
    {
        if (fadeCurtain == null) return;

        float targetAlpha = isActive ? 1f : 0f;
        fadeCurtain.gameObject.SetActive(isActive);

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(SetCurtainAlphaCoroutine(targetAlpha, duration));
    }

    IEnumerator SetCurtainAlphaCoroutine(float targetAlpha, float duration)
    {
        if (duration <= 0f)
        {
            float startAlpha = fadeCurtain.color.a;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                SetImageAlpha(fadeCurtain, currentAlpha);

                yield return null;
            }
        }

        SetImageAlpha(fadeCurtain, targetAlpha);
        fadeCoroutine = null;
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color color = img.color;
        color.a = alpha;
        img.color = color;
    }

}
