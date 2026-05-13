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

        // 게임 시작 시 저장된 Gem 데이터를 불러옵니다.
        LoadPermanentCurrencies();
    }

    // 골드 추가 (인게임)
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"<color=yellow>골드 획득!</color> 현재 골드: {currentGold}");
        // 여기에 UI 업데이트 함수를 연결하면 됩니다.
    }

    // 젬 추가 (영구)
    public void AddGem(int amount)
    {
        currentGem += amount;
        SavePermanentCurrencies(); // 젬은 얻자마자 저장합니다.
        Debug.Log($"<color=cyan>젬 획득!</color> 현재 젬: {currentGem}");
    }

    // 스테이지 종료/사망 시 골드 초기화
    public void ResetGold()
    {
        currentGold = 0;
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