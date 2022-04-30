using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Similar logic to pickups. If a player is in range and presses the interact button, the dialogue manager is called to start the dialogue. 
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] Dialogue dialogue;

    // State variable
    bool inDialogueRange;
    bool triggeredAlready = false;


    private void ReplayDialogue()
    // Allow player to trigger dialogue again manually. 
    {
        if (inDialogueRange)
        {
            TriggerDialogue();
        }
    }

    //void Update()
    //{
    //    if (inDialogueRange && Input.GetButtonDown("Pickup"))
    //    {
    //        TriggerDialogue();
    //    }
    //}



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !collision.GetComponent<Player>().GetIsInCombat() && !triggeredAlready)
        {
            inDialogueRange = true;
            triggeredAlready = true;
            TriggerDialogue();
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            inDialogueRange = true;
        }

    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            inDialogueRange = false;
        }
    }



    public void TriggerDialogue()
    // Start the dialogue associated with this trigger in the dialogue manager.
    {
        // Disables speech bubble animation if there is one
        if (gameObject.transform.childCount != 0)
        {
            GetComponentInChildren<Animator>().enabled = false;
        }

        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }



    private void OnEnable()
    // Subscribes to pauseGame event in Player script
    {
        Player.interact += ReplayDialogue;
    }



    private void OnDisable()
    // Unsubscribes from pauseGame event should the script become disabled
    {
        Player.interact -= ReplayDialogue;
    }

}
