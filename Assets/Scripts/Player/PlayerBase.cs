using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool onPlatform; // 위/아래 이동 가능한 플랫폼 위에 있는지 체크

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 좌우 이동
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // 위/아래 플랫폼 이동 (예: 사다리, 엘리베이터)
        if (onPlatform)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            rb.velocity = new Vector2(rb.velocity.x, vertical * moveSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 땅에 닿았는지 체크
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // 위/아래 이동 가능한 플랫폼에 닿았는지 체크
        if (collision.gameObject.CompareTag("Platform"))
        {
            onPlatform = true;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            onPlatform = false;
        }
    }
}