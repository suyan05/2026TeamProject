using System.Collections;
using UnityEngine;

public class EnemyArrowController : MonoBehaviour
{
    [Header("대미지")]
    public float damage = 10f;    // 화살이 가하는 대미지

    [Header("소멸")]
    public float waitTimeBeforeShrink = 3.0f;   // 충돌 후 사라지기 전 대기 시간 (초)
    public float shrinkDuration = 0.5f; // 작아지며 사라지는 데 걸리는 시간 (초)

    private bool isStuck = false;

    private Rigidbody2D rb;
    private Collider2D col;

    private Transform stuckTarget;
    private Vector3 stuckOffset;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        Destroy(gameObject, 30f);
    }

    private void FixedUpdate()
    {
        if (transform.position.y < -300f)
        {
            Destroy(gameObject);
            return;
        }

        if (!isStuck && rb.linearVelocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if (isStuck)
        {
            if (stuckTarget != null)
            {
                transform.position = stuckTarget.position + stuckOffset;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 인수 : 방향 - 힘
    /// </summary>
    public void Shoot(Vector2 direction, float force)
    {
        // 정규화된 방향으로 힘을 가하여 화살을 날려 보냅니다.
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isStuck) return;
        isStuck = true;

        // 추가 연산 중단
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        col.enabled = false;

        // 박힌 위치
        stuckTarget = collision.transform;
        stuckOffset = transform.position - stuckTarget.position;

        if (collision.gameObject == PlayerMovement.Instance.gameObject)
        {
            PlayerMovement.Instance.GetDamage(damage, transform);
        }

        // 사라지는 코루틴 시작
        StartCoroutine(ShrinkAndDestroy());
    }

    private IEnumerator ShrinkAndDestroy()
    {
        yield return new WaitForSeconds(waitTimeBeforeShrink);

        Vector3 originalScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}