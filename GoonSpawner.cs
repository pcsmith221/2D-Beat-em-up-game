using System.Collections;
using UnityEngine;

public class GoonSpawner : MonoBehaviour
{
    [SerializeField] float minSpawnDelay = 1f;
    [SerializeField] float maxSpawnDelay = 5f;

    [Tooltip("Enemy types spawner chooses from")]
    [SerializeField] GameObject[] enemyArray;

    [Tooltip("Enable if you would like the spawner to spawn until a condition is met")]
    [SerializeField] bool conditionalSpawner = false;

    [Tooltip("Not needed if conditional spawner")]
    public int numberOfEnemiesToSpawn;

    bool battleStarted = false;
    bool spawn = true;

    public IEnumerator ActivateSpawner()
    {
        if (conditionalSpawner)
        {
            while (spawn)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay));
                SpawnEnemy();
            }
        }
        else
        {
            for (int i = 1; i <= numberOfEnemiesToSpawn; i++)
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay));
                    SpawnEnemy();
                }
        }
    }

    public void StartBattle()
    {
        battleStarted = true;
    }

    public void StopSpawning() //must call somewhere if spawner is conditional
    {
        spawn = false;
    }

    private void SpawnEnemy()
    {
        var enemyIndex = UnityEngine.Random.Range(0, enemyArray.Length);
        Spawn(enemyArray[enemyIndex]);
    }

    private void Spawn(GameObject enemy)
    {
        GameObject newEnemy = Instantiate(enemy, transform.position, transform.rotation) as GameObject;
        newEnemy.transform.parent = transform; //instantiates attacker as child of spawner
    }
}
