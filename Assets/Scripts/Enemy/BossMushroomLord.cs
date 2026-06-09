using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 1. 메인 보스 AI 클래스
public class BossMushroomLord : BossBase
{
    [Header("Player Target")]
    public Transform playerTransform; // 플레이어 위치 추적용

    [Header("Pattern Prefabs")]
    public GameObject minionMushroomPrefab;   // 독성 공격을 하는 작은 버섯 몬스터 프리팹
    public GameObject landmineSporePrefab;    // 바닥에 배치할 포자 지뢰 프리팹
    public GameObject fallingMushroomPrefab;  // 2페이즈 천장 낙하 장애물 프리팹

    [Header("전투 UI 및 경고 연출")]
    public GameObject sporeMineIndicator;     // 포자 지뢰: 바닥에 초록색 점멸 원 UI
    public GameObject jumpSmashIndicator;     // 점프 착지: 착지 지점에 붉은 원과 충격파 방향 표시 UI
    public GameObject fallingIndicatorPrefab; // 낙하 장애물: 낙하 예정 위치에 노란 십자 마커 프리팹

    [Header("2페이즈 화면 효과 UI")]
    public GameObject greenSporeFilterUI;     // 화면 전반에 적용되는 녹색 포자 필터 오버레이
    public GameObject dotDamageIconUI;        // 플레이어 화면에 표시될 지속 피해 아이콘 UI

    [Header("기본 평타 공격 세팅")]
    public float attackRange = 3.0f;          // 플레이어가 이 거리 안에 들어오면 평타 발동
    public float attackCooldown = 1.5f;       // 평타 공격 애니메이션 반복 주기 (쿨타임)
    private Animator anim;                    // 자식에게서 가져올 애니메이터 컴포넌트
    private bool isExecutingPattern = false;  // 특수 패턴(점프, 지뢰 등)을 쓰는 중인지 체크하는 플래그

    private int attackPatternCounter = 0;      // 그로기 상태 진입용 패턴 카운터
    private Coroutine passiveFallingRoutine;   // 2페이즈 천장 낙하 상시 루틴 제어용

    // 💡 3D 물리 제어용 리지드바디 컴포넌트 변수 추가
    private Rigidbody rb;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();

        // 💡 3D 리지드바디 컴포넌트 안전하게 가져오기
        rb = GetComponent<Rigidbody>();

