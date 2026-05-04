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

    [Header("[스폰 데이터 - 그룹화 버전]")]
    public List<EnemySpawnGroup> enemySpawnGroups;

    [System.Serializable]
    public class EnemySpawnGroup
    {
        public string groupName;           // 관리용 그룹 이름 (예: 근거리_A구역)
        public GameObject enemyPrefab;    // 소환할 적 프리팹
        public Color gizmoColor = Color.red; // 씬 뷰에서 보일 원의 색상
        public float gizmoRadius = 0.5f;   // 씬 뷰에서 보일 원의 반지름
        public List<Transform> spawnPoints; // 이 프리팹이 생성될 여러 위치들
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() => SpawnAllEnemies();

    public void SpawnAllEnemies()
    {
        if (enemySpawnGroups == null) return;

        foreach (var group in enemySpawnGroups)
        {
            if (group.enemyPrefab == null || group.spawnPoints == null) continue;

            foreach (Transform point in group.spawnPoints)
            {
                if (point == null) continue;

                // 그룹에 설정된 프리팹을 각 스폰 포인트 위치에 생성
                GameObject enemyObj = Instantiate(group.enemyPrefab, point.position, point.rotation);
                ApplyDetailedPattern(enemyObj, group.groupName);
            }
        }
        Debug.Log($"<color=#00FF00><b>[EnemyManager]</b> 모든 적 스폰 및 {globalPattern.profileName} 프로필 주입 완료</color>");
    }

    private void ApplyDetailedPattern(GameObject enemy, string name)
    {
        Component targetAI = (Component)enemy.GetComponent<MeleeAttackEnemy>() ??
                             (Component)enemy.GetComponent<ArrowAttackEnemy>() ??
                             (Component)enemy.GetComponent<LaserAttackEnemy>();

        if (targetAI == null) return;

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
    }


    private void OnDrawGizmos()
    {
        if (enemySpawnGroups == null) return;

        foreach (var group in enemySpawnGroups)
        {
            if (group.spawnPoints == null) continue;

            Gizmos.color = group.gizmoColor;

            foreach (Transform point in group.spawnPoints)
            {
                if (point == null) continue;

                // 바닥과 겹쳐서 안 보이는 현상을 방지하기 위해 0.1f만큼 위로 띄워 계산
                Vector3 gizmoPos = point.position + Vector3.up * 0.1f;

                // 1. 원형 테두리 표시
                Gizmos.DrawWireSphere(gizmoPos, group.gizmoRadius);

                // 2. 중심점 표시
                Gizmos.DrawSphere(gizmoPos, 0.1f);

                // 3. 바라보는 방향 표시 (앞쪽으로 1m 길이의 선)
                Gizmos.DrawRay(gizmoPos, point.forward * 1f);
            }
        }
    }
}