using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class BossBase : MonoBehaviour
{
    [Header("Base Stats")]
    public string bossName;
    public float maxHealth = 1000f;
    protected float currentHealth;
    public float moveSpeed = 3f;

    [Header("Phase Settings")]
    protected int currentPhase = 1;
    protected bool isPhaseTransitioning = false;
    protected bool isDead = false;

    [Header("UI Reference")]
    public Slider hpBar; // 보스 체력바 연결

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        if (hpBar != null) hpBar.maxValue = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead || isPhaseTransitioning) return;

        currentHealth -= damage;
        if (hpBar != null) hpBar.value = currentHealth;

        // 50% 이하일 때 2페이즈 체크
        if (currentPhase == 1 && currentHealth <= maxHealth * 0.5f)
        {
            StartCoroutine(PhaseTransitionRoutine());
        }

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    protected abstract IEnumerator PhaseTransitionRoutine();
    protected abstract void Die();
}