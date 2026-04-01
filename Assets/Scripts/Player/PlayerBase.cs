using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool onPlatform; // 플랫폼 위에 있는지 여부

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 수평 이동
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // 플랫폼 위에서 수직 이동
        if (onPlatform)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * moveSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 지장 확인
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // 플랫폼 위에 있는지 확인
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