using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("플레이어 기본 스탯")]
    public float baseMaxHp = 100f;
    public float baseDamage = 20f;
    public float baseAttackSpeed = 1f;

    float bonusMaxHp = 0f;
    float bonusDamage = 0f;
    float bonusAttackSpeed = 0f;

    float weaponAttackPower = 0f;
    float weaponAttackSpeed = 0f;

    public float MaxHp => baseMaxHp + bonusMaxHp;
    public float Damage => baseDamage + bonusDamage + weaponAttackPower;
    public float AttackSpeed => baseAttackSpeed + bonusAttackSpeed + weaponAttackSpeed;

    public float currentHp;

    [Header("플레이어 움직임 제한")]
    public bool controlLocked = false;

    [Header("플레이어 이동")]
    public float maxSpeed = 5f;
    public float jumpForce = 7f;

    [Header("가속/감속")]
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("애니메이션")]
    public Animator animator;

    [Header("레이어 마스크")]
    public LayerMask groundLayerMask;

    [Header("임시용 (화살)")]
    public ArrowController arrowPrefab;
    public float maxArrowAngle = 30f;
    public Transform firePoint;

    [Header("근접")]
    public float damage = 20f;
    public Vector2 hitboxOffset = Vector2.zero;
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f);
    public LayerMask enemyLayer;

    [Header("구르기")]
    public float rollDuration = 0.5f;
    public float rollSpeedMultiplier = 1.5f;
    public float rollCoolDown = 0.4f;

    sbyte lastInputDirection = 1;
    float currentSpeed;
    float maxArrowPower = 20f;
    float arrowPower = 0f;
    bool isGrounded;
    bool canRoll = true;
    public bool isRolling;

    Rigidbody2D rb;
    Collider2D col;

    [Header("플레이어 장착용 프리팹")]
    public GameObject equipPrefab;
    public string currentWeaponName = "None";

    [Header("장비 장착 위치")]
    public Transform weaponHolder;
    private GameObject equippedWeaponObject;

    KeyBindingManager kb;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        kb = KeyBindingManager.Instance;

        currentHp = MaxHp;

        UIManager.Instance.UpdatePlayerStatsUI(MaxHp, Damage, AttackSpeed, weaponAttackPower, weaponAttackSpeed);
        UIManager.Instance.UpdatePlayerHP();
    }

    private void Update()
    {
        if (controlLocked)
            return;

        if (GameStateManager.Current == GameState.MergeOpen)
            return;

        bool inventoryOpen = GameStateManager.Current == GameState.InventoryOpen;

        // 공격 입력
        if (!inventoryOpen && Weapon1Pressed())
        {
            if (animator != null) animator.SetTrigger("Attack");
            MeleeAttack();
        }

        // 인벤토리
        if (Input.GetKeyDown(kb.inventory))
        {
            UIManager.Instance.ToggleInventory();
        }

        // 활 충전/발사
        if (!inventoryOpen)
        {
            if (Weapon2Hold())
            {
                arrowPower += Time.deltaTime * 30f;
                arrowPower = Mathf.Min(arrowPower, maxArrowPower);
                UIManager.Instance.UpdateChargeGauge(arrowPower, maxArrowPower);
            }
            else if (Weapon2Release())
            {
                LaunchArrow();
                arrowPower = 0f;
                UIManager.Instance.UpdateChargeGauge(0, maxArrowPower);
            }
        }

        // 구르기
        if (!inventoryOpen && Input.GetKeyDown(kb.rollKey))
        {
            if (animator != null) animator.SetTrigger("Roll");
            Roll();
        }

        float speed = Mathf.Abs(rb.linearVelocity.x);
        if (animator != null) animator.SetBool("Walk", speed > 0.05f);
    }

    // 무기 1 입력 (근접)
    bool Weapon1Pressed()
    {
        if (kb.weapon1IsMouse)
            return Input.GetMouseButtonDown((int)kb.weapon1Mouse);
        else
            return Input.GetKeyDown(kb.weapon1Key);
    }

    // 무기 2 입력 (활 충전)
    bool Weapon2Hold()
    {
        if (kb.weapon2IsMouse)
            return Input.GetMouseButton((int)kb.weapon2Mouse);
        else
            return Input.GetKey(kb.weapon2Key);
    }

    bool Weapon2Release()
    {
        if (kb.weapon2IsMouse)
            return Input.GetMouseButtonUp((int)kb.weapon2Mouse);
        else
            return Input.GetKeyUp(kb.weapon2Key);
    }

    public void RecalculateStats(List<ItemInstance> items)
    {
        float oldMaxHp = MaxHp;

        bonusMaxHp = 0;
        bonusDamage = 0;
        bonusAttackSpeed = 0;

        foreach (var item in items)
        {
            if (item.data.itemType != ItemType.Item)
                continue;

            bonusMaxHp += item.data.bonusMaxHp;
            bonusDamage += item.data.bonusBaseDamage;
            bonusAttackSpeed += item.data.bonusAttackSpeed;
        }

        float newMaxHp = MaxHp;
        float addedHp = newMaxHp - oldMaxHp;

        if (addedHp > 0)
            currentHp = Mathf.Min(currentHp + addedHp, newMaxHp);
        else
            currentHp = Mathf.Min(currentHp, newMaxHp);

        UIManager.Instance.UpdatePlayerStatsUI(MaxHp, Damage, AttackSpeed, weaponAttackPower, weaponAttackSpeed);
        UIManager.Instance.UpdatePlayerHP();
    }

    public void EquipWeapon(ItemData data)
    {
        if (equippedWeaponObject != null)
            Destroy(equippedWeaponObject);

        if (data == null || data.itemType != ItemType.Weapon)
        {
            currentWeaponName = "None";
            UIManager.Instance.UpdateEquippedWeaponUI(currentWeaponName);

            weaponAttackPower = 0f;
            weaponAttackSpeed = 0f;

            UIManager.Instance.UpdatePlayerStatsUI(MaxHp, Damage, AttackSpeed, 0, 0);
            return;
        }

        GameObject prefab = data.equipPrefab != null ? data.equipPrefab : data.worldPrefab;
        if (prefab == null) return;

        equippedWeaponObject = Instantiate(prefab, weaponHolder);
        equippedWeaponObject.transform.localPosition = Vector3.zero;
        equippedWeaponObject.transform.localRotation = Quaternion.identity;

        weaponAttackPower = data.weaponAttackPower;
        weaponAttackSpeed = data.weaponAttackSpeed;
        currentWeaponName = data.weaponType.ToString();

        UIManager.Instance.UpdateEquippedWeaponUI(currentWeaponName);
        UIManager.Instance.UpdatePlayerStatsUI(MaxHp, Damage, AttackSpeed, weaponAttackPower, weaponAttackSpeed);
    }

    public void GetDamage(float damageAmount, Transform damageSource)
    {
        if (isRolling) return;

        currentHp -= damageAmount;
        UIManager.Instance.UpdatePlayerHP();

        if (currentHp <= 0f)
            Die();
    }

    void Die()
    {
        Debug.Log("Player has died.");
    }

    void LaunchArrow()
    {
        ArrowController arrowScript = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        arrowPower = Mathf.Max(arrowPower, 3f);

        Vector2 arrowFireDirection = (Vector3.right * lastInputDirection) + (Vector3)rb.linearVelocity;

        float currentAngle = Mathf.Atan2(arrowFireDirection.y, arrowFireDirection.x) * Mathf.Rad2Deg;
        float minAngle, maxAngle;

        if (lastInputDirection > 0)
        {
            minAngle = -maxArrowAngle;
            maxAngle = maxArrowAngle;
        }
        else
        {
            if (currentAngle < 0) currentAngle += 360f;
            minAngle = 180f - maxArrowAngle;
            maxAngle = 180f + maxArrowAngle;
        }

        float clampedAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        float rad = clampedAngle * Mathf.Deg2Rad;
        arrowFireDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        float arrowSpeed = arrowPower + (rb.linearVelocity.magnitude / 3f);
        arrowScript.Shoot(arrowFireDirection, arrowSpeed);
    }

    void MeleeAttack()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * lastInputDirection, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(worldCenter, hitboxSize, 0f, enemyLayer);

        foreach (Collider2D targetCollider in hitTargets)
        {
            if (targetCollider.TryGetComponent<IEnemyCombat>(out IEnemyCombat enemyCombat))
            {
                enemyCombat.GetDamage(Damage, transform);
            }
        }
    }

    void Roll()
    {
        if (!canRoll || !isGrounded) return;
        StartCoroutine(RollCoroutine());
    }

    IEnumerator RollCoroutine()
    {
        isRolling = true;
        canRoll = false;

        float elapsedTime = 0f;
        while (elapsedTime < rollDuration)
        {
            rb.linearVelocity = new Vector2(lastInputDirection * maxSpeed * rollSpeedMultiplier, rb.linearVelocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isRolling = false;

        yield return new WaitForSeconds(rollCoolDown);
        canRoll = true;
    }

    private void FixedUpdate()
    {
        if (controlLocked)
            return;

        UpdateStates();

        if (!isRolling) MoveHandler();
        JumpHandler();
        RotationHandler();
    }

    void MoveHandler()
    {
        if (TryGetHorizontalInput(out sbyte horizontal))
        {
            if (!(lastInputDirection != horizontal && currentSpeed > maxSpeed * 0.1f))
            {
                lastInputDirection = horizontal;
                currentSpeed += acceleration * Time.fixedDeltaTime;
                currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            }
            else
            {
                currentSpeed -= deceleration * Time.fixedDeltaTime * 1.5f;
                currentSpeed = Mathf.Max(currentSpeed, 0f);
            }
        }
        else
        {
            currentSpeed -= deceleration * Time.fixedDeltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0f);
        }

        rb.linearVelocity = new Vector2(currentSpeed * lastInputDirection, rb.linearVelocity.y);
    }

    void JumpHandler()
    {
        if (isGrounded && Input.GetKey(kb.jumpKey))
        {
            Vector2 jumpVector = Vector2.up * jumpForce;
            jumpVector.x = rb.linearVelocity.x;
            rb.linearVelocity = jumpVector;
        }
    }

    bool TryGetHorizontalInput(out sbyte horizontal)
    {
        bool left = Input.GetKey(kb.leftKey);
        bool right = Input.GetKey(kb.rightKey);

        if (left && right)
        {
            horizontal = 0;
            return false;
        }
        else if (left)
        {
            horizontal = -1;
            return true;
        }
        else if (right)
        {
            horizontal = 1;
            return true;
        }
        else
        {
            horizontal = 0;
            return false;
        }
    }

    void RotationHandler()
    {
        float targetYAngle = (lastInputDirection == 1) ? 0.01f : 179.99f;
        Quaternion targetRotation = Quaternion.Euler(0, targetYAngle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if ((lastInputDirection > 0 && contact.normal.x < -0.5f) ||
                    (lastInputDirection < 0 && contact.normal.x > 0.5f))
                {
                    currentSpeed = 0f;
                }
            }
        }
    }

    private void UpdateStates()
    {
        isGrounded = CheckIsGrounded();
    }

    bool CheckIsGrounded()
    {
        Vector2 rayStart = new Vector2(col.bounds.center.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 0.05f, groundLayerMask);
        return hit.collider != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * lastInputDirection, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));
    }
}
