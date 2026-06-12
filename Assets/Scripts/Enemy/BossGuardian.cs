using UnityEngine;
using System.Collections;

public class BossGuardian : BossBase
{
    [Header("Player Target")]
    public Transform playerTransform;

    [Header("�ν� ���� ����")]
    public float detectionRange = 5.0f;        // ���� 5m ������ ������ �߰��� �����մϴ�!
    private bool isPlayerDetected = false;

    [Header("Pattern Prefabs")]
    public GameObject vineTrapPrefab;
    public GameObject smashWavePrefab;
    public GameObject seedProjectilePrefab;
    public Transform shootPoint;

    [Header("Indicators")]
    public GameObject redCircleIndicator;
    public GameObject yellowDotIndicator;
    public GameObject warningLineIndicator;

    [Header("Phase 2 Settings")]
    public int remainingWeaknessVines = 3;
    private float defenseModifier = 0f;

    private Animator anim;
    private Rigidbody rb;

    private int attackPatternCounter = 0;
    private bool isExecutingPattern = false;

    [Header("Patrol Settings")]
    public float patrolRadius = 6f;
    public float patrolSpeed = 2f;
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;

    private Rigidbody rb;
    private Animator anim;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // ?? [�ڵ� ���� ������ġ] Ȥ�� �ν����Ϳ� Ÿ���� �� �־�� Player �±׷� �ڵ� ���� ����!
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        StartCoroutine(BossAIRoutine());
    }

    private void Update()
    {
        if (!isDead && !isGroggy && !isExecutingPattern)
            RotateTowardPlayer();
    }

    private void RotateTowardPlayer()
    {
        if (playerTransform == null) return;

        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
        }
    }

    private bool IsPlayerInFront()
    {
        if (playerTransform == null) return false;

        Vector3 dir = (playerTransform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dir);

        return dot > 0.6f;
    }

    private void PatrolMove()
    {
        anim.SetBool("Walk", true);

        transform.position = Vector3.MoveTowards(
            transform.position,
            patrolTarget,
            patrolSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, patrolTarget) < 0.5f)
            SetNewPatrolPoint();
    }

    private void SetNewPatrolPoint()
    {
        Vector2 rand = Random.insideUnitCircle * patrolRadius;
        patrolTarget = patrolCenter + new Vector3(rand.x, 0, rand.y);
    }

    public override void TakeDamage(float damage)
    {
        if (currentPhase == 2 && remainingWeaknessVines > 0)
            damage *= (1f - defenseModifier);

        base.TakeDamage(damage);
    }

    private IEnumerator BossAIRoutine()
    {
        // ================= [1�ܰ�: ö���� �÷��̾� ���� ���] =================
        while (!isPlayerDetected)
        {
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance <= detectionRange)
                {
                    isPlayerDetected = true;
                    Debug.Log("<color=red><b>[���] ������ �÷��̾ �����߽��ϴ�! �߰��� �����մϴ�.</b></color>");
                }
            }

            if (anim != null) anim.SetBool("Walk", false);
            yield return new WaitForSeconds(0.2f);
        }
        // ===================================================================

        // �÷��̾� ���� ���� �� ù �ൿ �� ������� (2��)
        yield return new WaitForSeconds(2.0f);

        // ================= [2�ܰ�: �������� �߰� �� ���� ����] =================
        while (!isDead)
        {
            if (isPhaseTransitioning || isGroggy)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // ???��? [�ٽ� ���� �ڵ� �߰�] �ܼ� ��� ���, 3.5�� ���� �÷��̾ ��¥�� �Ѿư��ϴ�!
            if (anim != null) anim.SetBool("Walk", true);

            float moveTimer = 0f;
            while (moveTimer < 3.5f)
            {
                // �̵� ���� ����� �ٲ�ų� �׷α⿡ �ɸ��� ��� �̵� �ߴ�
                if (isPhaseTransitioning || isGroggy || isDead) break;

                if (playerTransform != null)
                {
                    // 1. �÷��̾ ���� �ε巴�� ȸ�� (����� ������ ������ �ʰ� Y���� ����)
                    Vector3 direction = playerTransform.position - transform.position;
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                    }

                    // 2. �θ� Ŭ����(BossBase)�� moveSpeed �ӵ��� �÷��̾ ���� ��¥ ��ǥ �̵�!
                    transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
                }

                moveTimer += Time.deltaTime;
                yield return null; // �� ������ �� �帣���� ���� ����
            }

            // �߰��� ������ ���� ������ ���� �̵� ����
            if (isPhaseTransitioning || isGroggy || isDead) continue;

            // �׷α� ī��Ʈ üũ
            attackPatternCounter++;
            if (attackPatternCounter >= 4)
            {
                if (anim != null) anim.SetBool("Walk", false);
                yield return StartCoroutine(GroggyStateRoutine());
                continue;
            }

            // ���� ���� �ܰ�: ������ ���� �ȱ� �ִϸ��̼��� ��� Idle ���¿��� Ʈ���� �۵�
            if (anim != null) anim.SetBool("Walk", false);
            yield return new WaitForSeconds(0.1f);

            // ���� ���� �ߵ�
            if (currentPhase == 1)
            {
                int p = Random.Range(0, 3);
                if (p == 0) yield return StartCoroutine(Pattern_VineBind());
                else if (p == 1) yield return StartCoroutine(Pattern_Smash());
                else yield return StartCoroutine(Pattern_SpawnSeeds());
            }
            else
            {
                int combo = Random.Range(0, 2);

                if (combo == 0)
                {
                    Debug.Log("<color=red>[2������ ���� �޺�] ���� �ӹ� ��� -> ������� ���� ����!</color>");
                    yield return StartCoroutine(Pattern_VineBind());
                    yield return new WaitForSeconds(0.5f);
                    yield return StartCoroutine(Pattern_Smash());
                }
                else
                {
                    Debug.Log("<color=red>[2������ ���� �޺�] ���� ����ü �߻� -> ���� ä�� �ֵθ��� ����!</color>");
                    yield return StartCoroutine(Pattern_SpawnSeeds());
                    yield return new WaitForSeconds(0.8f);

                    if (anim != null) anim.SetTrigger("Attack");
                    Debug.Log("���� �׼�: ���� ���� ä�� �ֵθ���� ���� Ÿ��!");
                }
            }

            isExecutingPattern = false;
        }
    }

    private IEnumerator Pattern_VineBind()
    {
        if (playerTransform == null) yield break;

        if (redCircleIndicator != null)
        {
            redCircleIndicator.transform.position =
                new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            redCircleIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(1.5f);

        if (redCircleIndicator != null)
            redCircleIndicator.SetActive(false);

        if (anim != null) anim.SetTrigger("Attack");

        GameObject vine = Instantiate(vineTrapPrefab, playerTransform.position, Quaternion.identity);
        VineTrapObject vineScript = vine.GetComponent<VineTrapObject>();
        if (vineScript == null) vineScript = vine.AddComponent<VineTrapObject>();
        vineScript.Setup(this);

        Debug.Log("���� ����: �÷��̾� �߹ؿ� �ӹ� ���� ��ȯ");
    }

    private IEnumerator Pattern_Smash()
    {
        if (warningLineIndicator != null)
            warningLineIndicator.SetActive(true);

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
        Debug.Log("���� ����: ���� ������� �� ����� �߻�");
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
    }
}

// [���� Ŭ�������� �ϴܿ� �����ϰ� �����ǹǷ� ���ǻ� ����]



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
        }
    }

    private void Update()
    {
        if (isPlayerTrapped)
        {
            timer += Time.deltaTime;

            bossRef.Heal(15f * Time.deltaTime);

            if (PlayerMovement.Instance != null && Time.frameCount % 30 == 0)
            {
                PlayerMovement.Instance.GetDamage(2f, transform);
            }

            if (timer >= 2.0f)
            {
                isPlayerTrapped = false;
                Destroy(gameObject);
            }
        }
    }
}

// =============================================================
// Seed Projectile
// =============================================================
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

        yield return StartCoroutine(ExplosionRoutine());
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
                Debug.Log("���� ����! ���� �� �÷��̾ ���ظ� �Ծ����ϴ�.");
            }
        }

        Destroy(gameObject);
    }
}
