using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    //config variables
    [Header("IMPORTANT: ALL ENEMY TYPES")]
    [SerializeField] LayerMask enemyLayers;

    [Header("Attack Points")]
    [SerializeField] Transform attackPoint;
    [SerializeField] Transform airAttackPoint;

    [Header("Movement")]
    [SerializeField] float runSpeed = 1f;
    [SerializeField] float jumpSpeed = 1f;
    [SerializeField] float gravityScaleWhenJumping = 1.5f;

    [Header("Attack Parameters")]
    [SerializeField] float attackRange = 0.5f;
    [SerializeField] int attackDamage = 20;
    [Tooltip("# Attacks per second")]
    [SerializeField] float attackRate = 10f; //number of attacks per second

    //cached component references
    Rigidbody2D myRigidBody;
    Animator animator;
    CapsuleCollider2D myCapsuleCollider2D;
    Collider2D enemyGrabbedCollider;

    //state variables
    Vector2 movement; 
    float yJumpPosition;
    float nextAttackTime = 0f;
    //bool isAlive = true;
    bool isGrabbing = false;
    bool isJumping = false;
    bool isAirAttacking = false;
    EnemyHealth enemyGrabbed;

    //"bool" variables because animation events cant pass bool parameters
    int isGroundAttacking = 0;
    int isHit = 0;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myCapsuleCollider2D = GetComponent<CapsuleCollider2D>(); 
    }

    void Update()
    {
        GroundAttack();
        Grab();
        GrabAttack();
        BackAttack();
    }

    private void FixedUpdate()
    {
        Move();
        FlipSprite();
        OnLanding();
        Jump();
        AirAttack();
    }

    public void ProcessHit()
    {
        //damage dealt in enemy script, EnemyDealDamage method
        //set animator state, play any sound or particle effects
        animator.SetTrigger("playerHit");

        if (isGrabbing)
        {
            animator.SetBool("isGrabbing", false);
            enemyGrabbed.SetIsGrabbed(false); //perhaps have some breakaway animation
            enemyGrabbed.GetComponent<Animator>().SetBool("isGrabbed", false);
            enemyGrabbedCollider.enabled = true;
            isGrabbing = false;
        }
    }

    private void GroundAttack()
    {
        if (Time.time >= nextAttackTime && isHit == 0 && !isJumping && !isGrabbing)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                nextAttackTime = Time.time + (1f / attackRate);

                animator.SetTrigger("attack");

                DealDamage(attackPoint, attackRange, attackDamage);
            }
        }
    }

    public void SetIsGroundAttacking(int oneOrZero) //animation events cant use functions with bool parameters
    {
        if ( (oneOrZero == 1) || (oneOrZero == 0) )
        {
            isGroundAttacking = oneOrZero;
        }
        else
        {
            Debug.LogError("SetIsGroundAttacking passed a value not one or zero");
        }
    }

    public void SetIsHit(int oneOrZero) //animation events cant use functions with bool parameters
    {
        if ((oneOrZero == 1) || (oneOrZero == 0))
        {
            isHit = oneOrZero;
        }
        else
        {
            Debug.LogError("SetIsHit passed a value not one or zero");
        }
    }

    private void BackAttack()
    {
        //maybe add another time condition to prevent back attack spam
        //check if attack pressed with direction opposite the one facing and is currently in a combo
        //then trigger the back attack and flip the sprite
        if (isGroundAttacking == 1 && Input.GetAxisRaw("Horizontal") == -1 && Input.GetButtonDown("Fire1") && transform.localScale.x == 1)
        {
            transform.localScale = new Vector3(-1,1,1);
            animator.SetTrigger("backAttack");
            DealDamage(attackPoint, attackRange, attackDamage);
        }
        else if (isGroundAttacking == 1 && Input.GetAxisRaw("Horizontal") == 1 && Input.GetButtonDown("Fire1") && transform.localScale.x == -1)
        {
            transform.localScale = new Vector3(1, 1, 1);
            animator.SetTrigger("backAttack");
            DealDamage(attackPoint, attackRange, attackDamage); //maybe can reduce back attack damage so while it's quicker it's better to do normal combos
        }
    }

    private void AirAttack()
    {
        if (Input.GetButtonDown("Fire1") && isJumping && !isAirAttacking)
        {
            isAirAttacking = true;
            animator.SetBool("isAirAttacking", true);

            DealDamage(airAttackPoint, attackRange, attackDamage);
        }
    }

    private void Grab()
    {
        //can add animation for grabbing at nothing if no enemy nearby just for feedback on what the button does
        if (Input.GetButtonDown("Fire2") && !isJumping && isHit == 0 && isGroundAttacking == 0 && !isGrabbing)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            if (hitEnemies.Length > 0) 
            {
                enemyGrabbed = hitEnemies[0].GetComponent<EnemyHealth>(); // only attempt to grab one enemy
                if (enemyGrabbed.IsGrabbable())
                {
                    //position the grabbed enemy in the right place depending on direction the player facing
                    if(transform.localScale.x == 1)
                    {
                        enemyGrabbed.transform.position = new Vector3(gameObject.transform.position.x + enemyGrabbed.grabPositionXOffset,
                                                                  gameObject.transform.position.y, 0);
                    }
                    else
                    {
                        enemyGrabbed.transform.position = new Vector3(gameObject.transform.position.x - enemyGrabbed.grabPositionXOffset,
                                                                  gameObject.transform.position.y, 0);
                    }

                    enemyGrabbedCollider = enemyGrabbed.GetComponent<Collider2D>();
                    enemyGrabbedCollider.enabled = false;
                    isGrabbing = true;
                    animator.SetBool("isGrabbing", true);
                    animator.SetBool("grabbedEnemyIsAlive", true);
                    enemyGrabbed.SetIsGrabbed(true);
                }
            }
        }
    }

    private void GrabAttack()
    {
        if (isGrabbing)
        {
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
            else if(Input.GetButtonDown("Fire3"))
            {
                animator.SetTrigger("throw");
                isGrabbing = false;
                animator.SetBool("isGrabbing", false);
                enemyGrabbed.SetIsGrabbed(false);
                enemyGrabbedCollider.enabled = true;
                //call enemy thrown method that gives enemy velocity in direction player facing, set recovery animation/state, take damage on landing
                //refer to player Jump() and OnLanding() methods for reference on enemy thrown velocity. 
                enemyGrabbed.ThrowEnemy(Mathf.Sign(transform.localScale.x));
            }
        }
    }

    private void DealDamage(Transform pointOfAttack, float range, int damage) //attacks can have diff points, ranges, damage
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(pointOfAttack.position, range, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>().TakeDamage(damage);
        }
    }

    private void Move()
    {
        if (isGroundAttacking == 1 || isHit == 1 || isGrabbing)
        {
            myRigidBody.velocity = new Vector2(0, 0);
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");

        if (!isJumping)
        {
            movement.y = Input.GetAxisRaw("Vertical");
        }
        myRigidBody.velocity = new Vector2(movement.normalized.x * runSpeed, movement.normalized.y * runSpeed);

        bool playerHasSpeed = (Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon) || ((Mathf.Abs(myRigidBody.velocity.y) > Mathf.Epsilon));
        animator.SetBool("isMoving", playerHasSpeed);
    }

    private void Jump()
    {
        //record starting y position and have gravity until that y position is reached

        if (Input.GetButtonDown("Jump") && !isJumping && (movement.y == 0) && (isGroundAttacking == 0)) //perhaps can lose third condition because why movement disabled if isJumping
        {
            //myRigidBody.bodyType = RigidbodyType2D.Dynamic;
            yJumpPosition = transform.position.y;
            isJumping = true;
            myRigidBody.gravityScale = gravityScaleWhenJumping;

            myCapsuleCollider2D.isTrigger = true; // allow player to jump through upper boundary

            Vector2 jumpVelocityToAdd = new Vector2(0f, jumpSpeed);
            myRigidBody.velocity += jumpVelocityToAdd;

            animator.SetBool("isJumping", true);
        }
    }

    private void OnLanding()
    {
        if (transform.position.y <= yJumpPosition)
        {
            isJumping = false;
            myRigidBody.gravityScale = 0;
            yJumpPosition = transform.position.y;
            myCapsuleCollider2D.isTrigger = false;

            animator.SetBool("isJumping", false);
            if (isAirAttacking)
            {
                isAirAttacking = false;
                animator.SetBool("isAirAttacking", false);
            }
        }
    }

    private void FlipSprite()
    {
        bool playerHasHorizontalSpeed = (Mathf.Abs(myRigidBody.velocity.x) > Mathf.Epsilon);
        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidBody.velocity.x), 1f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null || airAttackPoint == null)
        {
            Debug.LogError("No attack point on player");
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawWireSphere(airAttackPoint.position, attackRange);
    }
}
