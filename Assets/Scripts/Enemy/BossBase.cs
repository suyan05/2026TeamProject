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
        if (isDead || isPhaseTransitioning) return;

        if (isGroggy)
            damage *= 1.3f;

        currentHealth -= damage;
        if (hpBar != null) hpBar.value = currentHealth;

        if (currentPhase == 1 && currentHealth <= maxHealth * 0.5f)
            StartCoroutine(PhaseTransitionRoutine());

        if (currentHealth <= 0 && !isDead)
            Die();
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
