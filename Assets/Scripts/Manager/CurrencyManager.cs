using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("[ 현재 소지 재화 ]")]
    [SerializeField] private int currentGold; // 인게임 재화 (스테이지 클리어 시 초기화 예정)
    [SerializeField] private int currentGem;  // 업그레이드 재화 (영구 저장)

    public int Gold => currentGold;
    public int Gem => currentGem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        
        LoadPermanentCurrencies();
    }

  
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"<color=yellow>골드 획득!</color> 현재 골드: {currentGold}");

        // 팁: 만약 상점이 열려있는 상태에서 골드를 얻는다면 바로 UI를 갱신해 줍니다.
        if (ShopManager.Instance != null && ShopManager.Instance.shopPanelObject != null && ShopManager.Instance.shopPanelObject.activeSelf)
        {
            ShopManager.Instance.UpdateMoneyUI();
        }
    }

    // 2. 골드가 충분한지 확인 (상점 구매 조건 체크)
    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    // 3. 골드 소비 (상점 아이템 구매 시)
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            Debug.Log($"<color=red>골드 소비!</color> 남은 골드: {currentGold}");
            return true; // 구매 성공
        }
        else
        {
            Debug.Log($"<color=gray>골드 부족!</color> 현재 골드: {currentGold} / 필요 골드: {amount}");
            return false; // 구매 실패 (돈이 모자람)
        }
    }

    // 4. 스테이지 종료/사망 시 골드 초기화
    public void ResetGold()
    {
        currentGold = 0;
    }

  
    public void AddGem(int amount)
    {
        currentGem += amount;
        SavePermanentCurrencies(); // 젬은 얻자마자 저장합니다.
        Debug.Log($"<color=cyan>젬 획득!</color> 현재 젬: {currentGem}");
    }

    // 데이터 저장 (PlayerPrefs 사용)
    private void SavePermanentCurrencies()
    {
        PlayerPrefs.SetInt("TotalGem", currentGem);
        PlayerPrefs.Save();
    }

    // 데이터 불러오기
    private void LoadPermanentCurrencies()
    {
        currentGem = PlayerPrefs.GetInt("TotalGem", 0);
    }
}