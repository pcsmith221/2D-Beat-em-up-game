using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

public class BossFight : MonoBehaviour
// Used to trigger in game cinematics, particularly for boss encounters
{
    [Header("Cutscene details")]
    [SerializeField] Animator cameraAnimator;
    [SerializeField] string cutsceneName = "BossCutscene";
    [SerializeField] Dialogue dialogue;

    [Header("For boss fights")]
    [SerializeField] bool isBossFight = true;
    [SerializeField] GameObject bossHealthBar;
    [SerializeField] Enemy boss;
    [SerializeField] GoonSpawner[] enemySpawners;
    [SerializeField] GameObject playerBattleColliders;

    //[Tooltip("Time in seconds for cutscene")]
    //[SerializeField] float cutsceneDuration = 3;

    // State variables
    bool cutsceneStarted = false;
    bool bossFightStarted = false;

    // Cahced References
    Player player;
    CinemachineStateDrivenCamera virtualCamera;

    private void Start()
    {
        boss.GetComponent<Enemy>().enabled = false;
        virtualCamera = FindObjectOfType<CinemachineStateDrivenCamera>();
        playerBattleColliders.SetActive(false);

    }



    private void Update()
    {
        if (cutsceneStarted && !bossFightStarted)
        {
            // Once the player finishes dialogue they will no longer be disabled and the camera should go back to the player
            if (!player.GetIsDisabled())
            {
                StopCutscene();
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Only players can start the cutscene, and only if it has not been activated yet
        if (collision.gameObject.CompareTag("Player") && !cutsceneStarted)
        {
            player = collision.GetComponent<Player>();
            cameraAnimator.SetBool(cutsceneName, true);
            FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
            cutsceneStarted = true;
        }
    }



    private void StopCutscene()
    {
        cameraAnimator.SetBool(cutsceneName, false);
        Debug.Log("Stop Cutscene called");

        if (isBossFight)
        {
            bossFightStarted = true;
            StartBossFight();
        }
    }



    private void StartBossFight()
    {
        Debug.Log("Start boss fight called");
        bossHealthBar.SetActive(true);
        boss.GetComponent<Enemy>().enabled = true;
        virtualCamera.enabled = false;
        playerBattleColliders.SetActive(true);

        foreach(var spawner in enemySpawners)
        {
            StartCoroutine(spawner.StartSpawning());
        }
    }



    private void OnEnable()
    // Subscribes to boss died event in game manager
    {
        GameManager.bossDied += BossDefeated;
    }



    private void OnDisable()
    // Unsubscribes from boss died event in game manager
    {
        GameManager.bossDied -= BossDefeated;
    }



    private void BossDefeated()
    {
        Debug.Log("Boss Defeated");
        foreach (var spawner in enemySpawners)
        {
            spawner.StopSpawning();
            StopCoroutine(spawner.StartSpawning());
            spawner.enabled = false;
        }

        playerBattleColliders.SetActive(false);
        bossHealthBar.SetActive(false);
        virtualCamera.enabled = true;
        

    }
}
