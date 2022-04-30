using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System;
using Cinemachine;

public class PlayerManager : MonoBehaviour
// This class functions to manage the addition of a second keyboard player, but could be refactored/expanded to manage all players.
{
    [Tooltip("The game object player 2 will spawn as")]
    [SerializeField] GameObject playerPrefab;
    [Tooltip("The button that adds and removes player 2")]
    [SerializeField] Button multiplayerButton;

    [Header("UI for player 2")]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI livesText;
    [SerializeField] HealthBar healthBar;
    [SerializeField] GameObject respawnTimer;

    // Cached references 
    Text buttonText;
    Player p1;
    CinemachineTargetGroup playerTargetGroup;

    // Distance from first player to spawn player 2
    float xOffset = 5; 

    bool altKeyboardPlayerAdded = false;
    public event System.Action<PlayerInput> PlayerJoinedGame;

    public static event Action startedMultiplayer;
    public static event Action endedMultiplayer;

    // Keep track of number of players (though game currently only supports a max of two)
    int numberOfPlayers = 1;


    private void Start()
    // Get references to button, player 1, and the cinemachine target group
    {
        p1 = FindObjectOfType<Player>();
        buttonText = multiplayerButton.GetComponentInChildren<Text>();
        buttonText.text = "Add Keyboard Player";
        playerTargetGroup = GameObject.Find("PlayerTargetGroup").GetComponent<CinemachineTargetGroup>();
    }



    public void HandleMultiplayerButton()
    // Spawn or remove second keyboard player
    {
        if (!altKeyboardPlayerAdded)
        {
            SpawnAltKeyboardPlayer();
        }
        else
        {
            RemoveAltKeyboardPlayer();
        }
    }



    public void SpawnAltKeyboardPlayer()
    // Spawn second keyboard player at same position as other player
    {
        var p2 = PlayerInput.Instantiate(playerPrefab,
    controlScheme: "KeyboardAlt", pairWithDevice: Keyboard.current);

        var p1Position = p1.transform.position;
        p2.transform.position = new Vector3(p1Position.x - xOffset, p1Position.y);

        numberOfPlayers++;
        p2.GetComponent<Player>().SetPlayerNumber(numberOfPlayers);

        altKeyboardPlayerAdded = true;
        buttonText.text = "Remove Keyboard Player";
        startedMultiplayer?.Invoke();
        EnableUI(p2);

        playerTargetGroup.AddMember(p2.transform, 1, 20);

    }



    private void EnableUI(PlayerInput p2)
    // Connect and enable UI for player 2  
    {

        p2.GetComponent<Score>().scoreText = scoreText;
        p2.GetComponent<LivesText>().livesText = livesText;
        p2.GetComponent<Health>().healthBar = healthBar;
        p2.GetComponent<Health>().respawnTimer = respawnTimer; // Respawn timer starts off inactive

        ShowPlayerNumberText();

        scoreText.gameObject.SetActive(true);
        livesText.gameObject.SetActive(true);
        healthBar.gameObject.SetActive(true);
    }



    private void ShowPlayerNumberText()
    {
        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            var playerNumberText = player.GetComponentInChildren<TextMeshPro>();
            playerNumberText.enabled = true;
            switch (player.GetPlayerNumber())
            {
                case 1:
                    playerNumberText.text = "P1";
                    break;
                case 2:
                    playerNumberText.text = "P2";
                    break;
                default:
                    Debug.LogError("Game does not yet support more than two players");
                    break;
            }
            
        }
    }



    private void DisableUI()
    // Disable player 2 UI 
    {
        scoreText.gameObject.SetActive(false);
        livesText.gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
        respawnTimer.SetActive(false);
    }



    public void RemoveAltKeyboardPlayer()
    // Remove second keyboard player from the game
    {
        var players = FindObjectsOfType<Player>();

        foreach (var player in players)
        {
            // Disable player number text. Will need different solution if gamepads or 3+ player coop introduced
            player.GetComponentInChildren<TextMeshProUGUI>().enabled = false;

            var controlScheme = player.GetComponent<PlayerInput>().currentControlScheme;
            if (controlScheme == "KeyboardAlt")
            {
                DisableUI();
                playerTargetGroup.RemoveMember(player.transform);
                Destroy(player.gameObject);
                break;
            }
        }

        numberOfPlayers--;
        altKeyboardPlayerAdded = false;
        endedMultiplayer?.Invoke();
        buttonText.text = "Add Keyboard Player";
        
    }



    public bool InMultiplayer()
    // For now the only multiplayer is through the second keyboard player 
    {
        return altKeyboardPlayerAdded;
    }
}
