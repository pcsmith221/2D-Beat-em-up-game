using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEvent : MonoBehaviour
{
    [Tooltip("Place spawners in scene first then drag to this field")]
    [SerializeField] GoonSpawner[] goonSpawners;
    int totalNumberOfEnemiesToDefeat = 0;
    bool battleStarted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (battleStarted == false)
        {
            //lock camera
            battleStarted = true;
            EnableSpawners();
        }
    }

    private void EnableSpawners()
    {
        foreach (GoonSpawner spawner in goonSpawners)
        {
            spawner.enabled = true;
            StartCoroutine(spawner.ActivateSpawner());
            totalNumberOfEnemiesToDefeat += spawner.numberOfEnemiesToSpawn;
        }
    }

    private void CheckIfAllGoonsDefeated()
    {
        if ((totalNumberOfEnemiesToDefeat <= 0) && (battleStarted == true))
        {
            //unlock camera
            //display go graphic
        }
    }

    void Update()
    {
        CheckIfAllGoonsDefeated();
    }

    private void DisableSpawners()
    {
        foreach (GoonSpawner spawner in goonSpawners)
        {
            spawner.enabled = false;
        }
    }
}
