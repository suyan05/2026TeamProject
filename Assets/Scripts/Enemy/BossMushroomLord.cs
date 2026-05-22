using UnityEngine;
using System.Collections;

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

    private int attackPatternCounter = 0;      // 그로기 상태 진입용 패턴 카운터
    private Coroutine passiveFallingRoutine;   // 2페이즈 천장 낙하 상시 루틴 제어용

    private void Start()
    {
        StartCoroutine(BossAIRoutine());
    }

    private IEnumerator BossAIRoutine()
    {
        // 보스가 거대하게 변이하는 등장 연출 시간 대기
        yield return new WaitForSeconds(3.5f);

        while (!isDead)
        {
            // 패턴 간 기본 대기 쿨타임
            yield return new WaitForSeconds(4.0f);

            if (isPhaseTransitioning || isGroggy) continue;

            // 특정 패턴 이후 잠시 그로기 상태에 빠진다
            attackPatternCounter++;
            if (attackPatternCounter >= 4)
            {
                yield return StartCoroutine(GroggyStateRoutine());
                continue;
            }

            if (currentPhase == 1)
            {
                // 1페이즈 기본 공격 패턴 3종 무작위 실행
                int randomPattern = Random.Range(0, 3);
                if (randomPattern == 0) Pattern_SummonMinions();
                else if (randomPattern == 1) yield return StartCoroutine(Pattern_JumpSmash());
                else yield return StartCoroutine(Pattern_PlantSporeMines());
            }
            else
            {
                //  2페이즈 전용 패턴 연계 콤보 시스템
                int randomCombo = Random.Range(0, 2);
                if (randomCombo == 0)
                {
                    // 콤보 1: 흡입 ? 광역 포자 폭발
                    Debug.Log("<color=magenta>[2페이즈 연계 콤보] 흡입 ? 광역 포자 폭발 시작!</color>");
                    yield return StartCoroutine(Combo_SuckAndExplode());
                }
                else
                {
                    // 콤보 2: 점프 착지 ? 연속 충격파
                    Debug.Log("<color=magenta>[2페이즈 연계 콤보] 점프 착지 ? 연속 충격파 확정 연계 시작!</color>");
                    yield return StartCoroutine(Pattern_JumpSmash());
                }
            }
        }
    }

    // [기본 패턴 1] 잡옵 소환 기믹
    private void Pattern_SummonMinions()
    {
        //  작은 버섯 몬스터 2~4마리를 소환한다.
        int spawnCount = Random.Range(2, 5);
        Debug.Log($"버섯군주 패턴: 작은 버섯 몬스터 {spawnCount}마리 소환 (독성 공격 압박)");

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3.5f;
            spawnPos.y = transform.position.y; // 보스와 같은 바닥 높이
            Instantiate(minionMushroomPrefab, spawnPos, Quaternion.identity);
        }
    }

    // [기본 패턴 2] 점프 후 내리찍기 기믹
    private IEnumerator Pattern_JumpSmash()
    {
        if (playerTransform == null) yield break;

        Vector3 targetLandPos = playerTransform.position;

        // UI 경고 연출 착지 지점에 붉은 원과 충격파 방향 표시가 나타난다.
        if (jumpSmashIndicator != null)
        {
            jumpSmashIndicator.transform.position = targetLandPos;
            jumpSmashIndicator.SetActive(true);
        }

        Debug.Log("보스 액션: 보스가 크게 도약하여 공중으로 상승!");
        yield return new WaitForSeconds(1.2f); // 도약 후 체공 시간

        if (jumpSmashIndicator != null) jumpSmashIndicator.SetActive(false);

        //  착지하며 원형 충격파를 발생시킨다.
        transform.position = targetLandPos;
        Debug.Log("?? 쿵! 보스 착지 - 1차 원형 충격파 발산");

        // 충격파는 1~2회 연속으로 퍼질 수 있어 회피 타이밍을 흔든다.
        int extraWaves = Random.Range(1, 3); // 1~2회 연속 보너스 타격
        for (int i = 1; i < extraWaves; i++)
        {
            yield return new WaitForSeconds(0.7f); // 타이밍 흔들기용 박자 딜레이
            Debug.Log($"?? 쿵! {i + 1}차 연속 충격파 발산 (회피 타이밍 교란)");
        }
    }

    // [기본 패턴 3] 포자 폭발 지뢰 기믹
    private IEnumerator Pattern_PlantSporeMines()
    {
        if (playerTransform == null) yield break;

        Vector3 minePos = playerTransform.position + Random.insideUnitSphere * 2f;
        minePos.y = transform.position.y;

        // UI 경고 연출 바닥에 초록색 점멸 원이 표시된다.
        if (sporeMineIndicator != null)
        {
            sporeMineIndicator.transform.position = minePos;
            sporeMineIndicator.SetActive(true);
        }
        yield return new WaitForSeconds(1.0f);
        if (sporeMineIndicator != null) sporeMineIndicator.SetActive(false);

        //  바닥에 포자 덩어리를 생성해 지뢰처럼 배치한다.
        GameObject mine = Instantiate(landmineSporePrefab, minePos, Quaternion.identity);

        // 동적 컴포넌트 추가로 파일 누락 에러 원천 차단
        MushroomLandmine mineScript = mine.GetComponent<MushroomLandmine>();
        if (mineScript == null) mineScript = mine.AddComponent<MushroomLandmine>();
        mineScript.Setup();

        Debug.Log("패턴 시전: 전장에 독성 구름 포자 지뢰 배치 완료");
    }

    // [2페이즈 콤보] 흡입 ? 광역 포자 폭발
    private IEnumerator Combo_SuckAndExplode()
    {
        Debug.Log("기믹 시전: 보스가 주변의 모든 플레이어를 중심부로 강하게 자석처럼 끌어당김 (흡입)");
        // 플레이어를 끌어당기는 물리 로직이 들어가는 자리입니다.
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

    // [추가 패턴] 그로기 상태 루틴
    private IEnumerator GroggyStateRoutine()
    {
        isGroggy = true;
        attackPatternCounter = 0;
        Debug.Log("<color=cyan><b>? [이벤트] 버섯군주가 무리하게 패턴을 시전한 후 잠시 그로기 상태에 빠집니다! (피해 30% 증가)</b></color>");

        yield return new WaitForSeconds(4.0f); // 무방비 프리딜 타임 제공

        isGroggy = false;
        Debug.Log("버섯군주가 그로기 상태에서 회복해 다시 일어섭니다.");
    }

    // [페이즈 전환] 2페이즈 진입 연출 루틴
    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        attackPatternCounter = 0;
        currentPhase = 2;

        Debug.Log("<color=green><b>?? [페이즈 전환] 버섯군주 체력 50% 이하! 2페이즈 광폭화 연출 시작</b></color>");

        //  주변 공기가 뿌옇게 변하며 전장 전체에 포자 피해 구역이 형성된다.
        //  UI 연동 - 화면 전반에 녹색 포자 필터가 적용되고 지속 피해 아이콘이 표시된다.
        if (greenSporeFilterUI != null) greenSporeFilterUI.SetActive(true);
        if (dotDamageIconUI != null) dotDamageIconUI.SetActive(true);
        Debug.Log("UI 출력: 녹색 포자 화면 필터 활성화 및 플레이어 도트 데미지 디버프 아이콘 표기");

        //  이후 보스의 공격 속도가 빨라지고 (쿨타임 및 연계 속도 내부 보정)
        moveSpeed *= 1.3f;

        // 낙하 장애물 패턴이 해금된다. (상시 낙하 루틴 가동)
        if (passiveFallingRoutine == null)
        {
            passiveFallingRoutine = StartCoroutine(PassiveFallingObstacleRoutine());
        }

        yield return new WaitForSeconds(3.0f);
        isPhaseTransitioning = false;
    }

    // 2페이즈 해금 기믹: 상시 천장 낙하 장애물 제어 스케줄러
    private IEnumerator PassiveFallingObstacleRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(2.5f); // 2.5초마다 천장에서 무작위 드롭

            if (isPhaseTransitioning || playerTransform == null) continue;

            // 플레이어 주변 무작위 낙하 위치 선정
            Vector3 dropPosition = playerTransform.position + Random.insideUnitSphere * 4f;
            dropPosition.y = transform.position.y;

            // 기획서 내용: 낙하 예정 위치에 노란 십자 마커가 생성된다.
            if (fallingIndicatorPrefab != null)
            {
                GameObject indicator = Instantiate(fallingIndicatorPrefab, dropPosition, Quaternion.identity);
                Destroy(indicator, 1.5f); // 장애물 착지 전까지 표시 후 파괴
            }

            // 천장 높이(Y축 높게) 설정해서 낙하 오브젝트 스폰
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

        // 사망 시 화면 녹색 독구름 필터 및 지속피해 디버프 UI 자동 해제
        if (greenSporeFilterUI != null) greenSporeFilterUI.SetActive(false);
        if (dotDamageIconUI != null) dotDamageIconUI.SetActive(false);

        Debug.Log("돌연변이 버섯군주가 완전히 썩어 없어졌습니다. 클리어!");
    }
}


