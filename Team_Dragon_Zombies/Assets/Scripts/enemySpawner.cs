using System.Collections;
using UnityEngine;

public class enemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs; //set enemies to spawn from selected prefabs
    public int maxEnemies = 10; //max number of enemies that can exist (excluding static ones)
    public float spawnRange = 10f; //range from spawner where enemies can spawn
    public float raycastDelayMin = 0.1f; //min delay before spawning after raycast
    public float raycastDelayMax = 0.2f; //max delay before spawning after raycast

    private int currentEnemyCount = 0;

    void Start()
    {
        StartCoroutine(SpawnEnemiesRoutine());
    }

    IEnumerator SpawnEnemiesRoutine()
    {
        while (true)
        {
            if (currentEnemyCount < maxEnemies)
            {
                Vector3 randomSpawnPosition = GetRandomSpawnPosition();

                RaycastHit hit;
                Vector3 directionToPlayer = (gameManager.instance.player.transform.position - randomSpawnPosition).normalized;

                //if raycast hits player, don't spawn
                if (Physics.Raycast(randomSpawnPosition, directionToPlayer, out hit))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        Debug.DrawRay(randomSpawnPosition, directionToPlayer * 10f, Color.red, 2f);
                        yield return new WaitForSeconds(Random.Range(raycastDelayMin, raycastDelayMax));
                        continue;
                    }
                }

                //if the raycast hits an obstacle, spawn
                if (hit.transform != null && hit.transform.CompareTag("Obstacle"))
                {
                    Debug.DrawRay(randomSpawnPosition, directionToPlayer * 10f, Color.green, 2f); //if location is valid (behind obstacle) shoot green raycast

                    yield return new WaitForSeconds(Random.Range(raycastDelayMin, raycastDelayMax));

                    GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                    Instantiate(enemyPrefab, randomSpawnPosition, Quaternion.identity);
                    currentEnemyCount++;

                    Debug.Log("Spawned an enemy. Current enemy count: " + currentEnemyCount);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        float xOffset = Random.Range(-spawnRange, spawnRange);
        float zOffset = Random.Range(-spawnRange, spawnRange);

        return new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z + zOffset);
    }
}