using System.Collections;
using UnityEngine;

public class EnemyArrowController : MonoBehaviour
{
    [Header("대미지")]
    public float damage = 10f;

    [Header("소멸")]
    public float waitTimeBeforeShrink = 3.0f;
    public float shrinkDuration = 0.5f;

    private bool isStuck = false;

    private Rigidbody rb;
    private Collider col;

    private Transform stuckTarget;
    private Vector3 stuckOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // 빠른 투사체 충돌 누락 방지
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    private void Start()
    {
        // 박히지 않아도 자동 소멸
        StartCoroutine(ShrinkAndDestroy());
    }

    private void FixedUpdate()
    {
        if (!isStuck && rb.linearVelocity != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(rb.linearVelocity);
            Vector3 e = rot.eulerAngles;

            // 좌우 방향에 따라 앞/뒤만 회전
            e.y = (rb.linearVelocity.x >= 0) ? 0f : 180f;

            transform.rotation = Quaternion.Euler(e);
        }

        if (isStuck)
        {
            if (stuckTarget != null)
                transform.position = stuckTarget.position + stuckOffset;
            else
                Destroy(gameObject);
        }
    }

    public void Shoot(Vector3 direction, float force)
    {
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (isStuck) return;

        bool isPlayer = collision.gameObject == PlayerMovement.Instance.gameObject;

        // 플레이어가 구르는 중이면 무시
        if (isPlayer && PlayerMovement.Instance.isRolling)
            return;

        isStuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        col.enabled = false;

        stuckTarget = collision.transform;
        stuckOffset = transform.position - stuckTarget.position;

        // 플레이어 대미지
        if (isPlayer)
        {
            PlayerMovement.Instance.GetDamage(damage, transform);
        }
    }

    private IEnumerator ShrinkAndDestroy()
    {
        yield return new WaitForSeconds(waitTimeBeforeShrink);

        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsed / shrinkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