        StartCoroutine(BossAIRoutine());
        StartCoroutine(NormalAttackRoutine());
    }

    private void Update()
    {
        RotateTowardPlayer();
    }

    private void RotateTowardPlayer()
    {
        if (isDead || playerTransform == null || isExecutingPattern) return;

        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private IEnumerator NormalAttackRoutine()
    {
        yield return new WaitForSeconds(3.5f);

        while (!isDead)
        {
            if (isGroggy || isPhaseTransitioning || isExecutingPattern || playerTransform == null)
            {
                yield return null;
                continue;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                if (anim != null)
                {
                    anim.Play("Attack", 0, 0f);
                }

                Debug.Log("보스 평타: 플레이어가 가까이 있어 연속 공격 애니메이션 발동!");
                yield return new WaitForSeconds(attackCooldown);
            }
            else
            {
                if (anim != null && !anim.GetCurrentAnimatorStateInfo(0).IsName("Armature|Armature|Idle"))
                {
                    anim.Play("Armature|Armature|Idle", 0, 0f);
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator BossAIRoutine()
    {
        yield return new WaitForSeconds(3.5f);

        while (!isDead)
        {
            yield return new WaitForSeconds(4.0f);

            if (isPhaseTransitioning || isGroggy) continue;

            attackPatternCounter++;
            if (attackPatternCounter >= 4)
            {
                isExecutingPattern = true;
                yield return StartCoroutine(GroggyStateRoutine());
                isExecutingPattern = false;
                continue;
            }

            if (currentPhase == 1)
            {
                isExecutingPattern = true;

                int randomPattern = Random.Range(0, 3);
                if (randomPattern == 0) Pattern_SummonMinions();
                else if (randomPattern == 1) yield return StartCoroutine(Pattern_JumpSmash());
                else yield return StartCoroutine(Pattern_PlantSporeMines());

                isExecutingPattern = false;
            }
            else
            {
                isExecutingPattern = true;

                int randomCombo = Random.Range(0, 2);
                if (randomCombo == 0)
                {
                    Debug.Log("<color=magenta>[2페이즈 연계 콤보] 흡입 광역 포자 폭발 시작!</color>");
                    yield return StartCoroutine(Combo_SuckAndExplode());
                }
                else
                {
                    Debug.Log("<color=magenta>[2페이즈 연계 콤보] 점프 착지 연속 충격파 확정 연계 시작!</color>");
                    yield return StartCoroutine(Pattern_JumpSmash());
                }

                isExecutingPattern = false;
            }
        }
    }

    private void Pattern_SummonMinions()
    {
        int spawnCount = Random.Range(2, 5);
        Debug.Log($"버섯군주 패턴: 작은 버섯 몬스터 {spawnCount}마리 소환 (독성 공격 압박)");

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3.5f;
            spawnPos.y = transform.position.y;
            Instantiate(minionMushroomPrefab, spawnPos, Quaternion.identity);
        }
    }

    private IEnumerator Pattern_JumpSmash()
    {
        if (playerTransform == null) yield break;

        if (anim != null) anim.Play("Armature|Armature|Idle", 0, 0f);

        Vector3 targetLandPos = playerTransform.position;

        if (jumpSmashIndicator != null)
        {
            jumpSmashIndicator.transform.position = targetLandPos;
            jumpSmashIndicator.SetActive(true);
        }

        Debug.Log("보스 액션: 보스가 크게 도약하여 공중으로 상승!");
        yield return new WaitForSeconds(1.2f);

        if (jumpSmashIndicator != null) jumpSmashIndicator.SetActive(false);

        // 💡 [핵심 버그 수정] 3D 물리 엔진이 급발진하지 않도록 순간이동 전/후 속도와 관성을 완전히 0으로 초기화합니다.
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = targetLandPos;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("?? 쿵! 보스 착지 - 1차 원형 충격파 발산");

        int extraWaves = Random.Range(1, 3);
        for (int i = 1; i < extraWaves; i++)
        {
            yield return new WaitForSeconds(0.7f);
            Debug.Log($"?? 쿵! {i + 1}차 연속 충격파 발산 (회피 타이밍 교란)");
        }
    }

    private IEnumerator Pattern_PlantSporeMines()
    {
        if (playerTransform == null) yield break;

        Vector3 minePos = playerTransform.position + Random.insideUnitSphere * 2f;
        minePos.y = transform.position.y;

        if (sporeMineIndicator != null)
        {
            sporeMineIndicator.transform.position = minePos;
            sporeMineIndicator.SetActive(true);
        }
        yield return new WaitForSeconds(1.0f);
        if (sporeMineIndicator != null) sporeMineIndicator.SetActive(false);

        GameObject mine = Instantiate(landmineSporePrefab, minePos, Quaternion.identity);

        MushroomLandmine mineScript = mine.GetComponent<MushroomLandmine>();
        if (mineScript == null) mineScript = mine.AddComponent<MushroomLandmine>();
        mineScript.Setup();

        Debug.Log("패턴 시전: 전장에 독성 구름 포자 지뢰 배치 완료");
    }

    private IEnumerator Combo_SuckAndExplode()
    {
        Debug.Log("기믹 시전: 보스가 주변의 모든 플레이어를 중심부로 강하게 자석처럼 끌어당김 (흡입)");
        yield return new WaitForSeconds(2.0f);

        Debug.Log("<color=red>?? 콰앙!! 보스 주변 넓은 반경에 광역 포자 폭발 피해 발생!</color>");
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 6.5f);
        foreach (var player in hitPlayers)
        {
            if (player.CompareTag("Player"))
            {
                Debug.Log("플레이어가 광역 포자 폭발에 직격당했습니다.");
            }
        }
    }

    private IEnumerator GroggyStateRoutine()
    {
        isGroggy = true;
        attackPatternCounter = 0;
        if (anim != null) anim.Play("Armature|Armature|Idle", 0, 0f);
        Debug.Log("<color=cyan><b>? [이벤트] 버섯군주가 무리하게 패턴을 시전한 후 잠시 그로기 상태에 빠집니다! (피해 30% 증가)</b></color>");

        yield return new WaitForSeconds(4.0f);

        isGroggy = false;
        Debug.Log("버섯군주가 그로기 상태에서 회복해 다시 일어섭니다.");
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        attackPatternCounter = 0;
        currentPhase = 2;

        Debug.Log("<color=green><b>?? [페이즈 전환] 버섯군주 체력 50% 이하! 2페이즈 광폭화 연출 시작</b></color>");

        if (greenSporeFilterUI != null) greenSporeFilterUI.SetActive(true);
        if (dotDamageIconUI != null) dotDamageIconUI.SetActive(true);
        Debug.Log("UI 출력: 녹색 포자 화면 필터 활성화 및 플레이어 도트 데미지 디버프 아이콘 표기");

        moveSpeed *= 1.3f;

        if (passiveFallingRoutine == null)
        {
            passiveFallingRoutine = StartCoroutine(PassiveFallingObstacleRoutine());
        }

        yield return new WaitForSeconds(3.0f);
        isPhaseTransitioning = false;
    }

    private IEnumerator PassiveFallingObstacleRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(2.5f);

            if (isPhaseTransitioning || playerTransform == null) continue;

            Vector3 dropPosition = playerTransform.position + Random.insideUnitSphere * 4f;
            dropPosition.y = transform.position.y;

            if (fallingIndicatorPrefab != null)
            {
                GameObject indicator = Instantiate(fallingIndicatorPrefab, dropPosition, Quaternion.identity);
                Destroy(indicator, 1.5f);
            }

            Vector3 spawnHeight = new Vector3(dropPosition.x, dropPosition.y + 10f, dropPosition.z);
            GameObject fallingObj = Instantiate(fallingMushroomPrefab, spawnHeight, Quaternion.identity);

            MushroomFallingObstacle dropScript = fallingObj.GetComponent<MushroomFallingObstacle>();
            if (dropScript == null) dropScript = fallingObj.AddComponent<MushroomFallingObstacle>();
            dropScript.StartFalling(dropPosition);
        }
    }

    protected override void Die()
    {
        isDead = true;
        StopAllCoroutines();
        if (passiveFallingRoutine != null) StopCoroutine(passiveFallingRoutine);

        if (greenSporeFilterUI != null) greenSporeFilterUI.SetActive(false);
        if (dotDamageIconUI != null) dotDamageIconUI.SetActive(false);

        Debug.Log("돌연변이 버섯군주가 완전히 썩어 없어졌습니다. 클리어!");
    }
}

