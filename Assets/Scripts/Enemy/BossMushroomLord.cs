using UnityEngine;
using System.Collections;

public class BossMushroomLord : BossBase
{
    [Header("Player Target")]
    public Transform playerTransform;

    [Header("Pattern Prefabs")]
    public GameObject minionMushroomPrefab;
    public GameObject landmineSporePrefab;
    public GameObject fallingMushroomPrefab;

    [Header("UI Indicators")]
    public GameObject sporeMineIndicator;
    public GameObject jumpSmashIndicator;
    public GameObject fallingIndicatorPrefab;

    private Animator anim;
    private Rigidbody rb;

    private bool isExecutingPattern = false;
    private int attackPatternCounter = 0;

    private Coroutine passiveFallingRoutine;

    [Header("Patrol Settings")]
    public float patrolRadius = 6f;
    public float patrolSpeed = 2f;
    public float attackCooldown = 2f;
    private Vector3 patrolCenter;
    private Vector3 patrolTarget;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        // 자동 플레이어 탐색
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        patrolCenter = transform.position;
        SetNewPatrolPoint();

        StartCoroutine(BossAIRoutine());
        StartCoroutine(NormalAttackRoutine());
    }

    private void Update()
    {
        if (!isDead && !isGroggy && !isExecutingPattern)
            RotateTowardPlayer();
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = 0f; // Z축 고정
        transform.position = pos;
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

    private IEnumerator NormalAttackRoutine()
    {
        yield return new WaitForSeconds(3.5f);

        while (!isDead)
        {
            // 공격 불가 상태
            if (isGroggy || isPhaseTransitioning || isExecutingPattern || playerTransform == null)
            {
                anim.SetBool("Walk", false);
                yield return null;
                continue;
            }

            float dist = Vector3.Distance(transform.position, playerTransform.position);

            // 공격 조건
            bool canAttack =
                dist <= attackRange &&     // 공격 범위 안
                IsPlayerInFront() &&       // 정면
                !isExecutingPattern &&     // 패턴 중 아님
                !isGroggy &&               // 그로기 아님
                !isPhaseTransitioning;     // 페이즈 전환 중 아님

            if (canAttack)
            {
                anim.SetBool("Walk", false);
                anim.CrossFade("Attack", 0.1f);

                // 플레이어 데미지
                PlayerMovement.Instance.GetDamage(20f, transform);

                yield return new WaitForSeconds(attackCooldown);
            }
            else
            {
                // 공격 범위 밖 → 이동 또는 패트롤
                anim.SetBool("Walk", true);

                // 플레이어가 멀면 패트롤
                if (dist > 12f)
                    PatrolMove();

                yield return null;
            }
        }
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

    private IEnumerator BossAIRoutine()
    {
        yield return new WaitForSeconds(3f);

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

            isExecutingPattern = true;

            if (currentPhase == 1)
            {
                int p = Random.Range(0, 3);
                if (p == 0) Pattern_SummonMinions();
                else if (p == 1) yield return StartCoroutine(Pattern_JumpSmash());
                else yield return StartCoroutine(Pattern_PlantSporeMines());
            }
            else
            {
                int combo = Random.Range(0, 2);
                if (combo == 0) yield return StartCoroutine(Combo_SuckAndExplode());
                else yield return StartCoroutine(Pattern_JumpSmash());
            }

            isExecutingPattern = false;
        }
    }

    private void Pattern_SummonMinions()
    {
        int count = Random.Range(2, 5);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 3.5f;
            pos.y = Terrain.activeTerrain.SampleHeight(pos);

            Instantiate(minionMushroomPrefab, pos, Quaternion.identity);
        }
    }

    private IEnumerator Pattern_JumpSmash()
    {
        if (playerTransform == null) yield break;

        anim.CrossFade("JumpStart", 0.1f);

        Vector3 startPos = transform.position;
        Vector3 landPos = playerTransform.position;
        landPos.y = startPos.y;

        float jumpTime = 1.2f;
        float elapsed = 0f;
        float jumpHeight = 4f;

        if (jumpSmashIndicator != null)
        {
            jumpSmashIndicator.transform.position = landPos;
            jumpSmashIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        while (elapsed < jumpTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpTime;

            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            transform.position = Vector3.Lerp(startPos, landPos, t) + Vector3.up * height;

            yield return null;
        }

        if (jumpSmashIndicator != null)
            jumpSmashIndicator.SetActive(false);

        anim.CrossFade("JumpLand", 0.1f);

        float radius = 4f;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
                PlayerMovement.Instance.GetDamage(30f, transform);
        }
    }

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

    private IEnumerator Combo_SuckAndExplode()
    {
        anim.CrossFade("Static", 0.1f);

        yield return new WaitForSeconds(2f);

        Collider[] hits = Physics.OverlapSphere(transform.position, 6.5f);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
                PlayerMovement.Instance.GetDamage(25f, transform);
        }
    }

    private IEnumerator GroggyStateRoutine()
    {
        anim.CrossFade("Groggy", 0.1f);
        isGroggy = true;
        yield return new WaitForSeconds(5f);
        isGroggy = false;
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        anim.CrossFade("PhaseTransition", 0.1f);

        yield return new WaitForSeconds(3f);

        currentPhase++;

        if (currentPhase == 2 && passiveFallingRoutine == null)
            passiveFallingRoutine = StartCoroutine(PassiveFallingRoutine());

        isPhaseTransitioning = false;
    }

    private IEnumerator PassiveFallingRoutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));

            Vector3 pos = playerTransform.position + Random.insideUnitSphere * 3f;
            pos.y = transform.position.y + 10f;

            GameObject fall = Instantiate(fallingMushroomPrefab, pos, Quaternion.identity);

            MushroomFallingObstacle f = fall.GetComponent<MushroomFallingObstacle>();
            if (f == null) f = fall.AddComponent<MushroomFallingObstacle>();

            Vector3 floorPos = pos;
            floorPos.y = Terrain.activeTerrain.SampleHeight(pos);

            f.StartFalling(floorPos);
        }
    }

    protected override void Die()
    {
        isDead = true;

        anim.SetTrigger("Die");

        StopAllCoroutines();

        if (RewardManager.Instance != null)
            RewardManager.Instance.ShowRewardSelection();

        Destroy(gameObject, 5f);
    }

    // ---------------- Sub Classes ----------------

    public class MushroomLandmine : MonoBehaviour
    {
        private bool isTriggered = false;

        public void Setup()
        {
            Destroy(gameObject, 5f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isTriggered)
            {
                isTriggered = true;
                PlayerMovement.Instance.GetDamage(15f, transform);
                Destroy(gameObject);
            }
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
}
