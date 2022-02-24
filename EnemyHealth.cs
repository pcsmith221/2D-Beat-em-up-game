using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that manages Enemy health and grabbed states. 
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] string hitSound;
    [SerializeField] string deathSound;
    [SerializeField] string hitGroundSound;

    [Header("Health Parameters")]

    [SerializeField] int maxHealth = 100;
    [SerializeField] int scoreForDefeating = 100;
    [SerializeField] float gravityScaleWhenThrown = 1.5f;
    [SerializeField] float recoveryTime = 2f;
    [SerializeField] bool isGrabbable = true;
    [SerializeField] bool staggerOnHit = true;
    [SerializeField] int hitGroundDamage = 10;

    [Header("Knockback/grab Parameters")]

    [Tooltip("Enemies that will get knocked down if this thrown into them")]
    [SerializeField] LayerMask enemyLayers;
    [SerializeField] Transform knockbackOtherEnemiesPoint;

    [Tooltip("Other enemies caught in this range will be knocked back when this enemy is thrown")]
    [SerializeField] float knockbackOtherEnemiesRange = .5f;
    public Vector2 distanceKnockedback = new Vector2(5f, 5f);
    [SerializeField] float knockdownRecoveryTime = 1.5f;

    public Vector2 distanceThrownByPlayer = new Vector2(25f, 25f);
    public float grabPositionXOffset = 1f;
    
    // State variables
    int currentHealth;
    bool isGrabbed = false;
    bool isBeingThrown = false;
    bool isBeingKnockedback = false;
    bool isKnockedback = false;
    float yThrownPosition;
    float timeCanMoveAgain = 0f;
    Player playerMostRecentlyAttackedBy; // for correct individual scoring

    // Cached references
    Animator animator;
    Rigidbody2D rb;
    Collider2D myCollider2D;
    BattleEvent battleEvent;
    AudioManager audioManager;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        myCollider2D = GetComponent<Collider2D>();

        audioManager = FindObjectOfType<AudioManager>();

        // The only enemies that have a parent transform are those spawned within a battle event. This allows me to leave enemies outside battle events. 
        if(transform.parent)
        {
            battleEvent = GetComponentInParent<BattleEvent>();
        }
    }



    private void FixedUpdate()
    {
        if (isBeingThrown)
        {
            // Only thrown enemies can knock back other enemies to avoid domino effect
            KnockbackCollision();
            HitGround();
        }
        else if (isBeingKnockedback)
        {
            HitGround();
        }
    }



    private void KnockbackCollision()
    // Knockback other enemies the thrown enemy collides with. 
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(knockbackOtherEnemiesPoint.position, knockbackOtherEnemiesRange, enemyLayers);

        //TODO only call knockback method on enemies that are knockbackable 
        foreach (Collider2D knockedEnemyCollider in hitEnemies)
        {
            if (!knockedEnemyCollider.Equals(this.myCollider2D))
            {
                knockedEnemyCollider.GetComponent<EnemyHealth>().Knockback();
            }
        }
    }



    private void Knockback()
    // Knockback enemy. Currently called when hit by a thrown enemy, but could be called by Anahey's attacks. Maybe all get knocked down on respawn? 
    {
        isBeingKnockedback = true;

        yThrownPosition = transform.position.y;

        animator.SetBool("isThrown", true);

        rb.gravityScale = gravityScaleWhenThrown;
        rb.velocity = new Vector2(distanceKnockedback.x * -1, distanceKnockedback.y);

    }



    public void ThrowEnemy(float signOfX)
    // Triggeres thrown animation and applies velocity to the enemy in the direction the player was facing. 
    {
        yThrownPosition = transform.position.y;

        animator.SetBool("isGrabbed", false);
        animator.SetBool("isThrown", true);

        rb.gravityScale = gravityScaleWhenThrown;
        rb.velocity = new Vector2(distanceThrownByPlayer.x * signOfX, distanceThrownByPlayer.y);

        isBeingThrown = true;
    }



    private void HitGround()
    // Checks if enemy is still in the air. If not, reset gravity, change animation state, and begin knockdown recovery
    {
        if (transform.position.y < yThrownPosition)
        {

            timeCanMoveAgain = Time.time + recoveryTime;

            animator.SetBool("isThrown", false);
            audioManager.Play(hitGroundSound);

            isBeingKnockedback = false;
            isBeingThrown = false;

            rb.gravityScale = 0;
            rb.velocity = new Vector2(0, 0);

            TakePhysicsDamage(hitGroundDamage);

            isKnockedback = true;
            animator.SetBool("isKnockedDown", true);

            StartCoroutine(KnockdownRecover());
        }
    }



    private IEnumerator KnockdownRecover()
    // Force enemy to wait in knockdown state for a set amount of time
    {
        rb.velocity = new Vector2(0, 0);
        yield return new WaitForSeconds(knockdownRecoveryTime);
        animator.SetBool("isKnockedDown", false);
        isKnockedback = false;
    }



    public bool IsRecovered()
    // Check if Enemy is in default state. 
    {
        if (!isGrabbed && !isBeingThrown && !isBeingKnockedback && !isKnockedback)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    public void SetIsGrabbed(bool isGrabbed)
    {
        this.isGrabbed = isGrabbed;
        if (this.isGrabbed)
        {
            animator.SetBool("isGrabbed", true);
        }
    }



    public void TakeDamage(int damage)
    // Reduce health and trigger hit animation. 
    {
        currentHealth -= damage;

        // Prevent enemy from taking damage when knockedback. 
        if (isKnockedback)
        {
            return;
        }

        if (staggerOnHit)
        {
            animator.SetTrigger("hit");
            audioManager.Play(hitSound);
        }

        if (currentHealth <= Mathf.Epsilon)
        {
            Die();
        }
    }



    private void TakePhysicsDamage(int damage)
    // Take damage but do not trigger hit animation. 
    {
        currentHealth -= damage;
        if (currentHealth <= Mathf.Epsilon)
        {
            Die();
        }
    }



    private void Die()
    // Triggers death animation and disables the enemy. 
    {
        if(transform.parent)
        {
            battleEvent.DecrementEnemy();
        }
        
        animator.SetBool("isDead", true);
        audioManager.Play(deathSound);

        if (isGrabbed)
        {
            animator.SetBool("isGrabbed", false);
        }

        playerMostRecentlyAttackedBy.GetComponent<Score>().AddToScore(scoreForDefeating);

        //disable the enemy
        GetComponentInChildren<SpriteRenderer>().color = Color.grey;
        GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Dead";
        GetComponent<Collider2D>().enabled = false;

        if (gameObject.CompareTag("Enemy"))
        {
            GetComponent<Enemy>().enabled = false;
        }

        this.enabled = false;
    }



    public void SetPlayerMostRecentlyAttackedBy(Player player)
        // Used to give the correct player the score when the enemy is defeated.
    {
        playerMostRecentlyAttackedBy = player;
    }

    public bool GetIsGrabbed()
    {
        return isGrabbed;
    }

    public bool GetIsBeingThrown()
    {
        return isBeingThrown;
    }

    public bool IsGrabbable()
    {
        return isGrabbable;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    private void OnDrawGizmosSelected()
    // Visualizes knockback range for easier adjusting in the editor
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(knockbackOtherEnemiesPoint.position, knockbackOtherEnemiesRange);
    }
}