// 2. [서브 클래스 1] 포자 폭발 지뢰 스크립트
public class MushroomLandmine : MonoBehaviour
{
    private bool isTriggered = false;

    public void Setup()
    {
        Destroy(gameObject, 5.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            TriggerExplosion();
        }
    }

    private void OnDestroy()
    {
        LeavePoisonCloud();
    }

    private void TriggerExplosion()
    {
        isTriggered = true;
        Debug.Log("지뢰 기믹: 플레이어가 포자를 밟아 즉시 폭발 피해 발생!");
        Destroy(gameObject);
    }

    private void LeavePoisonCloud()
    {
        Debug.Log("지뢰 기믹 연출: 폭발 자리에 잔류 독성 구름 형성 (밟으면 지속 대미지)");
    }
}

// 3. [서브 클래스 2] 2페이즈 전용 천장 낙하 장애물 스크립트
public class MushroomFallingObstacle : MonoBehaviour
{
    private Vector3 targetFloorPos;
    private bool hasHitFloor = false;

    public void StartFalling(Vector3 targetFloor)
    {
        targetFloorPos = targetFloor;
        StartCoroutine(FallDownRoutine());
    }

    private IEnumerator FallDownRoutine()
    {
        float speed = 8f;
        while (transform.position.y > targetFloorPos.y)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetFloorPos, speed * Time.deltaTime);
            yield return null;
        }

        if (!hasHitFloor)
        {
            hasHitFloor = true;
            OnHitFloor();
        }
    }

    private void OnHitFloor()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 2.0f);
        foreach (var player in hitPlayers)
        {
            if (player.CompareTag("Player"))
            {
                Debug.Log("낙하 기믹: 플레이어가 천장에서 떨어진 균사체에 맞아 즉발 피해를 입었습니다.");
            }
        }

        Debug.Log("낙하 기믹: 균사체 덩어리가 깨지며 바닥에 3초간 소규모 독 지속 피해 영역 생성");
        Destroy(gameObject, 3.0f);
    }
}