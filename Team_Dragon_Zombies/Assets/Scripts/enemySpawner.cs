using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    // Spawn settings
    [SerializeField] private GameObject[] enemyPrefabs;  //enemy types to spawn
    [SerializeField] private Transform[] spawnPoints;    //spawn area
    [SerializeField] private float spawnCooldown = 3f;    //spawn cooldown
    [SerializeField] private int maxEnemiesOnMap = 10;    //enemy cap on map
    [SerializeField] private float spawnProximity = 10f;  //proximity, if player in proximity then dont spawn

    private int currentEnemyCount = 0;
    private GameObject player;
    private bool canSpawn = true;

    void Start()
    {
        player = gameManager.instance.player;
    }

    void Update()
    {
        if (canSpawn && currentEnemyCount < maxEnemiesOnMap)
        {
            //find potential spawn points
            Vector3 spawnPosition = GetValidSpawnPoint();
            if (spawnPosition != Vector3.zero)
            {
                SpawnEnemy(spawnPosition);
            }
        }
    }

    private Vector3 GetValidSpawnPoint()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (Vector3.Distance(spawnPoint.position, player.transform.position) > spawnProximity)
            {
                return spawnPoint.position;
            }
        }
        return Vector3.zero;
    }

    private void SpawnEnemy(Vector3 spawnPosition)
    {
        GameObject enemyToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);

        currentEnemyCount++;

        StartCoroutine(SpawnCooldown());
    }

    private IEnumerator SpawnCooldown()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }

    public void OnEnemyDestroyed()
    {
        currentEnemyCount--;
    }
}
