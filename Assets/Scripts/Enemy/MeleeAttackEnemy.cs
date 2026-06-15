using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MeleeAttackEnemy : MonoBehaviour, IEnemyCombat
{
    [Header("체력")]
    public float maxHp = 30f;

    [Header("적 데이터")]
    public EnemyData enemyData;

    [Header("움직임")]
    public float maxSpeed = 3; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f; // 가속도

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform upperGroundCheckPos;    // 땅 위쪽
    public Transform lowerGroundCheckPos;    // 땅 아래쪽

    [Header("공격")]
    public float readyToAttackTime = 0.5f;  // 공격으로 전환 시간
    public float attackChargeTime = 0.42f;  // 공격의 준비 시간
    public float attackCooldown = 0.35f;    // 공격 대기시간 (공격 쿨타임)

    [Header("대미지")]
    public float damage = 0.4f;  // 공격 대미지

    [Header("히트박스")]
    public Vector3 hitboxOffset = Vector3.zero;    //  3D 오프셋(Vector3)
    public Vector3 hitboxSize = new Vector3(1.0f, 1.0f, 1.0f); //  3D 크기(Vector3)

    [Header("시야 범위")]
    public Vector3 viewOffset = new Vector3(0f, 0.5f, 0f); //  3D 시야 오프셋
    public Vector3 viewSize = new Vector3(5f, 3f, 5f);       //  3D 시야 영역 크기

    [Header("감지, 레이어")]
    public float detectionDecayTime = 3f;   // 플레이어 감지 후 감지율이 0으로 떨어지는 시간
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 감지 레이어

    [Header("적 HP바 자동 생성")]
    public GameObject hpBarPrefab;
    private EnemyHPBarFollow hpBar;

    private int facingSign = 1; // 바라보는 방향

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float detectionRate = 0;    // 플레이어 감지율 (0~1)
    public float currentHp;    // 현재 체력

    // 💥 센서 크기를 넉넉하게 키웠습니다 (0.05f -> 0.2f)
    private float layerCheckRadius = 0.2f;

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부
    private bool isDead = false;    // 죽었는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, track, attack, endAttack }
    state currentState;

    private Rigidbody rb;
    GameObject playerObject;    // 플레이어 오브젝트
    public float dashTime;
    public float dashSpeed;

    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        if (PlayerMovement.Instance != null)
        {
            playerObject = PlayerMovement.Instance.gameObject;
        }

        isFacingRight = true;
        currentHp = maxHp;
        SetState(state.idle);

        if (EnemyCounter.Instance != null) EnemyCounter.Instance.AddEnemy();

        if (hpBarPrefab != null && UIManager.Instance != null && UIManager.Instance.worldCanvas != null)
        {
            GameObject ui = Instantiate(hpBarPrefab, UIManager.Instance.worldCanvas.transform);
            hpBar = ui.GetComponent<EnemyHPBarFollow>();
            hpBar.target = transform;
            hpBar.offset = new Vector3(0, 1.2f, 0);
        }
        else
        {
            if (hpBarPrefab == null) Debug.LogError("HP Bar 프리팹이 연결되지 않았습니다!");
            if (UIManager.Instance == null) Debug.LogError("씬에 UIManager가 없습니다!");
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateStates();

        if (anim != null && currentState != state.attack)
        {
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        }

        if (playerObject != null)
        {
            switch (currentState)
            {
                case state.idle:
                    if (IsPlayerInView())
                    {
                        SetState(state.track);
                    }
                    break;

                case state.track:
                    TrackHandler();
                    break;
            }
        }
        else if (currentState != state.idle)
        {
            SetState(state.idle);
        }
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        Vector3 originVelocity = rb.linearVelocity;
        originVelocity.x = 0;
        rb.linearVelocity = originVelocity;
        currentNormalizedSpeed = 0;

        if (targetState == state.idle || playerObject == null)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = isFacingRight ? movePosRight : movePosLeft;

            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.attack)
        {
            StartCoroutine(AttackHandler());
        }
        else if (targetState == state.endAttack)
        {
            StartCoroutine(EndAttack());
        }
    }

    void SwitchPos()
    {
        Flip();
        targetPos = isFacingRight ? movePosRight : movePosLeft;
    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

            // 순찰 중일 때는 바닥이 있는지(canGoStraight) 확인합니다.
            while (!HasArrived(targetPos) && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.5f);
                rb.linearVelocity = new Vector3(sign * currentNormalizedSpeed * maxSpeed, rb.linearVelocity.y, 0f);
                yield return null;
            }
            Vector3 originVelocity = rb.linearVelocity;
            originVelocity.x = 0;
            rb.linearVelocity = originVelocity;
            currentNormalizedSpeed = 0;

            yield return new WaitForSeconds(trunDuration);
            SwitchPos();
            yield return null;
        }
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
    }

    void TrackHandler()
    {
        if (playerObject == null) return;
        LookPos(playerObject.transform.position);

        Vector3 checkPos = playerObject.transform.position;
        checkPos.y = transform.position.y;

        // 💥 플레이어를 발견하면 벽/바닥 눈치 안 보고 무지성 돌격! (canGoStraight 제거)
        if (!HasArrived(checkPos))
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.505f, 1f);
            rb.linearVelocity = new Vector3(facingSign * currentNormalizedSpeed * maxSpeed, rb.linearVelocity.y, 0f);
        }
        else
        {
            Vector3 originVelocity = rb.linearVelocity;
            originVelocity.x = 0;
            rb.linearVelocity = originVelocity;
            currentNormalizedSpeed = 0;
        }

        if (IsPlayerInRange())
        {
            SetState(state.attack);
            return;
        }

        if (IsPlayerInView())
        {
            detectionRate += 1f;
        }
        else
        {
            detectionRate -= Time.deltaTime / detectionDecayTime;
        }

        detectionRate = Mathf.Clamp01(detectionRate);

        if (detectionRate == 0)
        {
            SetState(state.endAttack);
        }
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        UpdateStates();
    }

    bool IsPlayerInRange()
    {
        Vector3 localAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        Collider[] hitTargets = Physics.OverlapBox(worldCenter, hitboxSize / 2f, Quaternion.identity, playerLayer);

        return hitTargets.Length > 0;
    }

    IEnumerator AttackHandler()
    {
        while (true)
        {
            if (anim != null)
            {
                anim.Play("Armature|Armature|Attack", 0, 0f);
            }

            yield return new WaitForSeconds(attackChargeTime);

            Attack();

            yield return new WaitForSeconds(attackCooldown);

            if (!IsPlayerInRange()) break;
        }

        if (anim != null) anim.Play("Armature|Armature|Idle", 0, 0f);

        SetState(state.track);
    }

    void Attack()
    {
        Vector3 localAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        Collider[] hitTargets = Physics.OverlapBox(worldCenter, hitboxSize / 2f, Quaternion.identity, playerLayer);

        if (hitTargets.Length > 0)
        {
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.GetDamage(damage, transform);
            }
        }
    }

    IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }

    bool IsPlayerInView()
    {
        Vector3 localAdjustedOffset = new Vector3(viewOffset.x * facingSign, viewOffset.y, 0f);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        Collider[] hitTargets = Physics.OverlapBox(worldCenter, viewSize / 2f, Quaternion.identity, playerLayer);

        if (hitTargets.Length == 0) return false;

        // 💥 시야에 들어온 플레이어를 타겟으로 업데이트
        playerObject = hitTargets[0].gameObject;

        // 💥 레이저 발사 위치 상향 조정 (바닥 긁힘 방지)
        Vector3 startPos = transform.position + Vector3.up * 0.5f;
        Vector3 endPos = playerObject.transform.position + Vector3.up * 0.5f;

        Vector3 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit hit;
        if (Physics.Raycast(startPos, direction, out hit, distance, obstacleMask))
        {
            if (hit.collider != null) return false;
        }
        return true;
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool upperGroundDetect = Physics.CheckSphere(upperGroundCheckPos.position, layerCheckRadius, obstacleMask);
        bool lowerGroundDetect = Physics.CheckSphere(lowerGroundCheckPos.position, layerCheckRadius, obstacleMask);

        // 💥 오작동 유발하는 벽 센서 검사 제거. 바닥만 있으면 갈 수 있다고 판단!
        canGoStraight = upperGroundDetect || lowerGroundDetect;
    }

    public void GetDamage(float damage, Transform attackerTransform)
    {
        currentHp -= damage;

        if (hpBar != null)
            hpBar.SetHP(currentHp, maxHp);

        if (currentHp <= 0f)
        {
            Dead();
        }
    }

    void Dead()
    {
        if (isDead) return;
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (enemyData != null && CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGold(enemyData.dropGold);
            CurrencyManager.Instance.AddGem(enemyData.dropGem);
        }

        if (EnemyCounter.Instance != null) EnemyCounter.Instance.EnemyDefeated();
        if (hpBar != null) Destroy(hpBar.gameObject);

        Destroy(gameObject, 1.0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 hitboxLocalAdjustedOffset = new Vector3(hitboxOffset.x * facingSign, hitboxOffset.y, 0f);
        Vector3 hitboxGizmoCenter = transform.position + hitboxLocalAdjustedOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);

        Gizmos.color = Color.blue;
        Vector3 viewLocalAdjustedOffset = new Vector3(viewOffset.x * facingSign, viewOffset.y, 0f);
        Vector3 viewGizmoCenter = transform.position + viewLocalAdjustedOffset;
        Gizmos.DrawWireCube(viewGizmoCenter, viewSize);

        Gizmos.color = Color.cyan;
        if (Application.isPlaying)
        {
            if (currentState == state.idle)
            {
                Gizmos.DrawWireSphere(movePosRight, 0.25f);
                Gizmos.DrawWireSphere(movePosLeft, 0.25f);
                Gizmos.DrawLine(movePosRight, movePosLeft);
            }
        }
        else
        {
            Vector3 gizmosMovePosRight = transform.position;
            Vector3 gizmosMovePosLeft = transform.position;
            gizmosMovePosRight.x += moveRadius;
            gizmosMovePosLeft.x -= moveRadius;

            Gizmos.DrawWireSphere(gizmosMovePosRight, 0.25f);
            Gizmos.DrawWireSphere(gizmosMovePosLeft, 0.25f);
            Gizmos.DrawLine(gizmosMovePosRight, gizmosMovePosLeft);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(upperGroundCheckPos.position, layerCheckRadius); 
        Gizmos.DrawWireSphere(lowerGroundCheckPos.position, layerCheckRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, layerCheckRadius);
    }
}