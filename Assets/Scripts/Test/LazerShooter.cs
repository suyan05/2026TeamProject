using System.Collections;
using UnityEngine;

public class LazerShooter : MonoBehaviour
{
    [Header("발사 주기")]
    public float shootInterval = 3f;

    [Header("레이저 설정")]
    public EnemyLaser laserPrefab;
    public Transform target;

    private void Start()
    {
        StartCoroutine(laserFire());
    }

    IEnumerator laserFire()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootInterval);
            EnemyLaser laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
            laser.SetTarget(target);
            laser.SetOrigin(transform);
        }
    }
}
