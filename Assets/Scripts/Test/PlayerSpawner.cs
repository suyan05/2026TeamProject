using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    private float delay = 0.25f;
    private bool moved = false;

    void Update()
    {
        if (moved) return;

        delay -= Time.deltaTime;
        if (delay > 0f) return;

        TryMovePlayer();
    }

    void TryMovePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("PlayerSpawner: Player를 찾을 수 없습니다.");
            return;
        }

        // 스폰포인트가 아직 준비 안 됨
        if (PlayerSpawnPoint.spawnPosition == Vector3.zero)
        {
            Debug.Log("SpawnPoint not ready, retrying...");
            delay = 0.05f; // 0.05초 후 다시 시도
            return;
        }

        StartCoroutine(MovePlayerSafely(player));
    }

    System.Collections.IEnumerator MovePlayerSafely(GameObject player)
    {
        moved = true;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 위치 이동
        player.transform.position = PlayerSpawnPoint.spawnPosition;
        Debug.Log("Player moved to spawn: " + PlayerSpawnPoint.spawnPosition);

        // 1프레임 기다렸다가 물리 다시 켜기
        yield return null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
    }
}
