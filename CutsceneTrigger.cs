using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneTrigger : MonoBehaviour
// Used to trigger in game cinematics, particularly for boss encounters
{

    [SerializeField] Animator cameraAnimator;
    [SerializeField] string cutsceneName = "BossCutscene";
    [SerializeField] Dialogue dialogue;
    [SerializeField] GameObject bossHealthBar;
    [SerializeField] Enemy boss; 

    //[Tooltip("Time in seconds for cutscene")]
    //[SerializeField] float cutsceneDuration = 3;

    bool cutsceneStarted = false;
    Player player;

    private void Update()
    {
        if (cutsceneStarted)
        {
            if (!player.GetIsDisabled())
            {
                StopCutscene();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
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
        bossHealthBar.SetActive(true);
        boss.GetComponent<Enemy>().enabled = true;
        Destroy(gameObject);
    }
}
