using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

// Class that controls enemy movement and attacks.
public class Enemy : MonoBehaviour
{
    //TODO: Add functionality to recovery state and tie together with being thrown or anything else that should knock down enemies
    //      Visualize stopping distance in the inspector with a differently colored gizmo

    //configuration variables
    [Header("Movement")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] Vector2 roamDistance = new Vector2(1.0f, 3f);

    [Header("Decision times X: Min, Y: Max")]
    [SerializeField] Vector2 attackDecisionTime = new Vector2(1.0f, 1.5f); //original 1.0f, 1.5f
    [SerializeField] Vector2 waitDecisionTime = new Vector2(0.2f, 0.5f); //original 0.2f, 0.5f
    [SerializeField] Vector2 roamDecisionTime = new Vector2(.3f, .6f); //original 0.3f, 0.6f
    [SerializeField] Vector2 chaseDecisionTime = new Vector2(.2f, .4f); //original: 0.2f, 0.4f

    [Header("Targeting and Attacking")]
    [SerializeField] protected LayerMask playerLayers;
    [SerializeField] protected Transform attackPoint;

    [Tooltip("The distance in which the enemy will begin to target and chase after the player")]
    [SerializeField] float targetRange = 100f;
    [SerializeField] float stoppingDistance = .2f; //stopping distance is separate from attack range in case I want enemies to attack from farther away?

    [Tooltip("Radius in which enemy attacks will deal damage")]
    [SerializeField] protected float damageRange = .5f;

    [Tooltip("Radius where enemy will begin attacking")]
    [SerializeField] float attackRange = 1f;
    [SerializeField] int attackDamage = 20; //Attack damage configured within animation event

    [Tooltip("# Attacks per second")]
    [SerializeField] float attackRate = 1f;

    //state variables
    Player currentTarget;
    Health currentTargetHealth;
    float nextAttackTime = 0f;

    float decisionDuration;
    Vector3 roamDestination;

    int isHit = 0;
    int isGroundAttacking = 0;

    bool isChasingPlayer = false;
    bool isRoaming = false;
    bool canMakeDecision = true;

    //cached references
    Animator animator;
    EnemyHealth enemyHealth;
    Rigidbody2D rb;

    public enum EnemyState
    {
        waiting,
        recovering,
        roaming,
        chasing,
        attacking,
    }

    EnemyState currentState;

    public class DecisionWeight
    {
        public int weight;
        public EnemyState state;
        public DecisionWeight(int weight, EnemyState action)
        {
            this.weight = weight;
            state = action;
        }
    }

    List<DecisionWeight> weights;

    // Start is called before the first frame update
    void Start()
    {
        weights = new List<DecisionWeight>();
        animator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        //currentTarget = FindObjectOfType<Player>().transform;
        currentTarget = FindObjectOfType<Player>();
        currentTargetHealth = currentTarget.GetComponent<Health>();
        //For multiplayer try: find ObjectsOfType<Player>().transform and assign to transform array, make current target a random player
        //change target if enemy hit by different player within the take damage method
    }



    private void Update()
    {
        UpdateState();
        FlipSprite();
        //Debug.Log("decision duration = " + decisionDuration);
        HandleDecisions();
        ChasePlayer();
        Roaming();

        //StopSliding();
    }



    private void StopSliding()
    // I think the sliding issue was solved with the new knockdown recovery method
    {
        if (rb.velocity.magnitude >= Mathf.Epsilon && !enemyHealth.GetIsBeingThrown())
        {
            rb.velocity = new Vector2(0, 0);
        }
    }



    private void HandleDecisions()
    // Keeps enemy in current state until the time they can make a decision again. 
    {
        if (decisionDuration > 0.0f)
        {
            decisionDuration -= Time.deltaTime;
            canMakeDecision = false;
        }
        else
        {
            isRoaming = false;
            isChasingPlayer = false;
            canMakeDecision = true;
        }
    }



    private void UpdateState()
    // Decides which weights to give different states based on whether the enemy is within attack range or not, then calls decision method. 
    {
        if (isHit == 1 || isGroundAttacking == 1 || !enemyHealth.IsRecovered()) //simply take out isHit condition for enemies w/out stagger
        {
            animator.SetBool("isChasing", false);
            isRoaming = false;
            isChasingPlayer = false;
            return;
        }

        //TODO? 
        /*if (!enemyHealth.IsRecovered())
        {
            currentState = EnemyState.recovering;
            DoStateAction();
        }*/

        if (currentTarget.enabled == false)
        {
            currentState = EnemyState.waiting;
            DoStateAction();
            // Find new target? Call update target method?

            return;
        }

        else if (canMakeDecision)
        {
            //for reference: DecideWithWeights(int attack, int wait, int chase, int roam)

            if (!currentTargetHealth.GetIsAlive())
            // Current Target died
            // TODO change to retargeting when working on multiplayer
            {
                currentState = EnemyState.waiting;
                DoStateAction();
            }
            // If player is within targeting range but outside attack range, or player is knocked down: chase, wait, or roam. 
            else if ((Vector2.Distance(transform.position, currentTarget.transform.position) > attackRange) && 
                (Vector2.Distance(transform.position, currentTarget.transform.position) > stoppingDistance) && //stop enemy from wanting to travel into player
                (Vector2.Distance(transform.position, currentTarget.transform.position) < targetRange) ||
                 currentTarget.GetIsKnockedback())
            {
                DecideWithWeights(0, 10, 70, 10);
                DoStateAction();
            }
            else if (Vector2.Distance(transform.position, currentTarget.transform.position) <= attackRange)
            {
                DecideWithWeights(80, 10, 0, 10); //attack, wait, or roam. Default: (70, 15, 0, 15)
                DoStateAction();
            }
            else
            // No target in range 
            {
                currentState = EnemyState.waiting;
                DoStateAction();
            }
        }
    }



