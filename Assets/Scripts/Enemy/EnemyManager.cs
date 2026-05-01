using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [System.Serializable]
    public class EnemyPatternProfile
    {
        public string profileName = "Default Pattern";
        [Header("공격 타이밍 (초 단위)")]
        public float readyToAttackTime = 0.5f;  // 플레이어 감지 후 대기 시간
        public float attackChargeTime = 0.42f;  // 공격 직전 준비 동작 시간
        public float attackCooldown = 1.0f;     // 공격 후 다음 공격까지의 휴식
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab; // 소환할 적 프리팹 (Melee, Arrow, Laser 등)
        public Transform spawnPoint;   // 소환될 위치
    }

    [Header("패턴 설정")]
    public EnemyPatternProfile globalPattern; // 모든 적에게 적용될 공통 패턴

    [Header("스폰 리스트")]
    public List<EnemySpawnData> enemiesToSpawn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SpawnAllEnemies();
    }

    // 설정된 모든 적을 즉시 소환하는 함수
    public void SpawnAllEnemies()
    {
        foreach (var data in enemiesToSpawn)
        {
            if (data.enemyPrefab == null || data.spawnPoint == null) continue;

            GameObject enemyObj = Instantiate(data.enemyPrefab, data.spawnPoint.position, Quaternion.identity);

            // 소환된 적에게 공통 패턴 주입
            ApplyPatternToEnemy(enemyObj);
        }
    }

    private void ApplyPatternToEnemy(GameObject enemy)
    {
        // 1. 근접 적 확인 및 적용
        if (enemy.TryGetComponent<MeleeAttackEnemy>(out var melee))
        {
            melee.readyToAttackTime = globalPattern.readyToAttackTime;
            melee.attackChargeTime = globalPattern.attackChargeTime;
            melee.attackCooldown = globalPattern.attackCooldown;
        }
        // 2. 화살 적 확인 및 적용
        else if (enemy.TryGetComponent<ArrowAttackEnemy>(out var arrow))
        {
            arrow.readyToAttackTime = globalPattern.readyToAttackTime;
            arrow.attackChargeTime = globalPattern.attackChargeTime;
            arrow.attackCooldown = globalPattern.attackCooldown;
        }
        // 3. 레이저 적 확인 및 적용
        else if (enemy.TryGetComponent<LaserAttackEnemy>(out var laser))
        {
            laser.readyToAttackTime = globalPattern.readyToAttackTime;
            laser.attackChargeTime = globalPattern.attackChargeTime;
            laser.attackCooldown = globalPattern.attackCooldown;
        }
    }
}