using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    #region [기획 데이터 구조 - 변경 없음]
    [System.Serializable] public class MeleeRunnerSettings { public float readyTime = 0.3f; public float chargeDuration = 0.8f; public float chargeSpeed = 8f; public float cooldown = 2.0f; }
    [System.Serializable] public class MeleeStrikerSettings { public float readyTime = 0.25f; public float attackActiveTime = 0.4f; public float cooldown = 1.8f; }
    [System.Serializable] public class RangedShooterSettings { public float readyTime = 0.4f; public float projectileSpeed = 12f; public float cooldown = 2.5f; }
    [System.Serializable] public class LaserChargerSettings { public float chargeTime = 1.0f; public float laserDuration = 0.8f; public float cooldown = 3.0f; }
    [System.Serializable] public class RotatingLaserTowerSettings { public float readyTime = 0.7f; public float laserDuration = 2.0f; public float cooldown = 4.0f; }

    [System.Serializable]
    public class EnemyPatternProfile
    {
        public string profileName = "Plantellia_v1.0";
        public MeleeRunnerSettings meleeRunner;
        public MeleeStrikerSettings meleeStriker;
        public RangedShooterSettings rangedShooter;
        public LaserChargerSettings laser;
        public RotatingLaserTowerSettings rotatingLaser;
    }
    #endregion

    [Header("[중앙 기획 설정]")]
    public EnemyPatternProfile globalPattern;

    [Header("[공통 UI 프리팹]")]
    public GameObject hpBarPrefab;

    [Header("[스폰 데이터]")]
    public List<EnemySpawnData> enemiesToSpawn;

    [System.Serializable]
    public class EnemySpawnData
    {
        public string enemyName;
        public GameObject enemyPrefab;
        public Transform spawnPoint;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => SpawnAllEnemies();

    public void SpawnAllEnemies()
    {
        foreach (var data in enemiesToSpawn)
        {
            if (data.enemyPrefab == null) continue;

            GameObject enemyObj = Instantiate(data.enemyPrefab, data.spawnPoint.position, data.spawnPoint.rotation);
            ApplyDetailedPattern(enemyObj, data.enemyName);
        }
        Debug.Log($"<color=#00FF00><b>[EnemyManager]</b> 모든 적 스폰 및 {globalPattern.profileName} 프로필 주입 완료</color>");
    }

    private void ApplyDetailedPattern(GameObject enemy, string name)
    {
        // 1. 컴포넌트를 한 번만 가져오기 위한 로직
        Component targetAI = (Component)enemy.GetComponent<MeleeAttackEnemy>() ??
                             (Component)enemy.GetComponent<ArrowAttackEnemy>() ??
                             (Component)enemy.GetComponent<LaserAttackEnemy>();

        if (targetAI == null)
        {
            Debug.LogWarning($"<color=yellow>[Apply Skip]</color> {name}에게서 등록된 AI 스크립트를 찾을 수 없습니다.");
            return;
        }

        // 2. 타입에 따른 데이터 주입 (패턴 매칭 사용)
        switch (targetAI)
        {
            case MeleeAttackEnemy runner:
                var r = globalPattern.meleeRunner;
                runner.readyToAttackTime = r.readyTime;
                runner.dashTime = r.chargeDuration;
                runner.dashSpeed = r.chargeSpeed;
                runner.attackCooldown = r.cooldown;
                runner.hpBarPrefab = hpBarPrefab;
                break;

            case ArrowAttackEnemy shooter:
                var s = globalPattern.rangedShooter;
                shooter.readyToAttackTime = s.readyTime;
                shooter.arrowSpeed = s.projectileSpeed;
                shooter.attackCooldown = s.cooldown;
                shooter.hpBarPrefab = hpBarPrefab;
                break;

            case LaserAttackEnemy laser:
                var l = globalPattern.laser;
                laser.aimingTime = l.chargeTime;
                laser.timeToFire = l.laserDuration;
                laser.attackCooldown = l.cooldown;
                laser.hpBarPrefab = hpBarPrefab;
                break;
        }

        Debug.Log($"<color=cyan>[Apply Success]</color> {name} 수치 주입 완료.");
    }
}