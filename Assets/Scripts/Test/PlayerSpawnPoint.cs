using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public static Vector3 spawnPosition;

    void Awake()
    {
        spawnPosition = transform.position;
        Debug.Log("SpawnPoint Awake: " + spawnPosition);
    }


    void Start()
    {
        spawnPosition = transform.position;
    }
}