    private void DecideWithWeights(int attackWeight, int waitWeight, int chaseWeight, int roamWeight)
    // The way this works is that each enemy state has an associated weight. Each weight gets added together. A random value is then selected between 0 and the total.
    // Each state then subtracts its weight from that value. Whichever state gets the decision value to zero becomes the current state. The higher the weight, the more
    // likely that state will become the new enemy state. 
    {
        weights.Clear();
        //Debug.Log("Making decision");
        if (attackWeight > 0)
            weights.Add(new DecisionWeight(attackWeight, EnemyState.attacking));
        if (chaseWeight > 0)
            weights.Add(new DecisionWeight(chaseWeight, EnemyState.chasing));
        if (waitWeight > 0)
            weights.Add(new DecisionWeight(waitWeight, EnemyState.waiting));
        if (roamWeight > 0)
            weights.Add(new DecisionWeight(roamWeight, EnemyState.roaming));

        int total = attackWeight + chaseWeight + waitWeight + roamWeight;
        int intDecision = UnityEngine.Random.Range(0, total - 1);

        foreach (DecisionWeight weight in weights)
        {
            intDecision -= weight.weight;
            if (intDecision <= 0)
            {
                currentState = weight.state;
                //Debug.Log("Current state = " + currentState);
                break;
            }
        }
    }



    private void DoStateAction()
    // Call the method associated with the current state. 
    {
        switch (currentState)
        {
            case EnemyState.chasing:
                Chase();
                break;
            case EnemyState.roaming:
                Roam();
                break;
            case EnemyState.attacking:
                Attack();
                break;
            case EnemyState.waiting:
                Wait();
                break;
            /*case EnemyState.recovering:
                Recover();
                break;*/
            default:
                break;
        }
    }



    /*private void Recover()
    {

    }*/



    private void Wait()
    // Simply reset the decision time and do nothing. 
    {
        decisionDuration = UnityEngine.Random.Range(waitDecisionTime.x, waitDecisionTime.y);
        animator.SetBool("isChasing", false);
    }



    private void Chase()
    // Changes player state to chasing. Movement handled by ChasePlayer() method as it needs to be continuously called in Update(). 
    {
        isChasingPlayer = true;
        decisionDuration = UnityEngine.Random.Range(chaseDecisionTime.x, chaseDecisionTime.y);
    }



    private void ChasePlayer()
    // Moves enemy towards the player. 
    {
        if (isChasingPlayer)
        {
            animator.SetBool("isChasing", true);
            transform.position = Vector2.MoveTowards(transform.position, currentTarget.transform.position, moveSpeed * Time.deltaTime);
        }
    }
    


    private void Roam()
    // Decides location for Enemy to roam to in the Roaming() method. 
    {
        isRoaming = true;
        float randomDegree = UnityEngine.Random.Range(0, 360);
        Vector2 offset = new Vector2(Mathf.Sin(randomDegree), Mathf.Cos(randomDegree));
        float distance = UnityEngine.Random.Range(roamDistance.x, roamDistance.y);
        offset *= distance;

        Vector3 directionVector = new Vector3(offset.x, offset.y, 0);
        roamDestination = transform.position + directionVector;

        decisionDuration = UnityEngine.Random.Range(roamDecisionTime.x, roamDecisionTime.y);
    }



    private void Roaming()
    // Moves enemy towards the random destination decided in the Roam() method. 
    {
        if (isRoaming)
        {
            animator.SetBool("isChasing", true);

            transform.position = Vector2.MoveTowards(transform.position, roamDestination, moveSpeed * Time.deltaTime);
        }
    }



    public virtual void Attack()
    // Trigger attack animation if enemy is able to attack again. 
    {
        animator.SetBool("isChasing", false);

        if (Time.time >= nextAttackTime)
        {
            animator.SetTrigger("attack"); 
            nextAttackTime = Time.time + (1f / attackRate);
        }

        decisionDuration = UnityEngine.Random.Range(attackDecisionTime.x, attackDecisionTime.y);
    }



    public virtual void EnemyDealDamage(int damageToDeal)
    // Called through animation event during attack animation. 
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, damageRange, playerLayers);

        foreach (Collider2D player in hitPlayers) 
        {
            //possibly time consuming, possible to cache these references when dealing with unknown number of players?
            player.GetComponent<Health>().LoseHealth(damageToDeal);
            player.GetComponent<Player>().ProcessHit();
        }
    }



    public void SetIsHit(int isHit)
    // Sets isHit state variable from hit animation. Animation events cant use functions with bool parameters. 
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



    public void SetIsGroundAttacking(int isGroundAttacking)
    // Sets isGroundAttacking state variable from hit animation. Animation events cant use functions with bool parameters. 
    {
        if ((isGroundAttacking == 1) || (isGroundAttacking == 0))
        {
            this.isGroundAttacking = isGroundAttacking;
        }
        else
        {
            Debug.LogError("SetIsGroundAttacking passed a value not one or zero");
        }
    }



    public Player GetCurrentTarget()
    {
        return currentTarget;
    }



    private void FlipSprite()
    // Changes the x value based on the direction the player is moving ('+' = right, '-' = left)
    {
        if ((transform.position.x - currentTarget.transform.position.x) >= Mathf.Epsilon)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }



    private void OnDrawGizmosSelected()
    // Visualizes damage and targeting ranges in the inspector for easier adjusting in the editor
    {
        Gizmos.DrawWireSphere(transform.position, targetRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, damageRange);
    }
}
