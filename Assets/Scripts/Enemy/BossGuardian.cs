using UnityEngine;
using System.Collections;

// 1. 메인 보스 AI 클래스
public class BossGuardian : BossBase
{
    [Header("Player Target")]
    public Transform playerTransform;

    [Header("Pattern Prefabs")]
    public GameObject vineTrapPrefab;
    public GameObject smashWavePrefab;
    public GameObject seedProjectilePrefab;
    public Transform shootPoint;

    [Header("전투 UI 경고 연출")]
    public GameObject redCircleIndicator;
    public GameObject yellowDotIndicator;
    public GameObject warningLineIndicator;

    [Header("2페이즈 약점 시스템")]
    public int remainingWeaknessVines = 3;
    private float defenseModifier = 0f;

    private int attackPatternCounter = 0;

    private void Start()
    {
        StartCoroutine(BossAIRoutine());
    }

    public override void TakeDamage(float damage)
    {
        if (currentPhase == 2 && remainingWeaknessVines > 0)
        {
            damage *= (1f - defenseModifier);
        }
        base.TakeDamage(damage);
    }

    private IEnumerator BossAIRoutine()
    {
        yield return new WaitForSeconds(3f);

        while (!isDead)
        {
            yield return new WaitForSeconds(3.5f);

            if (isPhaseTransitioning || isGroggy) continue;

            attackPatternCounter++;
            if (attackPatternCounter >= 4)
            {
                StartCoroutine(GroggyStateRoutine());
                continue;
            }

            if (currentPhase == 1)
            {
                int randomPattern = Random.Range(0, 3);
                if (randomPattern == 0) yield return StartCoroutine(Pattern_VineBind());
                else if (randomPattern == 1) yield return StartCoroutine(Pattern_Smash());
                else yield return StartCoroutine(Pattern_SpawnSeeds());
            }
            else
            {
                int randomCombo = Random.Range(0, 2);
                if (randomCombo == 0)
                {
                    Debug.Log("<color=red>[2페이즈 연계 콤보] 덩굴 속박 흡수 ? 내리찍기 연계 시작!</color>");
                    yield return StartCoroutine(Pattern_VineBind());
                    yield return new WaitForSeconds(0.5f);
                    yield return StartCoroutine(Pattern_Smash());
                }
                else
                {
                    Debug.Log("<color=red>[2페이즈 연계 콤보] 씨앗 투사체 발사 ? 덩굴 채찍 휘두르기 시작!</color>");
                    yield return StartCoroutine(Pattern_SpawnSeeds());
                    yield return new WaitForSeconds(0.8f);
                    Debug.Log("보스 액션: 광역 덩굴 채찍 휘두르기로 연계 타격!");
                }
            }
        }
    }

    private IEnumerator Pattern_VineBind()
    {
        if (playerTransform == null) yield break;

        if (redCircleIndicator != null)
        {
            redCircleIndicator.transform.position = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            redCircleIndicator.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);
        if (redCircleIndicator != null) redCircleIndicator.SetActive(false);

        GameObject vine = Instantiate(vineTrapPrefab, playerTransform.position, Quaternion.identity);

        // 에러 방지: 컴포넌트가 없으면 동적으로 붙여서 작동하도록 수정
        VineTrapObject vineScript = vine.GetComponent<VineTrapObject>();
        if (vineScript == null) vineScript = vine.AddComponent<VineTrapObject>();
        vineScript.Setup(this);

        Debug.Log("패턴 시전: 플레이어 발밑에 속박 덩굴 오브젝트 소환 완료");
    }

    private IEnumerator Pattern_Smash()
    {
        if (warningLineIndicator != null) warningLineIndicator.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        if (warningLineIndicator != null) warningLineIndicator.SetActive(false);

        Debug.Log("보스가 팔을 크게 들어 올린 뒤 전방을 강하게 내리찍음");

        Instantiate(smashWavePrefab, transform.position + transform.forward * 1.5f, transform.rotation);
        Debug.Log("패턴 시전: 전방 강하게 내리찍기 및 직선 충격파 발사 완료");
    }

