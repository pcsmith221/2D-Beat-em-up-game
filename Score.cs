using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Methods for updating and displaying the player's current score.

public class Score : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] int score = 0; //serialized atm for debug purposes

    public void AddToScore(int scoreToAdd)
    {
        score += scoreToAdd;
        DisplayScore();
    }

    private void DisplayScore()
    {
        scoreText.text = score.ToString();
    }
}
