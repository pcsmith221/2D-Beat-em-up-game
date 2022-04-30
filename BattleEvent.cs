using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Mathematics;

// This class adds the functionality for locking the camera, adding new temporary colliders, and activating enemy spawners when the player crosses a given invisible boundary.
// The camera unlocks and the colliders are removed once all enemies are defeated. 
public class BattleEvent : MonoBehaviour
{
    [Tooltip("Place spawners in scene first then drag to this field")]
    [SerializeField] GoonSpawner[] goonSpawners;

    [Tooltip("Boundaries the player cannot cross during battles")]
    //[SerializeField] Collider2D[] playerColliders;
    [SerializeField] GameObject playerColliders;

    // Cached reference
    CinemachineStateDrivenCamera virtualCamera;

    // State variables
    int totalNumberOfEnemiesToDefeat = 0;
    bool battleStarted = false;



    private void Awake()
    {
        // Ensures battle colliders disabled by default
        SetBattleColliders(false);

        // Get reference to cinemachine camera controller to disable when battle event starts
        virtualCamera = FindObjectOfType<CinemachineStateDrivenCamera>();
    }



    void Update()
    {
        if (battleStarted == true)
        {
            CheckIfAllEnemiesDefeated();
        }
    }



    private void SetBattleColliders(bool isEnabled)
        // enable or disable battle colliders
    {
        foreach (var playerCollider in playerColliders.GetComponentsInChildren<Collider2D>())
        {
            playerCollider.enabled = isEnabled;
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
        // Starts battle event and enables spawners only if it has not been started already 
    {
        if (battleStarted == false)
        {
            Debug.Log("Battle event statrted");
            SetBattleColliders(true);

            var players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                player.SetIsInCombat(true);
            }

            // prevent camera from moving by disabling it
            virtualCamera.enabled = false;

            battleStarted = true;
            EnableSpawners();
        }
    }



    private void EnableSpawners()
        // Enables spawners and adds their enemy count to the total 
    {
        foreach (GoonSpawner spawner in goonSpawners)
        {
            spawner.enabled = true;
            totalNumberOfEnemiesToDefeat += spawner.GetNumberOfEnemiesToSpawn();
            StartCoroutine(spawner.StartSpawning());
        }
    }



    private void CheckIfAllEnemiesDefeated()
        // Reset camera and level boundaries once all enemies defeated
    {
        if (totalNumberOfEnemiesToDefeat <= 0)
        {
            Debug.Log("Battle Event Ended");
            virtualCamera.enabled = true;
            SetBattleColliders(false);

            var players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                player.SetIsInCombat(false);
            }

            // TODO display go graphic
            this.enabled = false;
        }
    }



    public void DecrementEnemy()
        // Reduce total when an enemy is defeated
    {
        totalNumberOfEnemiesToDefeat--;
    }



    private void DisableSpawners()
        // currently not in use, but may be useful if the battle event is made up of conditional spawners that
        // will spawn enemies indefinitely until disabled. 
    {
        foreach (GoonSpawner spawner in goonSpawners)
        {
            spawner.enabled = false;
        }
    }


}
