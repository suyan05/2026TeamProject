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


    void Update()
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
                currentSpeed += acceleration * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            }
            else
            {
                currentSpeed -= deceleration * Time.deltaTime * 1.5f;
                currentSpeed = Mathf.Max(currentSpeed, 0f);
            }
        }
        else
        {
            currentSpeed -= deceleration * Time.deltaTime;
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
}
