using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ArrowController : MonoBehaviour
{
    public float damage = 10f;
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
    }

    private void Start()
    {
        // 🔥 박히지 않아도 자동 소멸
        StartCoroutine(ShrinkAndDestroy());
    }

    private void FixedUpdate()
    {
        if (!isStuck && rb.linearVelocity != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(rb.linearVelocity);
            Vector3 e = rot.eulerAngles;

            // 🔥 좌우 방향에 따라 앞/뒤만 회전
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
        isStuck = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        col.enabled = false;

        stuckTarget = collision.transform;
        stuckOffset = transform.position - stuckTarget.position;

        if (collision.TryGetComponent<IEnemyCombat>(out IEnemyCombat enemyCombat))
            enemyCombat.GetDamage(damage, transform);
    }

    private IEnumerator ShrinkAndDestroy()
    {
        // 🔥 박히든 안 박히든 waitTime 후 사라짐
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