// 2. [서브 클래스 1] 포자 폭발 지뢰 스크립트
// ====================================================================
public class MushroomLandmine : MonoBehaviour
{
    private bool isTriggered = false;

    public void Setup()
    {
        //  포자는 일정 시간이 지나도 폭발하며, 플레이어가 가까이 가면 즉시 터질 수도 있다.
        Destroy(gameObject, 5.0f); // 5초 뒤에 밟지 않아도 타임아웃 자동 폭발
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
        // 타임아웃이든, 플레이어가 밟았든 파괴될 때 무조건 독성 구름을 남김
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
        //  폭발 후에는 독성 구름이 남아 추가 위협을 만든다. (지속 장판 영역)
        Debug.Log("지뢰 기믹 연출: 폭발 자리에 잔류 독성 구름 형성 (밟으면 지속 대미지)");
    }
}


// 3. [서브 클래스 2] 2페이즈 전용 천장 낙하 장애물 스크립트
// ====================================================================
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
        // 바닥에 도달할 때까지 아래로 하강
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
        //  천장에서 버섯 덩어리와 균사체 조각이 떨어진다. 낙하 지점에는 즉발 피해 또는 지속 피해가 발생한다.
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 2.0f);
        foreach (var player in hitPlayers)
        {
            if (player.CompareTag("Player"))
            {
                Debug.Log("낙하 기믹: 플레이어가 천장에서 떨어진 균사체에 맞아 즉발 피해를 입었습니다.");
            }
        }

        Debug.Log("낙하 기믹: 균사체 덩어리가 깨지며 바닥에 3초간 소규모 독 지속 피해 영역 생성");
        Destroy(gameObject, 3.0f); // 지속 장판 연출 후 소멸
    }
}