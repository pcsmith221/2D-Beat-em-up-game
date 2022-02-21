using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manages player health and updates the heatlh bar.

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] HealthBar healthBar;
    [SerializeField] int lives = 3;
    [SerializeField] GameManager gameManager;
    [SerializeField] float respawnTime = 3f;

    int health;
    bool isAlive;

    Animator animator;
    Player player;
    Collider2D playerCollider;
    SpriteRenderer spriteRenderer;
    LivesText livesText;

    private void Start()
    {
        health = maxHealth;
        isAlive = true;
        healthBar.SetMaxHealth(maxHealth);
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        livesText = GetComponent<LivesText>();
        livesText.DisplayLives(lives);
    }



    public int GetHealth()
    {
        return health;
    }



    public int GetLives()
    {
        return lives;
    }



    public void LoseHealth(int damage) 
    {
        health -= damage;
        healthBar.SetHealth(health);

        if (health <= Mathf.Epsilon) 
        {
            Debug.Log("Player died!");
            //die animation and respawn if lives > 0 (lives in game manager script?), or die and game over.
            /*if (lives < 0)
            {
                Respawn();
            }
            else
            {
                GameOver(); 
            }*/
            Die();
        }
    }



    private void Die()
    // Triggers death animation, disables the player, and shows game over UI.
    // Currently ends game, which should be changed to be handled entirely by the game manager
    {
        gameManager.DecrementPlayerLives();

        animator.SetBool("isDead", true);

        //disable the player
        isAlive = false;
        spriteRenderer.color = Color.grey;
        spriteRenderer.sortingLayerName = "Dead";
        playerCollider.enabled = false;

        // Prevent player sliding bug if die when moving. (might introduce new bug if killed by shark enemy)
        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0); 
        //player.enabled = false;

        //this.enabled = false;

        if (lives > 0)
        {
            lives--;
            livesText.DisplayLives(lives);
            StartCoroutine(Respawn());
        }
    }



    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);
        animator.SetBool("isDead", false);
        spriteRenderer.color = Color.white;
        spriteRenderer.sortingLayerName = "Alive";
        playerCollider.enabled = true;
        player.enabled = true;
        this.enabled = true;

        GainHealth(maxHealth);
        isAlive = true;
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

        healthBar.SetHealth(health);
    }


    public bool GetIsAlive()
    {
        if (isAlive)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
