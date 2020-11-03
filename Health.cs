using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    int health;

    private void Start()
    {
        health = maxHealth;
    }

    public int GetHealth()
    {
        return health;
    }

    public void LoseHealth(int damage) 
    {
        health -= damage;
        if (health <= Mathf.Epsilon) 
        {
            Debug.Log("Player died!");
            //die animation and respawn if lives > 0 (lives in game manager script?), or die and game over. 
        }
    }

    public void GainHealth(int recovery)
    {
        if (health + recovery > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += recovery;
        }
    }
}
