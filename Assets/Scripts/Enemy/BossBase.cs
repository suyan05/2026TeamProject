using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class BossBase : MonoBehaviour
{
    [Header("Base Stats")]
    public string bossName;
    public float maxHealth = 1000f;
    public float currentHealth;
    public float moveSpeed = 3f;

    [Header("Phase & State Settings")]
    public int currentPhase = 1;
    public bool isPhaseTransitioning = false;
    public bool isGroggy = false;
    protected bool isDead = false;

    [Header("UI Reference")]
    public Slider hpBar;

    [Header("Attack Settings")]
    public float attackRange = 3f;
    public bool showAttackRange = true;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        if (hpBar != null)
        {
            hpBar.maxValue = maxHealth;
            hpBar.value = currentHealth;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        // [로그 1] 플레이어가 때리는 것 자체가 성공했는지 확인
        Debug.Log($"?? [타격 감지] {bossName}가 {damage}의 데미지를 받기 시작함!");

        if (isDead || isPhaseTransitioning) return;

        if (isGroggy)
            damage *= 1.3f;

        currentHealth -= damage;
        if (hpBar != null) hpBar.value = currentHealth;

        // [로그 2] 피가 반절 깎여서 페이즈가 넘어갈 때 멈추는지 확인
        if (currentPhase == 1 && currentHealth <= maxHealth * 0.5f)
        {
            Debug.Log("?? [추적] 페이즈 전환(PhaseTransitionRoutine) 실행 직전!");
            StartCoroutine(PhaseTransitionRoutine());
            Debug.Log("?? [추적] 페이즈 전환 실행 직후!");
        }

        // [로그 3] 보스가 아예 죽는 순간에 멈추는지 확인
        if (currentHealth <= 0 && !isDead)
        {
            Debug.Log("?? [추적] 사망 처리(Die) 실행 직전!");
            Die();
            Debug.Log("?? [추적] 사망 처리 실행 직후!");
        }
    }

    public bool IsPlayerInAttackRange(Transform player)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackRange) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (hpBar != null)
            hpBar.value = currentHealth;
    }


    protected abstract IEnumerator PhaseTransitionRoutine();
    protected abstract void Die();
}
