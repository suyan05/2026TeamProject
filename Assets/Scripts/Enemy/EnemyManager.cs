using System.Collections.Generic;
using UnityEngine;


public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("[공통 UI 프리팹]")]
    public GameObject hpBarPrefab;

    [Header("[스폰 그룹 설정 - ScriptableObject 연동]")]
    public List<EnemySpawnGroup> enemySpawnGroups;

    [System.Serializable]
    public class EnemySpawnGroup
    {
        public string groupName;             // 관리용 그룹 이름
        public EnemyData enemyData;          // [중요] 적의 정보를 담은 데이터 파일 (ScriptableObject)
        public Color gizmoColor = Color.red; // 씬 뷰에서 보일 기즈모 색상
        public float gizmoRadius = 1.0f;     // 기즈모 원의 반지름
        public List<Transform> spawnPoints;  // 적이 생성될 위치 리스트
    }

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 모든 적 스폰
        SpawnAllEnemies();
    }

    
    public void SpawnAllEnemies()
    {
        if (enemySpawnGroups == null) return;

        foreach (var group in enemySpawnGroups)
        {
            // 데이터가 없거나 프리팹이 설정되지 않았다면 건너뜀
            if (group.enemyData == null || group.enemyData.enemyPrefab == null) continue;

            foreach (Transform point in group.spawnPoints)
            {
                if (point == null) continue;

                //  데이터 시트에 등록된 프리팹을 해당 위치에 소환
                GameObject enemyObj = Instantiate(group.enemyData.enemyPrefab, point.position, point.rotation);

                //  소환된 적에게 데이터 시트의 수치를 주입
                ApplyEnemyData(enemyObj, group.enemyData);
            }
        }
        Debug.Log("<color=#00FF00><b>[EnemyManager]</b> 모든 적 스폰 및 데이터 주입 완료</color>");
    }

    
    private void ApplyEnemyData(GameObject enemy, EnemyData data)
    {
        // 적에 붙어있는 AI 스크립트 탐색
        var meleeAI = enemy.GetComponent<MeleeAttackEnemy>();
        var rangedAI = enemy.GetComponent<ArrowAttackEnemy>();
        var laserAI = enemy.GetComponent<LaserAttackEnemy>();

        //  근거리 돌진형(MeleeRunner 등) 주입
        if (meleeAI != null)
        {
            meleeAI.readyToAttackTime = data.readyTime;
            meleeAI.dashTime = data.chargeDuration;
            meleeAI.dashSpeed = data.chargeSpeed;
            meleeAI.attackCooldown = data.attackCooldown;
            meleeAI.hpBarPrefab = hpBarPrefab;
        }
        //  원거리형(RangedShooter 등) 주입
        else if (rangedAI != null)
        {
            rangedAI.readyToAttackTime = data.readyTime;
            rangedAI.arrowSpeed = data.projectileSpeed;
            rangedAI.attackCooldown = data.attackCooldown;
            rangedAI.hpBarPrefab = hpBarPrefab;
        }
        //  레이저형 주입
        else if (laserAI != null)
        {
            laserAI.aimingTime = data.readyTime;
            laserAI.attackCooldown = data.attackCooldown;
            laserAI.hpBarPrefab = hpBarPrefab;
        }
    }

  
    private void OnDrawGizmos()
    {
        if (enemySpawnGroups == null) return;

        foreach (var group in enemySpawnGroups)
        {
            if (group.spawnPoints == null) continue;

            // 해당 그룹의 설정 색상 적용
            Gizmos.color = group.gizmoColor;

            foreach (Transform point in group.spawnPoints)
            {
                if (point == null) continue;

                // 바닥 겹침 방지를 위해 살짝 띄움
                Vector3 gizmoPos = point.position + Vector3.up * 0.1f;

                //  원형 레이 (WireSphere)
                Gizmos.DrawWireSphere(gizmoPos, group.gizmoRadius);

                //  발바닥 위치 점 (Sphere)
                Gizmos.DrawSphere(gizmoPos, 0.1f);

                //  적의 정면 방향 선 (Ray)
                Gizmos.DrawRay(gizmoPos, point.forward * 1.5f);
            }
        }
    }
}