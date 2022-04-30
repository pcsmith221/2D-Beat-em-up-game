using System.Collections;
using UnityEngine;

// Class that contains an array of enemies to spawn and defines how many and how often to spawn them. 
public class GoonSpawner : MonoBehaviour
{
    [Header("Random spawn timer")]
    [SerializeField] float minSpawnDelay = 1f;
    [SerializeField] float maxSpawnDelay = 5f;

    [Header("Choose spawner type (conditional must be stopped elsewhere")]
    [Tooltip("Enable if you would like the spawner to keep spawning until a condition is met")]
    [SerializeField] bool conditionalSpawner = false;

    [Tooltip("Enable if you would like the spawner to spawn a specific amount of randomly chosen enemy types")]
    [SerializeField] bool randomCountSpawner = false;

    [Tooltip("Enable if you would like the spawner to spawn a specific amount of enemies in array order")]
    [SerializeField] bool exactCountSpawner = false;

    [Header("Number/type of enemies")]
    [Tooltip("Not needed if conditional spawner; must be same as array size if exact spawner")]
    [SerializeField] int numberOfEnemiesToSpawn;

    [Tooltip("Enemy types spawner chooses from")]
    [SerializeField] GameObject[] enemyArray;

    bool spawn = true;

    public IEnumerator StartSpawning()
        // Begins spawning enemies from the array based on the type of spawner that it is
    {
        // NOTE: currently no scripts/battle events that stop conditional spawners from spawning. 
        if (conditionalSpawner)
        {
            while (spawn)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay));
                SpawnRandomEnemy();
            }
        }
        else if (randomCountSpawner)
        {
            for (int i = 1; i <= numberOfEnemiesToSpawn; i++)
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay));
                    SpawnRandomEnemy();
                }
        }
        else if (exactCountSpawner)
        {
            for (int i = 1; i <= numberOfEnemiesToSpawn; i++)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minSpawnDelay, maxSpawnDelay));
                SpawnEnemy(i);
            }
        }
        else
        {
            Debug.LogError(name + " spawner type not defined");
        }
    }



    public void StopSpawning()
        //must call somewhere if spawner is conditional
    {
        spawn = false;
    }



    private void SpawnEnemy(int index)
        // Spawns the enemy at the given enemy array index
    {
        Spawn(enemyArray[index]);
    }



    private void SpawnRandomEnemy()
        // Spawns an enemy from a random index in the enemy array. 
    {
        var enemyIndex = UnityEngine.Random.Range(0, enemyArray.Length);
        Spawn(enemyArray[enemyIndex]);
    }



    private void Spawn(GameObject enemy)
        // Instantiate the specified enemy 
    {
        GameObject newEnemy = Instantiate(enemy, transform.position, transform.rotation);

        //instantiates attacker as child of spawner for a cleaner hierarchy in the editor
        newEnemy.transform.parent = transform; 
    }



    public int GetNumberOfEnemiesToSpawn()
    {
        return numberOfEnemiesToSpawn;
    }



}