    private IEnumerator Pattern_SpawnSeeds()
    {
        int seedCount = Random.Range(2, 4);

        for (int i = 0; i < seedCount; i++)
        {
            if (playerTransform == null) break;

            Vector3 targetPosition = playerTransform.position + Random.insideUnitSphere * 2.5f;
            targetPosition.y = transform.position.y;

            if (yellowDotIndicator != null)
            {
                GameObject indicator = Instantiate(yellowDotIndicator, targetPosition, Quaternion.identity);
                Destroy(indicator, 1.2f);
            }

            GameObject seed = Instantiate(seedProjectilePrefab, shootPoint.position, Quaternion.identity);

            // 에러 방지: 컴포넌트가 없으면 동적으로 붙여서 작동하도록 수정
            SeedProjectileObject seedScript = seed.GetComponent<SeedProjectileObject>();
            if (seedScript == null) seedScript = seed.AddComponent<SeedProjectileObject>();
            seedScript.Launch(targetPosition);

            yield return new WaitForSeconds(0.2f);
        }
        Debug.Log($"패턴 시전: 폭발성 씨앗 {seedCount}개 포물선 발사 완료");
    }

    private IEnumerator GroggyStateRoutine()
    {
        isGroggy = true;
        attackPatternCounter = 0;
        Debug.Log("<color=cyan><b>? [이벤트] 보스가 그로기 상태에 빠졌습니다! (받는 피해 30% 증가)</b></color>");
        yield return new WaitForSeconds(4.0f);
        isGroggy = false;
        Debug.Log("보스가 그로기 상태에서 깨어났습니다.");
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        attackPatternCounter = 0;
        currentPhase = 2;

        Debug.Log("<color=yellow><b>?? [페이즈 전환] 덩굴의 수호자 2페이즈 연출 시작</b></color>");
        defenseModifier = 0.7f;
        remainingWeaknessVines = 3;

        yield return new WaitForSeconds(3.0f);
        isPhaseTransitioning = false;
    }

    public void OnWeaknessVineDestroyed()
    {
        if (currentPhase != 2) return;

        remainingWeaknessVines--;
        if (remainingWeaknessVines <= 0)
        {
            defenseModifier = 0f;
            Debug.Log("<color=green><b>보스의 모든 약점 덩굴 제거 완료! 방어 태세 해제</b></color>");
        }
    }

    protected override void Die()
    {
        isDead = true;
        StopAllCoroutines();
        Debug.Log("덩굴의 수호자가 쓰러졌습니다.");
    }
}

// ====================================================================
// 2. [추가] 에러 방지를 위해 같은 파일 하단에 클래스를 함께 선언합니다.
// ====================================================================

public class VineTrapObject : MonoBehaviour
{
    private BossGuardian bossRef;
    private bool isPlayerTrapped = false;
    private float timer = 0f;

    public void Setup(BossGuardian boss)
    {
        bossRef = boss;
        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPlayerTrapped)
        {
            isPlayerTrapped = true;
            timer = 0f;
            Debug.Log("플레이어가 덩굴에 걸려 2초간 속박됩니다! 지속 피해 시작");
        }
    }

    private void Update()
    {
        if (isPlayerTrapped)
        {
            timer += Time.deltaTime;
            if (bossRef != null)
            {
                bossRef.Heal(15f * Time.deltaTime);
            }

            if (timer >= 2.0f)
            {
                isPlayerTrapped = false;
                Destroy(gameObject);
            }
        }
    }
}

public class SeedProjectileObject : MonoBehaviour
{
    private Vector3 targetPos;

    public void Launch(Vector3 destination)
    {
        targetPos = destination;
        StartCoroutine(FlyToTarget());
    }

    private IEnumerator FlyToTarget()
    {
        float progress = 0f;
        Vector3 startPos = transform.position;
        while (progress < 1f)
        {
            progress += Time.deltaTime * 2f;
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * 3f;
            transform.position = currentPos;
            yield return null;
        }

        StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, 3.5f);
        foreach (var player in hitPlayers)
        {
            if (player.CompareTag("Player"))
            {
                Debug.Log("씨앗 폭발! 범위 내 플레이어가 피해를 입었습니다.");
            }
        }
        Destroy(gameObject);
    }
}