using UnityEngine;
using System.Collections;

public class BossGuardian : BossBase
{
    [Header("Player Target")]
    public Transform playerTransform;

    [Header("인식 범위 설정")]
    public float detectionRange = 5.0f;
    private bool isPlayerDetected = false;

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

 
    [Header("보상 설정")]
    public int rewardGold = 500;

    private int attackPatternCounter = 0;

    private Rigidbody rb;
    private Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

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
        while (!isPlayerDetected)
        {
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance <= detectionRange)
                {
                    isPlayerDetected = true;
                    Debug.Log("<color=red><b>[경고] 보스가 플레이어를 감지했습니다! 추격을 시작합니다.</b></color>");
                }
            }

            if (anim != null) anim.SetBool("Walk", false);
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(2.0f);

        while (!isDead)
        {
            if (isPhaseTransitioning || isGroggy)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            if (anim != null) anim.SetBool("Walk", true);

            float moveTimer = 0f;
            while (moveTimer < 3.5f)
            {
                if (isPhaseTransitioning || isGroggy || isDead) break;

                if (playerTransform != null)
                {
                    Vector3 direction = playerTransform.position - transform.position;
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                    }

                    transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
                }

                moveTimer += Time.deltaTime;
                yield return null;
            }

            if (isPhaseTransitioning || isGroggy || isDead) continue;

            attackPatternCounter++;
            if (attackPatternCounter >= 4)
            {
                if (anim != null) anim.SetBool("Walk", false);
                yield return StartCoroutine(GroggyStateRoutine());
                continue;
            }

            if (anim != null) anim.SetBool("Walk", false);
            yield return new WaitForSeconds(0.1f);

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
                    Debug.Log("<color=red>[2페이즈 연계 콤보] 덩굴 속박 흡수 -> 내리찍기 연계 시작!</color>");
                    yield return StartCoroutine(Pattern_VineBind());
                    yield return new WaitForSeconds(0.5f);
                    yield return StartCoroutine(Pattern_Smash());
                }
                else
                {
                    Debug.Log("<color=red>[2페이즈 연계 콤보] 씨앗 투사체 발사 -> 덩굴 채찍 휘두르기 시작!</color>");
                    yield return StartCoroutine(Pattern_SpawnSeeds());
                    yield return new WaitForSeconds(0.8f);

                    if (anim != null) anim.SetTrigger("Attack");
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

        if (anim != null) anim.SetTrigger("Attack");

        GameObject vine = Instantiate(vineTrapPrefab, playerTransform.position, Quaternion.identity);
        VineTrapObject vineScript = vine.GetComponent<VineTrapObject>();
        if (vineScript == null) vineScript = vine.AddComponent<VineTrapObject>();
        vineScript.Setup(this);

        Debug.Log("패턴 시전: 플레이어 발밑에 속박 덩굴 소환");
    }

    private IEnumerator Pattern_Smash()
    {
        if (warningLineIndicator != null) warningLineIndicator.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        if (warningLineIndicator != null) warningLineIndicator.SetActive(false);

        if (anim != null) anim.SetTrigger("Attack");

        Instantiate(smashWavePrefab, transform.position + transform.forward * 1.5f, transform.rotation);

        Collider[] hitPlayers = Physics.OverlapSphere(transform.position + transform.forward * 2f, 3.0f);
        foreach (var hit in hitPlayers)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerMovement.Instance.GetDamage(25f, transform);
            }
        }
        Debug.Log("패턴 시전: 전방 내리찍기 및 충격파 발사");
    }

    private IEnumerator Pattern_SpawnSeeds()
    {
        int seedCount = Random.Range(2, 4);
        if (anim != null) anim.SetTrigger("Attack");

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
            SeedProjectileObject seedScript = seed.GetComponent<SeedProjectileObject>();
            if (seedScript == null) seedScript = seed.AddComponent<SeedProjectileObject>();
            seedScript.Launch(targetPosition);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator GroggyStateRoutine()
    {
        isGroggy = true;
        attackPatternCounter = 0;
        yield return new WaitForSeconds(4.0f);
        isGroggy = false;
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        attackPatternCounter = 0;
        currentPhase = 2;
        if (anim != null) anim.SetBool("Walk", false);
        defenseModifier = 0.7f;
        remainingWeaknessVines = 3;
        yield return new WaitForSeconds(3.0f);
        isPhaseTransitioning = false;
    }

    public void OnWeaknessVineDestroyed()
    {
        if (currentPhase != 2) return;
        remainingWeaknessVines--;
        if (remainingWeaknessVines <= 0) defenseModifier = 0f;
    }

    protected override void Die()
    {
        isDead = true;
        if (anim != null) anim.SetTrigger("Die");
        if (anim != null) anim.SetBool("Walk", false);
        StopAllCoroutines();

      
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(rewardGold);
        }

       
        Destroy(gameObject, 3.0f);
    }
}

public class VineTrapObject : MonoBehaviour
{
    private BossGuardian bossRef;
    private bool isPlayerTrapped = false;
    private float floatTimer = 0f;

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
            floatTimer = 0f;
            Debug.Log("플레이어가 덩굴에 걸려 2초간 속박됩니다! 지속 피해 시작");
        }
    }

    private void Update()
    {
        if (isPlayerTrapped)
        {
            floatTimer += Time.deltaTime;
            if (bossRef != null)
            {
                bossRef.Heal(15f * Time.deltaTime);
            }

            if (PlayerMovement.Instance != null && Time.frameCount % 30 == 0)
            {
                PlayerMovement.Instance.GetDamage(2f, transform);
            }

            if (floatTimer >= 2.0f)
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
                PlayerMovement.Instance.GetDamage(15f, transform);
                Debug.Log("씨앗 폭발! 범위 내 플레이어가 피해를 입었습니다.");
            }
        }
        Destroy(gameObject);
    }
}