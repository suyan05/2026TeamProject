using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool onPlatform; // ��/�Ʒ� �̵� ������ �÷��� ���� �ִ��� üũ

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // �¿� �̵�
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // ����
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // ��/�Ʒ� �÷��� �̵� (��: ��ٸ�, ����������)
        if (onPlatform)
        {
            float vertical = Input.GetAxisRaw("Vertical");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, vertical * moveSpeed);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ���� ��Ҵ��� üũ
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        // ��/�Ʒ� �̵� ������ �÷����� ��Ҵ��� üũ
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