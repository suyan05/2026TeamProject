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

    
    [Header("보상 설정")]
    public int rewardGold = 500;

    private Animator anim;
    private Rigidbody rb;

    private bool isExecutingPattern = false;
    private int attackPatternCounter = 0;

    private Coroutine passiveFallingRoutine;
    private Coroutine aiRoutine;
    private Coroutine attackRoutine;

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

        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        patrolCenter = transform.position;
        SetNewPatrolPoint();

        // 코루틴 참조를 안전하게 저장
        aiRoutine = StartCoroutine(BossAIRoutine());
        attackRoutine = StartCoroutine(NormalAttackRoutine());
    }

    private void Update()
    {
        if (!isDead && !isGroggy && !isExecutingPattern)
            RotateTowardPlayer();
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
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
            if (isGroggy || isPhaseTransitioning || isExecutingPattern || playerTransform == null)
            {
                yield return null;
                continue;
            }

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            bool canAttack = dist <= attackRange && IsPlayerInFront();

            if (canAttack)
            {
                if (anim != null) anim.SetBool("Walk", false);
                if (anim != null) anim.CrossFade("Attack", 0.1f);

                if (PlayerMovement.Instance != null)
                    PlayerMovement.Instance.GetDamage(20f, transform);

                yield return new WaitForSeconds(attackCooldown);
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator BossAIRoutine()
    {
        yield return new WaitForSeconds(3f);

        while (!isDead)
        {
            float waitTimer = 0f;
            while (waitTimer < 4f)
            {
                if (isPhaseTransitioning || isGroggy || isDead) break;

                if (playerTransform != null)
                {
                    float dist = Vector3.Distance(transform.position, playerTransform.position);

                    if (dist > attackRange)
                    {
                        if (anim != null) anim.SetBool("Walk", true);

                        if (dist > 12f)
                        {
                            PatrolMove();
                        }
                        else
                        {
                            Vector3 targetPos = playerTransform.position;
                            targetPos.y = transform.position.y;
                            targetPos.z = transform.position.z;

                            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                        }
                    }
                }

                waitTimer += Time.deltaTime;
                yield return null;
            }

            if (isPhaseTransitioning || isGroggy || isDead)
            {
                yield return null;
                continue;
            }

            if (anim != null) anim.SetBool("Walk", false);

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

    private void PatrolMove()
    {
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

    private void Pattern_SummonMinions()
    {
        int count = Random.Range(2, 5);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 3.5f;
            pos.z = 0f;
            pos.y = GetGroundHeight(pos);

            if (minionMushroomPrefab != null)
                Instantiate(minionMushroomPrefab, pos, Quaternion.identity);
        }
    }

    private IEnumerator Pattern_JumpSmash()
    {
        if (playerTransform == null) yield break;

        if (anim != null) anim.CrossFade("JumpStart", 0.1f);

        Vector3 startPos = transform.position;
        Vector3 landPos = playerTransform.position;
        landPos.y = startPos.y;
        landPos.z = 0f;

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
            if (isDead) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / jumpTime;
            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            transform.position = Vector3.Lerp(startPos, landPos, t) + Vector3.up * height;
            yield return null;
        }

        if (jumpSmashIndicator != null)
            jumpSmashIndicator.SetActive(false);

        if (anim != null) anim.CrossFade("JumpLand", 0.1f);

        float radius = 4f;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player") && PlayerMovement.Instance != null)
                PlayerMovement.Instance.GetDamage(30f, transform);
        }
    }

    private IEnumerator Pattern_PlantSporeMines()
    {
        if (playerTransform == null) yield break;

        Vector3 pos = playerTransform.position + Random.insideUnitSphere * 2f;
        pos.y = transform.position.y;
        pos.z = 0f;

        if (sporeMineIndicator != null)
        {
            sporeMineIndicator.transform.position = pos;
            sporeMineIndicator.SetActive(true);
        }

        yield return new WaitForSeconds(1f);
        if (sporeMineIndicator != null) sporeMineIndicator.SetActive(false);

        if (landmineSporePrefab != null)
        {
            GameObject mine = Instantiate(landmineSporePrefab, pos, Quaternion.identity);
            MushroomLandmine script = mine.GetComponent<MushroomLandmine>();
            if (script == null) script = mine.AddComponent<MushroomLandmine>();
            script.Setup();
        }
    }

    private IEnumerator Combo_SuckAndExplode()
    {
        if (anim != null) anim.CrossFade("Static", 0.1f);

        yield return new WaitForSeconds(2f);
        if (isDead) yield break;

        Collider[] hits = Physics.OverlapSphere(transform.position, 6.5f);
        foreach (var h in hits)
        {
            if (h.CompareTag("Player") && PlayerMovement.Instance != null)
                PlayerMovement.Instance.GetDamage(25f, transform);
        }
    }

    private IEnumerator GroggyStateRoutine()
    {
        if (anim != null) anim.CrossFade("Groggy", 0.1f);
        isGroggy = true;
        yield return new WaitForSeconds(5f);
        isGroggy = false;
    }

    protected override IEnumerator PhaseTransitionRoutine()
    {
        isPhaseTransitioning = true;
        if (anim != null) anim.CrossFade("PhaseTransition", 0.1f);

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
            if (isDead || playerTransform == null) yield break;

            Vector3 pos = playerTransform.position + Random.insideUnitSphere * 3f;
            pos.z = 0f;
            pos.y = transform.position.y + 10f;

            if (fallingMushroomPrefab != null)
            {
                GameObject fall = Instantiate(fallingMushroomPrefab, pos, Quaternion.identity);
                MushroomFallingObstacle f = fall.GetComponent<MushroomFallingObstacle>();
                if (f == null) f = fall.AddComponent<MushroomFallingObstacle>();

                Vector3 floorPos = pos;
                floorPos.y = GetGroundHeight(pos);

                f.StartFalling(floorPos);
            }
        }
    }

    protected override void Die()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null) anim.SetTrigger("Die");

        if (aiRoutine != null) StopCoroutine(aiRoutine);
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        if (passiveFallingRoutine != null) StopCoroutine(passiveFallingRoutine);

        
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(rewardGold);
            Debug.Log($"<color=yellow>[골드 지급 성공] 버섯군주 처치! {rewardGold} 골드를 획득했습니다.</color>");
        }
        else
        {
            Debug.LogError("[골드 지급 실패] 현재 씬에서 'CurrencyManager'를 찾을 수 없습니다!");
        }

        if (RewardManager.Instance != null)
            RewardManager.Instance.ShowRewardSelection();

        Destroy(gameObject, 5f);
    }

    private float GetGroundHeight(Vector3 pos)
    {
        if (Terrain.activeTerrain != null)
        {
            return Terrain.activeTerrain.SampleHeight(pos);
        }

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 20f))
        {
            return hit.point.y;
        }
        return transform.position.y;
    }

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
                if (PlayerMovement.Instance != null)
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
                    if (h.CompareTag("Player") && PlayerMovement.Instance != null)
                        PlayerMovement.Instance.GetDamage(20f, transform);
                }

                Destroy(gameObject, 3f);
            }
        }
    }
}