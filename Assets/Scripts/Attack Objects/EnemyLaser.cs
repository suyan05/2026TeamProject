using System.Collections;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [Header("레이저")]
    public SpriteRenderer laserSpriteRenderer;

    [Header("조준")]
    public float aimingTime = 2;
    public float timeToFire = 0.3f;
    public Color aimingColor = Color.red;
    public Color lockOnColor = Color.red;

    [Header("공격")]
    public float damage = 0.3f;

    [Header("크기")]
    public float laserThickness = 0.2f;

    [Header("발사 이후 사라짐 / 색상")]
    public float shrinkTime = 0.1f;
    public Color startColor = Color.red;
    public Color endColor = Color.white;

    [Header("레이어")]
    public LayerMask collisionMask;

    const float maxRayDistance = 100f;
    bool hasOrigin = false;
    bool isFiring;  // 발사하고 사라지는 도중인지 여부

    Transform originTransform;
    Transform target;
    Vector2 targetPos;

    void Start()
    {
        transform.localScale = new Vector2(transform.localScale.x, laserThickness);
        laserSpriteRenderer.color = aimingColor;

        if (aimingTime > 0) StartCoroutine(Aiming());
        else StartCoroutine(Fire());
    }

    void Update()
    {
        if (hasOrigin)
        {
            if (originTransform != null) transform.position = originTransform.position;
            else if (!isFiring) Destroy(gameObject);
        }
    }

    public void SetOrigin(Transform target)
    {
        if (target != null) originTransform = target;
        hasOrigin = true;
    }

    public void SetTarget(Transform targetTransform)
    {
        if (targetTransform != null) target = targetTransform;
    }

    IEnumerator Aiming()
    {
        float elapsedTime = 0f;

        while (elapsedTime < aimingTime)
        {
            if (target != null) targetPos = target.position;
            LookPos(targetPos);
            RaycastResize();

            float currentThickness = Mathf.Lerp(0, laserThickness, elapsedTime / aimingTime);
            transform.localScale = new Vector2(transform.localScale.x, currentThickness);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(Fire());
    }

    IEnumerator Fire()
    {
        if (target != null) targetPos = target.position;
        LookPos(targetPos);

        if (timeToFire > 0)
        {
            laserSpriteRenderer.color = lockOnColor;
            float elapsedTime = 0f;
            while (elapsedTime < timeToFire)
            {
                RaycastResize();
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        RaycastResize();
        ShootLaser();
    }

    void ShootLaser()
    {
        laserSpriteRenderer.color = startColor;

        isFiring = true;

        Vector2 origin = transform.position;
        Vector2 direction = transform.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, collisionMask);

        if (hit)
        {
            if (hit.collider.gameObject == PlayerMovement.Instance.gameObject)
            {
                PlayerMovement.Instance.GetDamage(damage, transform);
            }
        }

        StartCoroutine(Co_ShrinkAndDestroy());
    }


    void Rotate(float angle)
    {
        float currentZ = transform.localEulerAngles.z;

        float newZ = currentZ + angle;

        transform.localRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            newZ
        );
    }

    IEnumerator Co_ShrinkAndDestroy()
    {
        float elapsedTime = 0f;
        float startThickness = transform.localScale.y;

        while (elapsedTime < shrinkTime)
        {
            float t = elapsedTime / shrinkTime;

            float currentThickness = Mathf.Lerp(startThickness, 0f, t);
            transform.localScale = new Vector2(transform.localScale.x, currentThickness);

            laserSpriteRenderer.color = Color.Lerp(startColor, endColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void LookPos(Vector2 pos)
    {
        targetPos = pos;
        Vector2 direction = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = rotation;
    }

    void RaycastResize()
    {
        Vector2 origin = transform.position;
        Vector2 direction = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, collisionMask);
        float distanceToSet = maxRayDistance;

        if (hit)
        {
            distanceToSet = hit.distance;
        }

        Vector3 newScale = transform.localScale;
        newScale.x = distanceToSet;
        transform.localScale = newScale;
    }
}
