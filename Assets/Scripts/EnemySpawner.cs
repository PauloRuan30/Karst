using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] GameObject enemyMeleePrefab;
    [SerializeField] GameObject enemyCasterPrefab;
    [SerializeField] GameObject enemySpawnerPrefab;
    [SerializeField] GameObject enemySkeletonPrefab;

    [Header("Time")]
    [SerializeField] float minimumSpawnTime;
    [SerializeField] float maximumSpawnTime;
    float timeUntilNextSpawn;

    private GameObject[] enemyPrefabs;

    private void Awake()
    {
        enemyPrefabs = new GameObject[] { enemyMeleePrefab, enemyCasterPrefab, enemySpawnerPrefab, enemySkeletonPrefab };
        SetTimeUntilSpawn();
    }

    void Update()
    {
        timeUntilNextSpawn -= Time.deltaTime;

        if (timeUntilNextSpawn <= 0 && CountEnemiesController.canSpawnEnemy)
        {
            SpawnRandomEnemy();
            SetTimeUntilSpawn();
        }
    }

    private void SetTimeUntilSpawn()
    {
        timeUntilNextSpawn = Random.Range(minimumSpawnTime, maximumSpawnTime);
    }

    private void SpawnRandomEnemy()
    {
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject randomEnemyPrefab = enemyPrefabs[randomIndex];
        Instantiate(randomEnemyPrefab, transform.position, transform.rotation);
    }
}
