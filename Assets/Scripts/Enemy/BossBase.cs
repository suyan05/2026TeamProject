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
    public bool isGroggy = false; // 그로기 상태 관리
    protected bool isDead = false;

    [Header("UI Reference")]
    public Slider hpBar; // 보스 체력바 슬라이더

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

        // 기획서 내용: 그로기 동안 보스가 받는 피해 30% 증가
        if (isGroggy)
        {
            damage *= 1.3f;
        }

        currentHealth -= damage;
        if (hpBar != null) hpBar.value = currentHealth;

        // 체력이 50% 이하로 떨어지면 2페이즈 진입 연출 트리거
        if (currentPhase == 1 && currentHealth <= maxHealth * 0.5f)
        {
            StartCoroutine(PhaseTransitionRoutine());
        }

        if (currentHealth <= 0 && !isDead) Die();
    }

    // 기획서 내용: 덩굴 속박 흡수 기믹용 체력 회복 함수
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (hpBar != null) hpBar.value = currentHealth;
        Debug.Log($"{bossName} 체력 회복 (+{amount}). 현재 체력: {currentHealth}");
    }

    protected abstract IEnumerator PhaseTransitionRoutine();
    protected abstract void Die();
}