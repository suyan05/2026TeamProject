using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDummy : MonoBehaviour, IEnemyCombat
{
    [Header("HP")]
    public float maxHP = 50;    // 최대 HP

    [Header("Death Effect")]
    public float deathForce = 10f; // 죽을 때 날아가는 힘의 세기
    public float rotationSpeed = 500f; // 회전하며 날아가는 속도

    private float currentHP;    // 현재 HP
    Vector3 startPos;
    bool isDead;

    private Collider2D col;
    private Rigidbody2D rb;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        currentHP = maxHP;
        startPos = transform.position;
    }

    private void Update()
    {
        if (!isDead) transform.position = startPos;
    }

    public void GetDamage(float damage, Transform attacker)
    {
        currentHP -= damage;
        if (currentHP <= 0) Dead(attacker.position);
    }

    void Dead(Vector3 attackerPosition)
    {
        StartCoroutine(ShrinkAndDestroy(attackerPosition));

        Vector2 pushDirection = (transform.position - attackerPosition).normalized;
        pushDirection.y = Mathf.Abs(pushDirection.y);
        rb.linearVelocity = pushDirection * deathForce;
        rb.AddTorque(rotationSpeed, ForceMode2D.Impulse);
        isDead = true;

        col.enabled = false;
    }

    private IEnumerator ShrinkAndDestroy(Vector3 attackerPosition)
    {
        Vector3 originalScale = transform.localScale;
        float elapsedTime = 0f;
        float shunkDuration = 0.5f; // 축소되는 시간

        while (elapsedTime < shunkDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, elapsedTime / shunkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
