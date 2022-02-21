using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LivesText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI livesText;

    public void DisplayLives(int lives)
    {
        livesText.text = "Lives: " + lives;
    }
}
