using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("기본 스탯")]
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

    [Header("움직임 제한")]
    public bool controlLocked = false;

    [Header("이동")]
    public float maxSpeed = 5f;
    public float jumpForce = 7f;

    [Header("가속/감속")]
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("애니메이션")]
    public Animator animator;

    [Header("레이어 마스크")]
    public LayerMask groundLayerMask;

    [Header("활")]
    public ArrowController arrowPrefab;
    public float maxArrowAngle = 30f;
    public Transform firePoint;

    [Header("근접")]
    public float damage = 20f;
    public Vector3 hitboxOffset = Vector3.zero;
    public Vector3 hitboxSize = new Vector3(1.0f, 1.0f, 1.0f);
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

    Rigidbody rb;
    Collider col;

    [Header("장비")]
    public GameObject equipPrefab;
    public string currentWeaponName = "None";
    public Transform weaponHolder;
    private GameObject equippedWeaponObject;

    KeyBindingManager kb;

    int jumpCount = 0;
    public int maxJumpCount = 2;

    bool isMeleeAttacking = false;
    bool isBowCharging = false;

    sbyte bowChargeDirection = 1;

    bool isHit = false;
    public float hitStunDuration = 0.3f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
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

        if (Input.GetKeyDown(kb.inventory))
            UIManager.Instance.ToggleInventory();

        if (controlLocked || isHit)
            return;

        if (GameStateManager.Current == GameState.MergeOpen)
            return;

        bool inventoryOpen = GameStateManager.Current == GameState.InventoryOpen;

        // 근접 공격
        if (!inventoryOpen && Weapon1Pressed() && !isBowCharging)
        {
            animator?.SetTrigger("Attack");
            isMeleeAttacking = true;
            MeleeAttack();
            StartCoroutine(EndMeleeAttack());
        }

        // 활 차징
        if (!inventoryOpen)
        {
            if (Weapon2Hold() && !isMeleeAttacking)
            {
                if (!isBowCharging)
                    bowChargeDirection = lastInputDirection;

                isBowCharging = true;
                animator?.SetBool("BowCharge", true);

                arrowPower += Time.deltaTime * 30f;
                arrowPower = Mathf.Min(arrowPower, maxArrowPower);
                UIManager.Instance.UpdateChargeGauge(arrowPower, maxArrowPower);
            }
            else if (Weapon2Release() && isBowCharging)
            {
                animator?.SetTrigger("BowShoot");
                animator?.SetBool("BowCharge", false);
                animator?.SetBool("BowMoveForward", false);
                animator?.SetBool("BowMoveBackward", false);

                LaunchArrow();
                arrowPower = 0f;
                isBowCharging = false;

                lastInputDirection = bowChargeDirection;

                UIManager.Instance.UpdateChargeGauge(0, maxArrowPower);
            }
        }

        // 구르기
        if (!inventoryOpen && Input.GetKeyDown(kb.rollKey))
        {
            animator?.SetTrigger("Roll");
            Roll();
        }

        float speed = Mathf.Abs(rb.linearVelocity.x);
        if (!isBowCharging)
            animator?.SetBool("Walk", speed > 0.05f);

        if (controlLocked || isHit)
            return;

        UpdateStates();

        if (isBowCharging)
            BowMoveHandler();
        else if (!isRolling)
            MoveHandler();

        if (!isBowCharging)
            RotationHandler();

        JumpHandler();
    }

    bool Weapon1Pressed()
    {
        return kb.weapon1IsMouse ?
            Input.GetMouseButtonDown((int)kb.weapon1Mouse) :
            Input.GetKeyDown(kb.weapon1Key);
    }

    bool Weapon2Hold()
    {
        return kb.weapon2IsMouse ?
            Input.GetMouseButton((int)kb.weapon2Mouse) :
            Input.GetKey(kb.weapon2Key);
    }

    bool Weapon2Release()
    {
        return kb.weapon2IsMouse ?
            Input.GetMouseButtonUp((int)kb.weapon2Mouse) :
            Input.GetKeyUp(kb.weapon2Key);
    }

    private void FixedUpdate()
    {
        
    }

    void BowMoveHandler()
    {
        sbyte horizontal = 0;

        if (TryGetHorizontalInput(out horizontal))
        {
            float chargeSlow = Mathf.Lerp(1f, 0.4f, arrowPower / maxArrowPower);
            float targetSpeed = horizontal * maxSpeed * chargeSlow;

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector3(currentSpeed, rb.linearVelocity.y, 0);
    }

    void MoveHandler()
    {
        if (TryGetHorizontalInput(out sbyte horizontal))
        {
            float targetSpeed = horizontal * maxSpeed;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            lastInputDirection = horizontal;
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector3(currentSpeed, rb.linearVelocity.y, 0);
    }

    void JumpHandler()
    {
        // 점프 딜레이 해결: 입력 즉시 반응하도록 변경
        if (Input.GetKeyDown(kb.jumpKey))
        {
            if (jumpCount < maxJumpCount)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                jumpCount++;

                if (jumpCount == 1)
                    animator?.SetTrigger("Jump");
                else
                    animator?.SetTrigger("DoubleJump");
            }
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
        float targetYAngle = (lastInputDirection == 1) ? 0f : 180f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetYAngle, 0), Time.deltaTime * 10f);
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
            Vector3 rollVel = rb.linearVelocity;
            rollVel.x = lastInputDirection * maxSpeed * rollSpeedMultiplier;
            rb.linearVelocity = rollVel;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        isRolling = false;

        yield return new WaitForSeconds(rollCoolDown);
        canRoll = true;
    }

    public void GetDamage(float damageAmount, Transform damageSource)
    {
        if (isRolling || isHit) return;

        currentHp -= damageAmount;
        UIManager.Instance.UpdatePlayerHP();

        // 넉백 추가
        Vector3 knockbackDir = (transform.position - damageSource.position).normalized;
        rb.AddForce(knockbackDir * 5f, ForceMode.Impulse);

        StartCoroutine(HitStun());

        if (currentHp <= 0f)
            Die();
    }

    IEnumerator HitStun()
    {
        isHit = true;

        isMeleeAttacking = false;
        isBowCharging = false;

        animator?.SetTrigger("Hit");
        animator?.SetBool("BowCharge", false);
        animator?.SetBool("BowMoveForward", false);
        animator?.SetBool("BowMoveBackward", false);

        yield return new WaitForSeconds(hitStunDuration);

        isHit = false;
    }

    void Die()
    {
        Debug.Log("Player has died.");
    }

    void LaunchArrow()
    {
        ArrowController arrowScript = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);

        arrowPower = Mathf.Max(arrowPower, 3f);

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Vector3 rawDirection = (worldMousePos - firePoint.position);
        rawDirection.z = 0;
        rawDirection.Normalize();

        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;

        float minAngle = -maxArrowAngle;
        float maxAngle = maxArrowAngle;

        if (lastInputDirection < 0)
        {
            if (angle > 0) angle -= 180f;
            else angle += 180f;

            minAngle = 180f - maxArrowAngle;
            maxAngle = 180f + maxArrowAngle;
        }

        float clampedAngle = Mathf.Clamp(angle, minAngle, maxAngle);

        Vector3 finalDirection = new Vector3(
            Mathf.Cos(clampedAngle * Mathf.Deg2Rad),
            Mathf.Sin(clampedAngle * Mathf.Deg2Rad),
            0
        );

        float arrowSpeed = arrowPower + (rb.linearVelocity.magnitude * 0.1f);

        arrowScript.Shoot(finalDirection, arrowSpeed);
    }

    void MeleeAttack()
    {
        Vector3 localAdjustedOffset = new Vector3(hitboxOffset.x * lastInputDirection, hitboxOffset.y, hitboxOffset.z);
        Vector3 worldCenter = transform.position + localAdjustedOffset;

        
        Collider[] hitTargets = Physics.OverlapBox(worldCenter, hitboxSize * 0.5f, Quaternion.identity, enemyLayer);

        foreach (Collider targetCollider in hitTargets)
        {
            
            if (targetCollider.TryGetComponent<IEnemyCombat>(out IEnemyCombat enemyCombat))
            {
                enemyCombat.GetDamage(Damage, transform);
                Debug.Log($"[일반몹 타격] {targetCollider.name}에게 {Damage}의 피해!");
            }
            
            else if (targetCollider.TryGetComponent<BossBase>(out BossBase boss))
            {
                boss.TakeDamage(Damage); 
                Debug.Log($"[보스몹 타격] {targetCollider.name}에게 {Damage}의 피해!");
            }
        }
    }

    IEnumerator EndMeleeAttack()
    {
        yield return new WaitForSeconds(0.3f);
        isMeleeAttacking = false;
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

        currentHp = addedHp > 0 ?
            Mathf.Min(currentHp + addedHp, newMaxHp) :
            Mathf.Min(currentHp, newMaxHp);

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

    private void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
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
        bool wasGrounded = isGrounded;
        isGrounded = CheckIsGrounded();

        if (!wasGrounded && isGrounded)
        {
            jumpCount = 0;
            animator?.SetTrigger("Land");
        }
    }

    bool CheckIsGrounded()
    {
        Vector3 boxCenter = new Vector3(col.bounds.center.x, col.bounds.min.y - 0.05f, col.bounds.center.z);
        Vector3 boxSize = new Vector3(col.bounds.size.x * 0.9f, 0.1f, col.bounds.size.z * 0.9f);

        return Physics.CheckBox(boxCenter, boxSize * 0.5f, Quaternion.identity, groundLayerMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 hitboxLocalAdjustedOffset = new Vector3(hitboxOffset.x * lastInputDirection, hitboxOffset.y, hitboxOffset.z);
        Vector3 hitboxGizmoCenter = transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);
    }
}
