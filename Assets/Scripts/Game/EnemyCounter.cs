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
    }

    private void Start()
    {
        foreach (EnemyInfo info in enemyInfos)
        {
            if (info.offAtStart)
            {
                foreach (GameObject obj in info.gameObjects)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    public void AddEnemy() => totalEnemieCounts++;

    public void EnemyDefeated()
    {
        totalEnemieCounts--;

        foreach (EnemyInfo info in enemyInfos)
        {
            bool isQualified = info.allowSmaller ?
                info.targetCount <= totalEnemieCounts :
                info.targetCount == totalEnemieCounts;

            if (isQualified)
            {
                foreach (GameObject obj in info.gameObjects)
                {
                    obj.SetActive(true);
                }
            }
        }
    }
}
