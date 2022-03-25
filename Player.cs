using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Unity.Mathematics;
using UnityEngine;

// This class contains all attributes and methods associated with the player character's movement, attacks, and health. 
public class Player : MonoBehaviour
{
    //config variables
    // Any enemy class not included in the layer mask array will not attackable by the player 
    [Header("IMPORTANT: ALL ENEMY TYPES")]
    [SerializeField] LayerMask enemyLayers;

    [Header("Attack Points")]
    [SerializeField] Transform attackPoint;
    [SerializeField] Transform airAttackPoint;

    [Header("Movement")]
    [SerializeField] float runSpeed = 1f;
    [SerializeField] float jumpSpeed = 1f;
    [SerializeField] float gravityScaleWhenJumping = 1.5f;

    [Header("Knockdown")]
    [SerializeField] Vector2 distanceKnockedback = new Vector2(5f, 5f);
    [SerializeField] float knockdownRecoveryTime = 1.5f;

    [Header("Attack Parameters")]
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] int attackDamage = 20;
    [Tooltip("# Attacks per second")]
    [SerializeField] float attackRate = 10f;

    [Header("Sound clip names")]
    [SerializeField] string playerHitSound;

    //cached component references
    Rigidbody2D myRigidBody;
    Animator animator;
    CapsuleCollider2D myCapsuleCollider2D;
    Health health;
    //Collider2D enemyGrabbedCollider;
    PauseScreen pauseScreen;
    DialogueManager dialogueManager;
    AudioManager audioManager;

    //state variables
    Vector2 movement; 
    float yOnGroundPosition;
    float nextAttackTime = 0f;

    bool isGrabbing = false;
    bool isJumping = false;
    bool isAirAttacking = false;
    bool isKnockedback = false;
    bool isDisabled = false;
    bool isInCombat = false;
    EnemyHealth enemyGrabbed;

    //"bool" variables because animation events cant pass bool parameters
    int isGroundAttacking = 0;
    int isHit = 0;

    // Start is called before the first frame update
    void Start()
    {
        // cache the player component references into variables to be used throughout the script 
        myRigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myCapsuleCollider2D = GetComponent<CapsuleCollider2D>();
        health = GetComponent<Health>();

        audioManager = FindObjectOfType<AudioManager>();
        pauseScreen = FindObjectOfType<PauseScreen>();
        dialogueManager = FindObjectOfType<DialogueManager>();
    }



    void Update()
    {
        // Ground attacks don't deal with physics so they can go into Update()
        if (!isKnockedback && !isDisabled && !pauseScreen.GetIsGamePaused() && health.GetIsAlive())
        {
            GroundAttack();
            Grab();
            GrabAttack();
            BackAttack();
        }
        else if (isDisabled)
        {
            Dialogue();
        }
    }



    private void FixedUpdate()
    // The methods that deal with physics should go into FixedUpdate(). 
    {
        if (!isKnockedback && !isDisabled && !pauseScreen.GetIsGamePaused() && health.GetIsAlive())
        {
            Move();
            FlipSprite();
            OnLanding();
            Jump();
            AirAttack();
        }
        else if (isKnockedback || isJumping)
        {
            OnLanding();
        }
    }



    private void Dialogue()
    {
        //if (!isJumping)
        //{
        //    myRigidBody.velocity = new Vector2(0, 0);
        //}

        if (Input.GetButtonDown("Fire1"))
        {
            dialogueManager.DisplayNextSentence();
        }
    }



    public void ProcessHit()
    // Called when the player is hit by an enemy.
    // Damage is dealt to the player in enemy script, EnemyDealDamage() method
    {

        // could add some particle effects
        animator.SetTrigger("playerHit");
        audioManager.Play(playerHitSound);

        // Let any grabbed enemies go if the player is hit
        if (isGrabbing)
        {
            animator.SetBool("isGrabbing", false);
            enemyGrabbed.SetIsGrabbed(false);
            // Could add a breakaway animation for enemies here
            enemyGrabbed.GetComponent<Animator>().SetBool("isGrabbed", false);
            isGrabbing = false;
        }
    }



    private void GroundAttack()
    // The default attack when the player is on the ground and not grabbing an enemy.
    {
        if ((Time.time >= nextAttackTime) && (isHit == 0) && !isJumping && !isGrabbing)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                // Ensure player can only attack the specified number of times per second.
                nextAttackTime = Time.time + (1f / attackRate);

                animator.SetTrigger("attack");

                DealDamage(attackPoint, attackRange, attackDamage);
            }
        }
    }



    public void SetIsGroundAttacking(int isGroundAttacking)
    // Animation events cannot call methods with bool parameters.
    // This method is called from the Attacking animation to make isGroundAttacking true, and from the idle animation to make it false.
    {
        if ( (isGroundAttacking == 1) || (isGroundAttacking == 0) )
        {
            this.isGroundAttacking = isGroundAttacking;
        }
        else
        {
            Debug.LogError("SetIsGroundAttacking passed a value that is not one or zero");
        }
    }



    public void SetIsHit(int isHit)
    // This method is called from the Hit animation to make isHit true, and from the idle animation to make it false.
    {
        if ((isHit == 1) || (isHit == 0))
        {
            this.isHit = isHit;
        }
        else
        {
            Debug.LogError("SetIsHit passed a value not one or zero");
        }
    }



    private void BackAttack()
    // When the player attacks in the opposite direction that they are facing after they have already begun a combo
    {
        // TODO? add another time condition to prevent back attack spam
        // maybe can reduce back attack damage so while it's quicker it's better to do normal combos
        // could probably also rework this method to be less redundant

        // check if player is attacking and pushing left while the character is already attacking to the right
        if (isGroundAttacking == 1 && Input.GetAxisRaw("Horizontal") == -1 && Input.GetButtonDown("Fire1") && transform.localScale.x == 1)
        {
            // Flip the player sprite and trigger back attack
            transform.localScale = new Vector3(-1,1,1);
            animator.SetTrigger("backAttack");

            DealDamage(attackPoint, attackRange, attackDamage);
        }
        // same as above code except the player is starting out facing left
        else if (isGroundAttacking == 1 && Input.GetAxisRaw("Horizontal") == 1 && Input.GetButtonDown("Fire1") && transform.localScale.x == -1)
        {
            transform.localScale = new Vector3(1, 1, 1);
            animator.SetTrigger("backAttack");

            DealDamage(attackPoint, attackRange, attackDamage); 
        }
    }



    private void AirAttack()
    // Trigger air attack if player presses attack while jumping and has not already air attacked
    {
        if (Input.GetButtonDown("Fire1") && isJumping && !isAirAttacking)
        {
            isAirAttacking = true;
            animator.SetBool("isAirAttacking", true);

            DealDamage(airAttackPoint, attackRange, attackDamage);
        }
    }



    private void Grab()
    // Grab an enemy and prevent them from moving.
    // Currently no functionality for an enemy breaking free on their own. Enemies are only released when the player is hit, when they are thrown, or when they lose all health. 
    {
        //only grab if not being hit or already performing another action
        if (Input.GetButtonDown("Fire2") && !isJumping && isHit == 0 && isGroundAttacking == 0 && !isGrabbing)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            // Only attempt to grab one enemy
            if (hitEnemies.Length > 0) 
            {
                enemyGrabbed = hitEnemies[0].GetComponent<EnemyHealth>();

                if (enemyGrabbed.IsGrabbable())
                {
                    // Position the grabbed enemy in the right place depending on the direction the player facing
                    if(transform.localScale.x == 1)
                    {
                        enemyGrabbed.transform.position = new Vector3(gameObject.transform.position.x + enemyGrabbed.grabPositionXOffset,
                                                                  gameObject.transform.position.y, 0);
                    }
                    // For when the player grabs while facing left
                    else
                    {
                        enemyGrabbed.transform.position = new Vector3(gameObject.transform.position.x - enemyGrabbed.grabPositionXOffset,
                                                                  gameObject.transform.position.y, 0);
                    }

                    isGrabbing = true;
                    animator.SetBool("isGrabbing", true);
                    animator.SetBool("grabbedEnemyIsAlive", true);
                    enemyGrabbed.SetIsGrabbed(true);
                }
            }
        }
    }



    private void GrabAttack()
    // Attack or throw grabbed enemy 
    {
        if (isGrabbing)
        {
            // Attack enemy while still adhering to the attack rate. 
            if (Input.GetButtonDown("Fire1") && Time.time >= nextAttackTime)
            {
                animator.SetTrigger("attack");
                nextAttackTime = Time.time + (1f / attackRate);
                enemyGrabbed.TakeDamage(attackDamage);

                if (enemyGrabbed.GetCurrentHealth() <= 0)
                {
                    isGrabbing = false;
                    animator.SetBool("isGrabbing", false);
                    animator.SetBool("grabbedEnemyIsAlive", false);
                }
            }
            // Grab throw
            else if(Input.GetButtonDown("Fire3"))
            {
                animator.SetTrigger("throw");
                isGrabbing = false;
                animator.SetBool("isGrabbing", false);

                enemyGrabbed.SetIsGrabbed(false);

                // Throw enemy in the direction that the player is facing
                enemyGrabbed.ThrowEnemy(Mathf.Sign(transform.localScale.x));
            }
        }
    }



    private void DealDamage(Transform pointOfAttack, float range, int damage)
    // Create array of all enemies within range of attack point and deal damage.
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pointOfAttack.position, range, enemyLayers);

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                hit.GetComponent<EnemyHealth>().TakeDamage(damage);
                hit.GetComponent<EnemyHealth>().SetPlayerMostRecentlyAttackedBy(this);
            }
            else if (hit.CompareTag("Breakable"))
            {
                hit.GetComponent<Breakable>().HandleHit();
            }
            else
            {
                Debug.LogWarning("Might be missing tag on " + hit.name);
            }
        }
    }



    private void Move()
    // Move player on the 2D plane using rigid body velocity. 
    {
        if (isGroundAttacking == 1 || isHit == 1 || isGrabbing)
        {
            myRigidBody.velocity = new Vector2(0, 0);
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");

        // Only allow player to move horizontally while jumping. Vertical movement would let the player fly or allow them to cut the jump short. 
        if (!isJumping) 
        {
            movement.y = Input.GetAxisRaw("Vertical");
        }

        // Normalize movement vector to prevent faster diagonal movement.
        myRigidBody.velocity = new Vector2(movement.normalized.x * runSpeed, movement.normalized.y * runSpeed);

        bool playerIsMoving = (Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon) || (Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon);
        animator.SetBool("isMoving", playerIsMoving);
    }



    private void Jump()
    // Gives the player upwards velocity and turns on gravity until the character returns to the y value they jumped from. `
    // Gravity generally remains off, otherwise the player would be constantly falling. 
    {
        if (Input.GetButtonDown("Jump") && !isJumping && (movement.y == 0) && (isGroundAttacking == 0) && !isGrabbing) 
        {
            yOnGroundPosition = transform.position.y;
            isJumping = true;
            myRigidBody.gravityScale = gravityScaleWhenJumping;

            // Allow player to jump through upper boundary.
            myCapsuleCollider2D.isTrigger = true;

            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity += jumpVelocityToAdd;

            animator.SetBool("isJumping", true);
        }
    }



    public void Knockback(float signOfX)
    // Knocks player down, transitions animation, and changes isKnockedback state to true
    {
        yOnGroundPosition = transform.position.y; 
        isKnockedback = true;
        myCapsuleCollider2D.isTrigger = true;

        myRigidBody.gravityScale = gravityScaleWhenJumping;

        // Have player face opposite the direction they are being knockedback 
        transform.localScale = new Vector2(-signOfX, 1f);
        myRigidBody.velocity = new Vector2(distanceKnockedback.x * signOfX, distanceKnockedback.y);

        animator.SetBool("isKnockedback", true);
    }



    private IEnumerator KnockdownRecover()
    //force player to wait in knockdown state for a set amount of time
    {
        myRigidBody.velocity = new Vector2(0, 0);

        if (health.GetIsAlive())
        {
            yield return new WaitForSeconds(knockdownRecoveryTime);
        }

        isKnockedback = false;
        animator.SetBool("isKnockedback", false);
    }



    private void OnLanding()
    // Turns off the player's gravity when they touch the ground, ends jump animation if jumping, begins recover if knocked back. 
    {
        if (transform.position.y < yOnGroundPosition)
        {
            myRigidBody.gravityScale = 0;
            yOnGroundPosition = transform.position.y;
            myCapsuleCollider2D.isTrigger = false;

            if (isJumping)
            {
                isJumping = false;
                animator.SetBool("isJumping", false);

                if (isAirAttacking)
                {
                    isAirAttacking = false;
                    animator.SetBool("isAirAttacking", false);
                }
            }
            else if (isKnockedback)
            {
                StartCoroutine(KnockdownRecover());
            }
        }
    }



    public bool GetIsKnockedback()
    {
        return isKnockedback;
    }



    private void FlipSprite()
    // Changes the x value based on the direction the player is moving ('+' = right, '-' = left)
    {
        bool playerHasHorizontalSpeed = (Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon);
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f); 
        }
    }



    public void SetIsDisabled(bool isDisabled)
    // Used to force player into idle state where they can't move until enabled again
    {
        if (isDisabled)
        {
            this.isDisabled = true;
            //animator.SetTrigger("idle");
            animator.SetBool("isMoving", false);

            myRigidBody.velocity = new Vector2(0, 0);
        }
        else
        {
            this.isDisabled = false;
        }
    }



    public bool GetIsDisabled()
    {
        return isDisabled;
    }




    public void SetIsInCombat(bool isInCombat)
    // Used to ensure that dialogue not triggered while in combat
    {
        this.isInCombat = isInCombat;
    }



    public bool GetIsInCombat()
    {
        return isInCombat;
    }



    private void OnDrawGizmosSelected()
    // Visualizes the player's attack ranges in the inspector for easier adjusting in the editor
    {
        if (attackPoint == null)
        {
            Debug.LogError("No attack point on player");
            return;
        }

        if (airAttackPoint == null)
        {
            Debug.LogError("No air attack point on player");
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawWireSphere(airAttackPoint.position, attackRange);
    }



}
