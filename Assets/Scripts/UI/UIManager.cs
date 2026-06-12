using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("World Canvas (적 HP바 등 표시용)")]
    public GameObject worldCanvas;

    [Header("인벤토리 UI")]
    public GameObject inventoryUI;

    [Header("화살 충전 게이지")]
    public RectTransform chargeGaugeRoot; // 게이지 전체 오브젝트
    public Image chargeFill;

    [Header("플레이어 왼쪽 오프셋")]
    public Vector2 offset = new Vector2(-80f, 40f);

    [Header("페이드 커튼")]
    public Image fadeCurtain;

    [Header("Player HP UI")]
    public Slider playerHpBar;

    [Header("Enemy HP UI")]
    public Transform enemyHpPanel;      // Vertical Layout Group
    public GameObject enemyHpItemPrefab;
    public float enemyDetectRadius = 10f;   // 플레이어 주변 감지 범위
    public LayerMask enemyLayer;

    [Header("현재 장착 무기")]
    public Text equippedWeaponText;

    [Header("플레이어 스텟 택스처")]
    public Text statText;

    private Dictionary<GameObject, EnemyHPItem> enemyUIMap = new Dictionary<GameObject, EnemyHPItem>();

    Coroutine fadeCoroutine;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        AutoReference();
    }

    void AutoReference()
    {
        // 월드 캔버스 자동 참조
        if (worldCanvas == null)
            worldCanvas = GameObject.Find("WorldCanvas");

        // 인벤토리 UI 자동 참조
        if (inventoryUI == null)
            inventoryUI = GameObject.Find("InventoryUI");

        // 차지 게이지 자동 참조
        if (chargeGaugeRoot == null)
            chargeGaugeRoot = transform.Find("ChargeGaugeRoot")?.GetComponent<RectTransform>();

        if (chargeFill == null && chargeGaugeRoot != null)
            chargeFill = chargeGaugeRoot.GetComponentInChildren<Image>();

        // 페이드 커튼 자동 참조
        if (fadeCurtain == null)
            fadeCurtain = GameObject.Find("FadeCurtain")?.GetComponent<Image>();

        // 플레이어 HP 바 자동 참조
        if (playerHpBar == null)
            playerHpBar = GameObject.Find("PlayerHPBar")?.GetComponent<Slider>();

        // Enemy HP Panel 자동 참조
        if (enemyHpPanel == null)
            enemyHpPanel = GameObject.Find("EnemyHPPanel")?.transform;

        // Enemy HP Item Prefab 자동 로드 (Resources 폴더 기준)
        if (enemyHpItemPrefab == null)
            enemyHpItemPrefab = Resources.Load<GameObject>("UI/EnemyHPItem");

        // 무기 텍스트 자동 참조
        if (equippedWeaponText == null)
            equippedWeaponText = GameObject.Find("EquippedWeaponText")?.GetComponent<Text>();

        // 스탯 텍스트 자동 참조
        if (statText == null)
            statText = GameObject.Find("PlayerStatText")?.GetComponent<Text>();
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

        bool isActive = !inventoryUI.activeSelf;
        inventoryUI.SetActive(isActive);

        GameStateManager.Current = isActive ? GameState.InventoryOpen : GameState.Normal;
    }



    /// <summary>
    /// 페이드 커튼의 알파값 변경. duration이 0보다 크면 점진적으로 변경됨.
    /// </summary>
    public void SetCurtainToggle(bool isActive, float duration, float waitingTime = 0f)
    {
        if (fadeCurtain == null) return;

        fadeCurtain.gameObject.SetActive(true);
        float targetAlpha = isActive ? 1f : 0f;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(SetCurtainAlphaCoroutine(targetAlpha, duration, isActive, waitingTime));
    }

    IEnumerator SetCurtainAlphaCoroutine(float targetAlpha, float duration, bool isActive, float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);

        if (duration > 0f)
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
        if (!isActive) fadeCurtain.gameObject.SetActive(false);

        fadeCoroutine = null;
    }

    void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color color = img.color;
        color.a = alpha;
        img.color = color;
    }

    public void UpdatePlayerHP()
    {
        if (PlayerMovement.Instance == null) return;

        if (playerHpBar == null)
        {
            return;
        }

        float cur = PlayerMovement.Instance.currentHp;
        float max = PlayerMovement.Instance.MaxHp;

        if (max > 0)
        {
            playerHpBar.value = cur / max;
        }
    }

    public void UpdateNearbyEnemiesHP()
    {
        // 1) 기존 UI 정리
        foreach (var kv in enemyUIMap)
        {
            if (kv.Key == null) Destroy(kv.Value.gameObject);
        }

        // null 제거
        List<GameObject> removeList = new List<GameObject>();
        foreach (var kv in enemyUIMap)
        {
            if (kv.Key == null) removeList.Add(kv.Key);
        }
        foreach (var r in removeList) enemyUIMap.Remove(r);

        // 2) 주변 적 탐색
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            PlayerMovement.Instance.transform.position,
            enemyDetectRadius,
            enemyLayer
        );

        foreach (Collider2D col in enemies)
        {
            GameObject enemy = col.gameObject;

            // 이미 UI가 있으면 업데이트만
            if (enemyUIMap.ContainsKey(enemy))
            {
                UpdateEnemyHPUI(enemy, enemyUIMap[enemy]);
            }
            else
            {
                // 새 UI 생성
                GameObject uiObj = Instantiate(enemyHpItemPrefab, enemyHpPanel);
                EnemyHPItem item = uiObj.GetComponent<EnemyHPItem>();
                enemyUIMap.Add(enemy, item);

                UpdateEnemyHPUI(enemy, item);
            }
        }
    }

    void UpdateEnemyHPUI(GameObject enemy, EnemyHPItem ui)
    {
        // 적 스크립트 가져오기
        var melee = enemy.GetComponent<MeleeAttackEnemy>();
        var arrow = enemy.GetComponent<ArrowAttackEnemy>();
        var laser = enemy.GetComponent<LaserAttackEnemy>();

        float cur = 0;
        float max = 0;

        if (melee != null) { cur = melee.currentHp; max = melee.maxHp; }
        else if (arrow != null) { cur = arrow.currentHp; max = arrow.maxHp; }
        else if (laser != null) { cur = laser.currentHp; max = laser.maxHp; }

        ui.SetHP(enemy.name, cur, max);

        if (cur <= 0)
        {
            Destroy(ui.gameObject);
            enemyUIMap.Remove(enemy);
        }
    }

    public void UpdateEquippedWeaponUI(string weaponName)
    {
        if (equippedWeaponText != null)
            equippedWeaponText.text = $"현재 무기: {weaponName}";
    }

    public void UpdatePlayerStatsUI(float maxHp, float baseDamage, float baseAttackSpeed,
                                float weaponDamage, float weaponSpeed)
    {
        string dmgText = weaponDamage != 0 ? $"{baseDamage} <color=yellow>(+{weaponDamage})</color>" : $"{baseDamage}";
        string spdText = weaponSpeed != 0 ? $"{baseAttackSpeed} <color=yellow>(+{weaponSpeed})</color>" : $"{baseAttackSpeed}";

        statText.text =
            $"<b>플레이어 스탯</b>\n" +
            $"최대 체력: {maxHp}\n" +
            $"공격력: {dmgText}\n" +
            $"공격 속도: {spdText}";
    }
}
