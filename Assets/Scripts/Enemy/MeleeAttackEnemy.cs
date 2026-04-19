using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MeleeAttackEnemy : MonoBehaviour, IEnemyCombat
{
    [Header("체력")]
    public float maxHp = 30f;   // 최대 체력

    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
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
    public float knockbackPower = 1f;
    public float knockbacktime = 0.05f;

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("감지, 레이어")]
    public float detectionDecayTime = 3f;   // 플레이어 감지 후 감지율이 0으로 떨어지는 시간
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 감지 레이어

    private int facingSign = 1; // 바라보는 방향

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float detectionRate = 0;    // 플레이어 감지율 (0~1)
    public float currentHp;    // 현재 체력
    private const float layerCheckRadius = 0.05f;  // 감지 위치 반경

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isDead = false;    // 죽었는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, track, attack, endAttack }
    state currentState;

    private Rigidbody2D rb;
    GameObject playerObject;    // 플레이어 오브젝트

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        playerObject = PlayerMovement.Instance.gameObject;

        isFacingRight = true;
        currentHp = maxHp;
        SetState(state.idle);
    }

    void Update()
    {
        if (isDead) return;

        UpdateStates();
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

        Vector2 originVelocity = rb.linearVelocity;
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

            while (!HasArrived(targetPos) && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.5f);
                rb.linearVelocity = new Vector2(sign * currentNormalizedSpeed * maxSpeed, rb.linearVelocity.y);
                yield return null;
            }
            Vector2 originVelocity = rb.linearVelocity;
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

        Vector2 checkPos = playerObject.transform.position;
        checkPos.y = transform.position.y;

        if (canGoStraight && !HasArrived(checkPos))
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.505f, 1f);
            rb.linearVelocity = new Vector2(facingSign * currentNormalizedSpeed * maxSpeed, rb.linearVelocity.y);
        }
        else
        {
            Vector2 originVelocity = rb.linearVelocity;
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
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.gameObject == playerObject)
                {
                    return true;
                }
            }
        }

        return false;
    }

    IEnumerator AttackHandler()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackChargeTime);
            Attack();
            yield return new WaitForSeconds(attackCooldown);

            if (!IsPlayerInRange()) break;
        }

        SetState(state.track);
    }

    void Attack()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.gameObject == playerObject)
                {
                    PlayerMovement.Instance.GetDamage(damage, transform);
                    break;
                }
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
        Vector2 localAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            viewSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        bool isPlayerInView = false;

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.gameObject == playerObject)
                {
                    isPlayerInView = true;
                    break;
                }
            }
        }

        if (!isPlayerInView) return false;

        Vector2 startPos = transform.position;
        Vector2 endPos = playerObject.transform.position;
        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance, obstacleMask);
        if (hit.collider != null) return false;
        return true;
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool upperGroundDetect = Physics2D.OverlapCircle(upperGroundCheckPos.position, layerCheckRadius, obstacleMask);
        bool lowerGroundDetect = Physics2D.OverlapCircle(lowerGroundCheckPos.position, layerCheckRadius, obstacleMask);

        bool isGrounded = upperGroundDetect || lowerGroundDetect;
        bool isTouchingAnyWall = Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = isGrounded && !isTouchingAnyWall;
    }

    public void GetDamage(float damage, Transform attackerTransform)
    {
        currentHp -= damage;
        if (currentHp <= 0f)
        {
            Dead();
        }
    }

    void Dead()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.color = Color.blue;

        Vector2 viewLocalAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 viewGizmoCenter = (Vector2)transform.position + viewLocalAdjustedOffset;

        Gizmos.DrawWireCube(viewGizmoCenter, new Vector3(viewSize.x, viewSize.y, 0f));

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
        Gizmos.DrawWireSphere(upperGroundCheckPos.position, 0.05f);
        Gizmos.DrawWireSphere(lowerGroundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
    }
}