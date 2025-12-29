using UnityEngine;

public class SpawnOnApproach : MonoBehaviour
{
    private Transform player;

    public GameObject[] objectsToSpawn;

    private float spawnDistance = 5f;
    public bool destroyAfterSpawn = true; 

    private bool hasSpawned = false;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning(" 'Player' not found");
        }
    }

    void Update()
    {
        if (hasSpawned || player == null || objectsToSpawn.Length == 0)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= spawnDistance)
        {
            int randomIndex = Random.Range(0, objectsToSpawn.Length);
            Instantiate(objectsToSpawn[randomIndex], transform.position, Quaternion.identity);

            hasSpawned = true;

            if (destroyAfterSpawn)
            {
                Destroy(gameObject);
            }
        }
    }
}
