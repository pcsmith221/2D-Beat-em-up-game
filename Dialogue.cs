using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class contains the name and sentences of dialogue for a character that is configured and started by a DialogueTrigger
[System.Serializable]
public class Dialogue
{
    [SerializeField] string name;

    [TextArea(3,10)]
    [SerializeField] string[] sentences;

    public string[] GetSentences()
    {
        return sentences;
    }


    public string GetName()
    {
        return name;
    }
}
