using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Manages player health and updates the heatlh bar.

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    public HealthBar healthBar;
    [SerializeField] int lives = 3;
    [SerializeField] float respawnTime = 3f;

    [Tooltip("Amount of time before player respawns in multiplayer")]
    [SerializeField] float multiplayerRespawnTime = 5f;
    [SerializeField] float multiplayerRespawnTimer;
    [Tooltip("Minimum distance required for respawning player to spawn next to other player")]
    [SerializeField] float spawnNextToOtherPlayerDistance = 100f;
    // Distance from alive to player to spawn if body outside camera 
    float xOffset = 5;

    [SerializeField] string playerDeathSound;
    public GameObject respawnTimer;

    int health;
    bool isAlive;
    bool isGameOver = false;
    bool inMultiplayer;
    bool timerIsGoing;
    bool respawned;

    Animator animator;
    Player player;
    Collider2D playerCollider;
    SpriteRenderer spriteRenderer;
    LivesText livesText;
    GameManager gameManager;
    TextMeshProUGUI respawnTimerText;

    //private void Awake()
    //{
    //    respawnTimer.SetActive(false);
    //}



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

        gameManager = FindObjectOfType<GameManager>();
        gameManager.AddPlayerLives(lives+1);

        respawnTimerText = respawnTimer.GetComponent<TextMeshProUGUI>();
        multiplayerRespawnTimer = multiplayerRespawnTime;
    }



    private void Update()
    {
        if (timerIsGoing)
        {
            MultiplayerRespawnTimer();
        }
    }



    private void MultiplayerRespawnTimer()
    // Respawns player automatically in multiplayer
    {
        if (!isGameOver)
        {
            if ((multiplayerRespawnTimer > 0) && !isGameOver)
            {
                multiplayerRespawnTimer -= Time.deltaTime;
                respawnTimerText.text = "Respawning in: " + (int)multiplayerRespawnTimer;
            }
            // Respawning takes some time so try to come as close as possible to aligning timer with actual player spawn
            if (!respawned && (multiplayerRespawnTimer < respawnTime) && !isGameOver)
            {
                respawned = true;
                StartCoroutine(Respawn());
            }
            else if ((multiplayerRespawnTimer < 0) || isGameOver)
            {
                respawnTimer.SetActive(false);
                timerIsGoing = false;
                multiplayerRespawnTimer = multiplayerRespawnTime;
                gameManager.IncrementPlayerLives();

            }
        }
        else
        {
            respawnTimer.SetActive(false);
            timerIsGoing = false;
        }
        
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
            Die();
        }
    }



    private void Die()
    // Triggers death animation and disables player until they respawn
    {
        gameManager.DecrementPlayerLives();

        animator.SetBool("isDead", true);
        FindObjectOfType<AudioManager>().Play(playerDeathSound);

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
        else if ((lives == 0) && inMultiplayer && !isGameOver)
        {
            timerIsGoing = true;
            respawnTimer.SetActive(true);
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

        // If multiplayer respawn, spawn player next to other player if distance puts player roughly outisde camera range
        if (respawned)
        {
            var alivePlayers = FindObjectsOfType<Health>();
            foreach (var player in alivePlayers)
            {
                // var distanceBetweenPlayers = Vector3.Distance(transform.position, player.transform.position);
                if (player.GetIsAlive()) //&& (distanceBetweenPlayers > spawnNextToOtherPlayerDistance))
                {
                    transform.position = new Vector3(player.transform.position.x - xOffset, player.transform.position.y);
                    respawned = false;
                    break;
                }
            }
        }

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



    private void OnEnable()
    // Subscribes to multiplayer events in PlayerManager script
    {
        PlayerManager.startedMultiplayer += AddRespawnTimer;
        PlayerManager.endedMultiplayer += TakeOutRespawnTimer;
        GameManager.gameEnded += GameIsOver;
    }

    private void AddRespawnTimer()
    {
        inMultiplayer = true;
    }

    private void TakeOutRespawnTimer()
    {
        inMultiplayer = false;
    }

    private void GameIsOver()
    {
        isGameOver = true;
    }

    private void OnDisable()
    // Unsubscribes from multiplayer events in PlayerManager script
    {
        PlayerManager.startedMultiplayer -= AddRespawnTimer;
        PlayerManager.endedMultiplayer -= TakeOutRespawnTimer;
        GameManager.gameEnded -= GameIsOver;
    }
}
