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

    // 3D 물리 제어용 리지드바디 컴포넌트 변수 추가
    private Rigidbody rb;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        StartCoroutine(BossAIRoutine());
        StartCoroutine(NormalAttackRoutine());
    }

    private void Update()
    {
        RotateTowardPlayer();
    }

    // 플레이어 방향으로 회전
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

    // 기본 평타 및 이동 루틴
    private IEnumerator NormalAttackRoutine()
    {
        yield return new WaitForSeconds(3.5f);

        while (!isDead)
        {
            if (isGroggy || isPhaseTransitioning || isExecutingPattern || playerTransform == null)
            {
                
                if (anim != null) anim.SetBool("Walk", false);
                yield return null;
                continue;
            }

            float dist = Vector3.Distance(transform.position, playerTransform.position);

            if (dist <= attackRange)
            {
               
                anim.SetBool("Walk", false);

                anim.Play("Attack", 0, 0f);

                // 플레이어 데미지
                PlayerMovement.Instance.GetDamage(20f, transform);

                yield return new WaitForSeconds(attackCooldown);
            }
            else
            {
                
                anim.SetBool("Walk", true);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    // 보스 AI 루틴
    private IEnumerator BossAIRoutine()
    {
        yield return new WaitForSeconds(3.5f);

        while (!isDead)
        {
            yield return new WaitForSeconds(4f);

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

                int p = Random.Range(0, 3);
                if (p == 0) Pattern_SummonMinions();
                else if (p == 1) yield return StartCoroutine(Pattern_JumpSmash());
                else yield return StartCoroutine(Pattern_PlantSporeMines());

                isExecutingPattern = false;
            }
            else
            {
                isExecutingPattern = true;

                int combo = Random.Range(0, 2);
                if (combo == 0) yield return StartCoroutine(Combo_SuckAndExplode());
                else yield return StartCoroutine(Pattern_JumpSmash());

                isExecutingPattern = false;
            }
        }
    }

    // 작은 버섯 소환
    private void Pattern_SummonMinions()
    {
        int count = Random.Range(2, 5);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 3.5f;
            pos.y = transform.position.y;
            Instantiate(minionMushroomPrefab, pos, Quaternion.identity);
        }
    }

    // 점프 착지 충격파
    private IEnumerator Pattern_JumpSmash()
    {
        if (playerTransform == null) yield break;

        
        anim.SetTrigger("Static");

        Vector3 landPos = playerTransform.position;

        if (jumpSmashIndicator != null)
        {
            jumpSmashIndicator.transform.position = landPos;
            jumpSmashIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(1.2f);

        if (jumpSmashIndicator != null) jumpSmashIndicator.SetActive(false);

        // 순간이동 전후 속도 초기화
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = landPos;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 착지 데미지
        float radius = 4f;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
                PlayerMovement.Instance.GetDamage(30f, transform);
        }

        // 추가 충격파
        int extra = Random.Range(1, 3);
        for (int i = 1; i < extra; i++)
        {
            yield return new WaitForSeconds(0.7f);

            foreach (var h in hits)
            {
                if (h.CompareTag("Player"))
                    PlayerMovement.Instance.GetDamage(20f, transform);
            }
        }
    }

    // 포자 지뢰 설치
    private IEnumerator Pattern_PlantSporeMines()
    {
        if (playerTransform == null) yield break;

        Vector3 pos = playerTransform.position + Random.insideUnitSphere * 2f;
        pos.y = transform.position.y;

        if (sporeMineIndicator != null)
        {
            sporeMineIndicator.transform.position = pos;
            sporeMineIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(1f);
        if (sporeMineIndicator != null) sporeMineIndicator.SetActive(false);

        GameObject mine = Instantiate(landmineSporePrefab, pos, Quaternion.identity);

        MushroomLandmine script = mine.GetComponent<MushroomLandmine>();
        if (script == null) script = mine.AddComponent<MushroomLandmine>();
        script.Setup();
    }

    // 2페이즈 광역 폭발
    private IEnumerator Combo_SuckAndExplode()
    {
        
        anim.SetTrigger("Static");

        yield return new WaitForSeconds(2f);

        Collider[] hits = Physics.OverlapSphere(transform.position, 6.5f);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
                PlayerMovement_3D.Instance.GetDamage(25f, transform);
        }
    }

    // 그로기 상태 루틴 추가
    private IEnumerator GroggyStateRoutine()
    {
        anim.Play("Groggy", 0, 0f); // 그로기 애니메이션 재생
        yield return new WaitForSeconds(5f); // 5초 동안 그로기 상태 유지
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        anim.Play("PhaseTransition", 0, 0f); // Phase 전환 애니메이션 재생
        yield return new WaitForSeconds(3f); // 전환 시간 대기
        currentPhase++; // Phase 증가
        isPhaseTransitioning = false;
    }

    
    protected override void Die()
    {
        isDead = true;

       
        if (anim != null) anim.SetTrigger("Die");

        StopAllCoroutines(); // 모든 코루틴 중지

       
        if (RewardManager.Instance != null)
        {
            RewardManager.Instance.ShowRewardSelection();
        }

        Destroy(gameObject, 5f); // 5초 후 보스 오브젝트 제거
    }

    // 포자 지뢰 서브 클래스
    public class MushroomLandmine : MonoBehaviour
    {
        private bool isTriggered = false;

        public void Setup()
        {
            Destroy(gameObject, 5f); // 자동 폭발
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

            // 데미지
            PlayerMovement.Instance.GetDamage(15f, transform);

            Destroy(gameObject);
        }

        private void LeavePoisonCloud()
        {
            // 지속 피해 장판
        }
    }

    
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
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
            foreach (var h in hits)
            {
                if (h.CompareTag("Player"))
                    PlayerMovement.Instance.GetDamage(20f, transform);
            }

            Destroy(gameObject, 3f);
        }
    }
}