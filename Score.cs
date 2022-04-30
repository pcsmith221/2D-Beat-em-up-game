using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Methods for updating and displaying the player's current score.

public class Score : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    [SerializeField] int score = 0; //serialized atm for debug purposes

    private void Start()
    {
        scoreText.text = "Score: " + score.ToString();
    }



    public void AddToScore(int scoreToAdd)
    {
        score += scoreToAdd;
        DisplayScore();
    }



    private void DisplayScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }



    public int GetScore()
    {
        return score;
    }
}
