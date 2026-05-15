using UnityEngine;

public class EnemyCounter : MonoBehaviour
{
    public static EnemyCounter Instance { get; private set; }

    [System.Serializable]
    public class EnemyInfo
    {
        public int targetCount;
        public bool allowSmaller;
        public GameObject[] gameObjects;
        public bool offAtStart;
    }

    public EnemyInfo[] enemyInfos;
    public static int totalEnemieCounts;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 게임 시작 시 카운트 초기화 (안전장치)
        totalEnemieCounts = 0;
    }

    private void Start()
    {
        foreach (EnemyInfo info in enemyInfos)
        {
            if (info.offAtStart)
            {
                foreach (GameObject obj in info.gameObjects)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }
        }
    }

    public void AddEnemy() => totalEnemieCounts++;

    public void EnemyDefeated()
    {
        totalEnemieCounts--;
        Debug.Log($"적 처치! 남은 적: {totalEnemieCounts}");

        // --- 추가된 보상창 트리거 로직 ---
        if (totalEnemieCounts <= 0)
        {
            Debug.Log("<color=cyan>모든 적 처치 완료! 보상 시스템을 호출합니다.</color>");
            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.ShowRewardSelection();
            }
        }
        // ------------------------------

        foreach (EnemyInfo info in enemyInfos)
        {
            bool isQualified = info.allowSmaller ?
                info.targetCount <= totalEnemieCounts :
                info.targetCount == totalEnemieCounts;

            if (isQualified)
            {
                foreach (GameObject obj in info.gameObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        }
    }
}
