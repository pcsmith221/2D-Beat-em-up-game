using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that allows players to pick up items to increase health or score if they are near it. 
public class Pickup : MonoBehaviour
{
    [SerializeField] Item item;
    SpriteRenderer spriteRenderer;
    Health playerHealth;
    Score playerScore;

    //state variables
    public bool pickupAllowed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = item.art;
    }


    void Update()
    {
        if (pickupAllowed && Input.GetButtonDown("Pickup"))
        {
            ItemPickedUp();
        }
    }



    private void ItemPickedUp()
    {
        playerHealth.GainHealth(item.health);
        playerScore.AddToScore(item.score);
        Destroy(gameObject);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        //comparing tag may not even be necessary if player layer the only one able to collide with object in the first place
        if (collision.gameObject.CompareTag("Player"))
        {
            //Potential issue in multiplayer if both players are standing on the same pickup? Potential case where health could go to player who stood on it first
            playerHealth = collision.GetComponent<Health>();
            playerScore = collision.GetComponent<Score>();
            pickupAllowed = true;
        }

    }



    private void OnTriggerExit2D(Collider2D collision)
    {
        //ditto above tag comment
        if (collision.gameObject.CompareTag("Player"))
        {
            pickupAllowed = false;
        }
    }
}
