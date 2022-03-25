using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] Animator animator;

    Queue<string> sentences;


    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }



    public void StartDialogue(Dialogue dialogue)
    // Brings down dialogue UI and disables player until dialogue is finished. 
    {
        //Time.timeScale = 0f;

        var players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            player.SetIsDisabled(true);
        }

        animator.SetBool("isOpen", true);

        nameText.text = dialogue.GetName();

        sentences.Clear();

        foreach (string sentence in dialogue.GetSentences())
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }



    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();

        // If sentence already running, ensure current coroutine is stopped 
        StopAllCoroutines();

        StartCoroutine(TypeSentence(sentence));
    }



    IEnumerator TypeSentence(string sentence)
     // Type sentence out letter by letter with a frame duration between each letter
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }



    void EndDialogue()
    // Remove dialogue box and enable player
    {
        animator.SetBool("isOpen", false);

        var players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            player.SetIsDisabled(false);
        }

        //Time.timeScale = 1f;
    }
}
