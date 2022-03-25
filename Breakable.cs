using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    // Configuration parameters
    [SerializeField] Pickup pickup;
    [SerializeField] Sprite[] hitSprites;
    [Tooltip("How many hits it takes to reach the next damage level")]
    [SerializeField] int hitsPerDamageLevel = 1;

    // State variables
    int maxHits;
    public int damageLevel;
    int spriteIndex = 0;

    // Chached references
    AudioManager audioManager;
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        maxHits = (hitSprites.Length + 1) * hitsPerDamageLevel;

        audioManager = FindObjectOfType<AudioManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }



    public void HandleHit()
    {
        audioManager.Play("Hit Breakable");

        damageLevel++;

        if (damageLevel >= maxHits)
        {
            Break();
        }
        else if (damageLevel % hitsPerDamageLevel == 0)
        {
            ShowNextHitSprite();
        }
    }



    private void Break()
    // Spawn pickup and destroy object. 
    {
        audioManager.Play("Broke Breakable");
        Instantiate(pickup, transform.position, transform.rotation);
        Destroy(gameObject);
    }



    private void ShowNextHitSprite()
    // Change sprite to next damage level.
    {
        if (hitSprites[spriteIndex] != null)
        {
            spriteRenderer.sprite = hitSprites[spriteIndex];
        }
        else
        {
            Debug.LogError("Block sprite is missing from array " + gameObject.name);
        }

        spriteIndex++;
    }
}
