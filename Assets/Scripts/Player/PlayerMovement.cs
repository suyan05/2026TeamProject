using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("플레이어 이동")]
    public float maxSpeed = 5f;   // 이동 속도
    public float jumpForce = 7f;   // 점프 힘
    [Header("가속/감속")]
    public float acceleration = 10f;    // 가속도
    public float deceleration = 10f;    // 감속도

    [Header("레이어 마스크")]
    public LayerMask groundLayerMask;    // 지면 레이어

    [Header("임시용 (화살)")]
    public ArrowController arrowPrefab;
    public Transform firePoint;

    [Header("근접")]
    public float damage = 20f;   // 근접 공격 대미지
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask enemyLayer;    // 적 레이어

    [Header("키")]
    public KeyCode weapon1Key = KeyCode.Alpha1;
    public KeyCode weapon2Key = KeyCode.Alpha2;
    public KeyCode skill1Key = KeyCode.E;
    public KeyCode skill2Key = KeyCode.Q;
    public KeyCode skull3Key = KeyCode.F;

    const KeyCode LeftKey = KeyCode.A;
    const KeyCode RightKey = KeyCode.D;
    const KeyCode JumpKey = KeyCode.Space;
    sbyte lastInputDirection = 1; // 마지막 입력 방향 (-1: 왼쪽, 1: 오른쪽)
    float currentSpeed;   // 현재 이동 속도
    bool isGrounded;    // 지면에 있는지 여부

    Rigidbody2D rb; // 플레이어의 Rigidbody2D 컴포넌트
    Collider2D col; // 플레이어의 Collider2D 컴포넌트

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(skill1Key)) LaunchArrow();
        if (Input.GetKeyDown(skill2Key)) MeleeAttack();
    }

    void LaunchArrow()
    {
        ArrowController arrowScript = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        arrowScript.Shoot(Vector3.right * lastInputDirection, 10f);
    }

    void MeleeAttack()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * lastInputDirection, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            enemyLayer             // 감지할 레이어
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.TryGetComponent<IEnemyCombat>(out IEnemyCombat enemyCombat))
                {
                    enemyCombat.GetDamage(damage, transform);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateStates();

        MoveHandler();
        JumpHandler();
        RotationHandler();
    }

    void MoveHandler()
    {
        if (TryGetHorizontalInput(out sbyte horizontal))
        {
            if (!(lastInputDirection != horizontal && currentSpeed > maxSpeed * 0.1f))
            {
                lastInputDirection = horizontal;
                currentSpeed += acceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            }
            else
            {
                currentSpeed -= deceleration * Time.fixedDeltaTime * 1.5f;
                currentSpeed = Mathf.Max(currentSpeed, 0f);
            }
        }
        else
        {
            currentSpeed -= deceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);
        }

        rb.linearVelocity = new Vector2(currentSpeed * lastInputDirection, rb.linearVelocity.y);
    }

    void JumpHandler()
    {
        if (isGrounded && Input.GetKey(JumpKey))
        {
            Vector2 jumpVector = Vector2.up * jumpForce;
            jumpVector.x = rb.linearVelocity.x;
            rb.linearVelocity = jumpVector;
        }
    }

    bool TryGetHorizontalInput(out sbyte horizontal)
    {
        bool left = Input.GetKey(LeftKey);
        bool right = Input.GetKey(RightKey);

        if (left && right)
        {
            horizontal = 0;
            return false;
        }
        else if (left)
        {
            horizontal = -1;
            return true;
        }
        else if (right)
        {
            horizontal = 1;
            return true;
        }
        else
        {
            horizontal = 0;
            return false;
        }
    }

    void RotationHandler()
    {
        float targetYAngle = (lastInputDirection == 1) ? 0.01f : 179.99f;

        Quaternion targetRotation = Quaternion.Euler(0, targetYAngle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
    }

    private void OnCollisionStay2D(Collision2D collision)   // 벽 충돌 시 속도 초기화
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if ((lastInputDirection > 0 && contact.normal.x < -0.5f) ||
                    (lastInputDirection < 0 && contact.normal.x > 0.5f))
                {
                    currentSpeed = 0f;
                }
            }
        }
    }

    private void UpdateStates()
    {
        isGrounded = CheckIsGrounded();
    }

    bool CheckIsGrounded()
    {
        Vector2 rayStart = new Vector2(col.bounds.center.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 0.05f, groundLayerMask);

        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * lastInputDirection, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));
    }
}