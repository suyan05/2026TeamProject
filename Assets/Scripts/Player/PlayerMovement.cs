using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("플레이어 움직임 제한")]
    public bool controlLocked = false;

    [Header("플레이어 최대 체력")]
    [SerializeField] public readonly float maxHp = 100.0f;
    
    [Header("플레이어 이동")]
    public float maxSpeed = 5f;   // 이동 속도
    public float jumpForce = 7f;   // 점프 힘
    
    [Header("가속/감속")]
    public float acceleration = 10f;    // 가속도
    public float deceleration = 10f;    // 감속도

    [Header("애니메이션")]
    public Animator animator;   // 플레이어 애니메이터

    [Header("레이어 마스크")]
    public LayerMask groundLayerMask;    // 지면 레이어

    [Header("임시용 (화살)")]
    public ArrowController arrowPrefab;
    public float maxArrowAngle = 30f;   // 최대 발사 각도 (수평 기준)
    public Transform firePoint;

    [Header("근접")]
    public float damage = 20f;   // 근접 공격 대미지
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask enemyLayer;    // 적 레이어

    [Header("구르기")]
    public float rollDuration = 0.5f; // 구르기 지속 시간
    public float rollSpeedMultiplier = 1.5f; // 구르기 속도 배율
    public float rollCoolDown = 0.4f;

    [Header("키")]
    public KeyCode weapon1Key = KeyCode.Alpha1;
    public KeyCode weapon2Key = KeyCode.Alpha2;
    public KeyCode skill1Key = KeyCode.E;
    //public KeyCode skill2Key = KeyCode.Q;
    public KeyCode skill3Key = KeyCode.Q;
    public KeyCode inventory = KeyCode.Tab;

    const KeyCode LeftKey = KeyCode.A;
    const KeyCode RightKey = KeyCode.D;
    const KeyCode JumpKey = KeyCode.Space;
    sbyte lastInputDirection = 1; // 마지막 입력 방향 (-1: 왼쪽, 1: 오른쪽)
    float currentSpeed;   // 현재 이동 속도
    public float currentHp;      // 현재 체력
    float maxArrowPower = 20f;   // 최대 화살 발사 힘
    float arrowPower = 0f;   // 화살 발사 힘
    bool isGrounded;    // 지면에 있는지 여부
    bool canRoll = true;
    [HideInInspector] public bool isRolling;    // 구르기 중인지 여부

    Rigidbody2D rb; // 플레이어의 Rigidbody2D 컴포넌트
    Collider2D col; // 플레이어의 Collider2D 컴포넌트

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void OnMouseDown()
    {
        print("Player Clicked");
    }

    private void Start()
    {
        currentHp = maxHp;
    }

    private void Update()
    {
        if (controlLocked)
            return;

        // 머지 스테이션 열려 있으면 모든 조작 금지
        if (GameStateManager.Current == GameState.MergeOpen)
            return;

        // 인벤토리 열려 있으면 공격만 금지
        bool inventoryOpen = GameStateManager.Current == GameState.InventoryOpen;

        // 공격 금지
        if (!inventoryOpen)
        {
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("Attack");
                MeleeAttack();
            }
        }

        // 인벤토리 토글
        if (Input.GetKeyDown(inventory))
        {
            UIManager.Instance.ToggleInventory();
        }

        // 활 충전/발사도 공격이므로 인벤토리 열렸을 때 금지
        if (!inventoryOpen)
        {
            if (Input.GetMouseButton(1))
            {
                arrowPower += Time.deltaTime * 30f;
                arrowPower = Mathf.Min(arrowPower, maxArrowPower);

                UIManager.Instance.UpdateChargeGauge(arrowPower, maxArrowPower);
            }
            else if (Input.GetMouseButtonUp(1))
            {
                LaunchArrow();
                arrowPower = 0f;
                UIManager.Instance.UpdateChargeGauge(0, maxArrowPower);
            }
        }

        // 구르기
        if (!inventoryOpen && Input.GetKeyDown(skill3Key))
        {
            animator.SetTrigger("Roll");
            Roll();
        }

        // 애니메이션 갱신 등 기존 로직 유지
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetBool("Walk", speed > 0.05f);
    }


    public void GetDamage(float damageAmount, Transform damageSource)
    {
        if (isRolling) return;

        UIManager.Instance.UpdatePlayerHP();

        currentHp -= damageAmount;
        Debug.Log("Player Get Damage: " + damageAmount + ", Current HP: " + currentHp);
        if (currentHp <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // 사망 처리
        Debug.Log("Player has died.");
    }

    void LaunchArrow()
    {
        ArrowController arrowScript = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        arrowPower = Mathf.Max(arrowPower, 3f); // 최소 발사 힘

        Vector2 arrowFireDirection = (Vector3.right * lastInputDirection) + (Vector3)rb.linearVelocity;

        float currentAngle = Mathf.Atan2(arrowFireDirection.y, arrowFireDirection.x) * Mathf.Rad2Deg;
        float minAngle, maxAngle;
        if (lastInputDirection > 0)
        {
            minAngle = -maxArrowAngle;
            maxAngle = maxArrowAngle;
        }
        else
        {
            if (currentAngle < 0) currentAngle += 360f;
            minAngle = 180f - maxArrowAngle;
            maxAngle = 180f + maxArrowAngle;
        }

        float clampedAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        float rad = clampedAngle * Mathf.Deg2Rad;
        arrowFireDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        float arrowSpeed = arrowPower + (rb.linearVelocity.magnitude / 3f);
        arrowScript.Shoot(arrowFireDirection, arrowSpeed);
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

    void Roll()
    {
        if (!canRoll || !isGrounded) return;
        StartCoroutine(RollCoroutine());
    }

    IEnumerator RollCoroutine()
    {
        isRolling = true;
        canRoll = false;

        float elapsedTime = 0f;
        while (elapsedTime < rollDuration)
        {
            rb.linearVelocity = new Vector2(lastInputDirection * maxSpeed * rollSpeedMultiplier, rb.linearVelocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isRolling = false;

        yield return new WaitForSeconds(rollDuration);
        canRoll = true;
    }

    private void FixedUpdate()
    {
        if (controlLocked)
            return;

        UpdateStates();

        if (!isRolling) MoveHandler();
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