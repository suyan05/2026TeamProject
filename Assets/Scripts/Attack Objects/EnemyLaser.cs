using System.Collections;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [Header("레이저")]
    public MeshRenderer laserRenderer;

    [Header("조준")]
    public float aimingTime = 2;
    public float timeToFire = 0.3f;
    public Color aimingColor = Color.red;
    public Color lockOnColor = Color.red;

    [Header("공격")]
    public float damage = 0.3f;

    [Header("분산")]
    public float laserDispersion = 0f;

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
    bool isFiring;

    Transform originTransform;
    Transform target;
    Vector3 targetPos;

    void Start()
    {
        transform.localScale = new Vector3(0, laserThickness, laserThickness);
        laserRenderer.material.color = aimingColor;

        if (aimingTime > 0) StartCoroutine(Aiming());
        else StartCoroutine(Fire());
    }

    void Update()
    {
        if (hasOrigin)
        {
            if (originTransform != null)
                transform.position = originTransform.position;
            else if (!isFiring)
                Destroy(gameObject);
        }
    }

    public void SetOrigin(Transform t)
    {
        originTransform = t;
        hasOrigin = true;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    IEnumerator Aiming()
    {
        float elapsed = 0f;

        while (elapsed < aimingTime)
        {
            if (target != null) targetPos = target.position;

            LookPos(targetPos);
            RaycastResize();

            float currentThickness = Mathf.Lerp(0, laserThickness, elapsed / aimingTime);
            transform.localScale = new Vector3(transform.localScale.x, currentThickness, currentThickness);

            elapsed += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(Fire());
    }

    IEnumerator Fire()
    {
        if (target != null) targetPos = target.position;

        LookPos(targetPos);

        if (laserDispersion > 0)
        {
            float randomAngle = Random.Range(-laserDispersion / 2f, laserDispersion / 2f);
            transform.Rotate(0, randomAngle, 0);
        }

        if (timeToFire > 0)
        {
            laserRenderer.material.color = lockOnColor;
            float elapsed = 0f;

            while (elapsed < timeToFire)
            {
                RaycastResize();
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        RaycastResize();
        ShootLaser();
    }

    void ShootLaser()
    {
        laserRenderer.material.color = startColor;
        isFiring = true;

        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRayDistance, collisionMask))
        {
            if (hit.collider.gameObject == PlayerMovement.Instance.gameObject)
                PlayerMovement.Instance.GetDamage(damage, transform);
        }

        StartCoroutine(Co_ShrinkAndDestroy());
    }

    IEnumerator Co_ShrinkAndDestroy()
    {
        float elapsed = 0f;
        float startThickness = transform.localScale.y;

        while (elapsed < shrinkTime)
        {
            float t = elapsed / shrinkTime;

            float currentThickness = Mathf.Lerp(startThickness, 0f, t);
            transform.localScale = new Vector3(transform.localScale.x, currentThickness, currentThickness);

            laserRenderer.material.color = Color.Lerp(startColor, endColor, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void LookPos(Vector3 pos)
    {
        Vector3 dir = pos - transform.position;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = rot;
    }

    void RaycastResize()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        float distance = maxRayDistance;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxRayDistance, collisionMask))
            distance = hit.distance;

        Vector3 newScale = transform.localScale;
        newScale.x = distance;
        transform.localScale = newScale;
    }
}
