using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private float delay = 0.25f;
    private bool moved = false;

    void Update()
    {
        if (moved) return;

        delay -= Time.deltaTime;

        if (delay <= 0f)
        {
            MovePlayerToSpawn();
            moved = true;
        }
    }

    void MovePlayerToSpawn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("PlayerSpawner: Player를 찾을 수 없습니다.");
            return;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;   // 물리 끄기
        }

        player.transform.position = PlayerSpawnPoint.spawnPosition;

        if (rb != null)
        {
            rb.isKinematic = false;  // 다시 켜기
            rb.linearVelocity = Vector3.zero; // 튀는 현상 방지
        }

        Debug.Log("Player moved to spawn: " + PlayerSpawnPoint.spawnPosition);
    }
}
