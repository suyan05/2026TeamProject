using UnityEngine;

public enum EnemyType
{
    None,
    MeleeRunner,   // 근거리 돌진형
    MeleeStriker,  // 근거리 타격형
    RangedShooter, // 원거리 발사형
    LaserCharger,  // 레이저 충전형
    Boss           // 보스
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("적 고유 ID")]
    [SerializeField]
    private int enemyID;
    public int EnemyID => enemyID;

    [Header("적 표시 이름")]
    public string displayName;

    [Header("적 타입")]
    public EnemyType enemyType = EnemyType.None;

    [Header("기본 능력치")]
    public float maxHP = 100f;
    public float moveSpeed = 3f;
    public float defense = 5f;            // [추가] 적 방어력

    [Header("내성 시스템 (데미지 배율)")]
    [Tooltip("1.0이면 100% 데미지, 0.5면 50% 데미지만 받음 (내성)")]
    public float fireResistance = 1.0f;   // 불 속성 내성
    public float iceResistance = 1.0f;    // 얼음 속성 내성
    public float lightResistance = 1.0f;  // 빛 속성 내성

    [Header("드랍 재화")]
    public int dropGold = 50;             // [추가] 일반 인게임 재화 (사망 시 초기화)
    public int dropGem = 1;               // [추가] 업그레이드 재화 (영구 유지)

    [Header("상세 패턴 수치 (타입에 맞는 값만 입력)")]
    public float readyTime = 0.3f;        // 선딜레이/준비시간
    public float chargeSpeed = 8f;        // 돌진 속도
    public float chargeDuration = 0.8f;   // 돌진 지속 시간
    public float attackCooldown = 2.0f;   // 공격 쿨타임
    public float projectileSpeed = 12f;   // 투사체 속도

    [Header("프리팹 설정")]
    public GameObject enemyPrefab;

    [Header("UI 및 시각화")]
    public Sprite icon;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // ID가 비어있으면 자동 생성
        if (enemyID == 0)
            enemyID = GetInstanceID();

        // 필수 필드 누락 시 경고
        if (string.IsNullOrWhiteSpace(displayName))
            Debug.LogWarning($"{name} : 적 이름이 비어 있습니다.", this);
        if (enemyPrefab == null)
            Debug.LogWarning($"{name} : 프리팹이 지정되지 않았습니다.", this);
    }
#endif

    private void OnEnable()
    {
        if (enemyID == 0)
            enemyID = GetInstanceID();
    }
}