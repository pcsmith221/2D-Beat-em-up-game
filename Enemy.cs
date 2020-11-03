using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //TODO: Add functionality to recovery state and tie together with being thrown or anything else that should knock down enemies
    //      Visualize stopping distance in the expector with a differently colored gizmo

    //configuration variables
    [SerializeField] float moveSpeed = 2f;

    [Header("Decision times X: Min, Y: Max")]
    [SerializeField] Vector2 attackDecisionTime = new Vector2(1.0f, 1.5f); //original 1.0f, 1.5f
    [SerializeField] Vector2 waitDecisionTime = new Vector2(0.2f, 0.5f); //original 0.2f, 0.5f
    [SerializeField] Vector2 roamDecisionTime = new Vector2(.3f, .6f); //original 0.3f, 0.6f
    [SerializeField] Vector2 chaseDecisionTime = new Vector2(.2f, .4f); //original: 0.2f, 0.4f

    [Header("Targeting and Attacking")]
    [SerializeField] LayerMask playerLayers;
    [SerializeField] Transform attackPoint;
    [SerializeField] float targetRange = 100f;
    [SerializeField] float stoppingDistance = .2f;
    [SerializeField] float damageRange = .5f;
    [SerializeField] float attackRange = 1f;
    [SerializeField] int attackDamage = 20; //Attack damage configured within animation event
    [Tooltip("# Attacks per second")]
    [SerializeField] float attackRate = 1f;

    //state variables
    Transform currentTarget;
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
    EnemyHealth myHealth;
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
        myHealth = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        currentTarget = FindObjectOfType<Player>().transform;
        //For multiplayer try: find ObjectsOfType<Player>().transform and assign to transform array, make current target a random player
        //change target if enemy hit by different player within the take damage method
    }

    private void Update()
    {
        StopSliding();
        UpdateState();
        FlipSprite();
        //Debug.Log("decision duration = " + decisionDuration);
        HandleDecisions();
        ChasePlayer();
        Roaming();
    }

    private void StopSliding()
    {
        if (rb.velocity.magnitude >= Mathf.Epsilon && !myHealth.GetIsBeingThrown())
        {
            rb.velocity = new Vector2(0, 0);
        }
    }

    private void HandleDecisions()
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
    {
        if (isHit == 1 || isGroundAttacking == 1 || myHealth.GetIsGrabbed() || myHealth.GetIsBeingThrown() || !myHealth.IsRecovered()) //simply take out isHit condition for enemies w/out stagger
        {
            animator.SetBool("isChasing", false);
            isRoaming = false;
            isChasingPlayer = false;
            //currentState = EnemyState.recovering;
            //DoStateAction();
        }

        else if (canMakeDecision)
        {
            //for reference: DecideWithWeights(int attack, int wait, int chase, int roam

            if ((Vector2.Distance(transform.position, currentTarget.position) > attackRange) && 
                (Vector2.Distance(transform.position, currentTarget.position) > stoppingDistance) && //stop enemy from wanting to travel into player
                (Vector2.Distance(transform.position, currentTarget.position) < targetRange))
            {
                DecideWithWeights(0, 20, 80, 0); //chase or wait
                DoStateAction();
            }
            else if (Vector2.Distance(transform.position, currentTarget.position) <= attackRange)
            {
                DecideWithWeights(80, 10, 0, 10); //attack, wait, or roam. Default: (70, 15, 0, 15)
                DoStateAction();
            }
            else
            {
                currentState = EnemyState.waiting;
                DoStateAction();
            }
        }
    }

    private void DecideWithWeights(int attack, int wait, int chase, int roam)
    {
        weights.Clear();
        //Debug.Log("Making decision");
        if (attack > 0)
            weights.Add(new DecisionWeight(attack, EnemyState.attacking));
        if (chase > 0)
            weights.Add(new DecisionWeight(chase, EnemyState.chasing));
        if (wait > 0)
            weights.Add(new DecisionWeight(wait, EnemyState.waiting));
        if (roam > 0)
            weights.Add(new DecisionWeight(roam, EnemyState.roaming));

        int total = attack + chase + wait + roam;
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
            case EnemyState.recovering:
                break;
            default:
                break;
        }
    }

    private void Wait()
    {
        decisionDuration = UnityEngine.Random.Range(waitDecisionTime.x, waitDecisionTime.y);
        animator.SetBool("isChasing", false);
    }

    private void Attack()
    {
        animator.SetBool("isChasing", false);

        if (Time.time >= nextAttackTime)
        {
            animator.SetTrigger("attack"); //deal dmg called at end of attack animation
            nextAttackTime = Time.time + (1f / attackRate);
        }

        decisionDuration = UnityEngine.Random.Range(attackDecisionTime.x, attackDecisionTime.y);
    }

    private void ChasePlayer()
    {
        if (isChasingPlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
            animator.SetBool("isChasing", true);
        }
    }
    private void Chase()
    {
        isChasingPlayer = true;
        decisionDuration = UnityEngine.Random.Range(chaseDecisionTime.x, chaseDecisionTime.y);
    }

    private void Roaming()
    {
        if (isRoaming)
        {
            animator.SetBool("isChasing", true);
            
            transform.position = Vector2.MoveTowards(transform.position, roamDestination, moveSpeed * Time.deltaTime);
        }
    }

    private void Roam()
    {
        isRoaming = true;
        float randomDegree = UnityEngine.Random.Range(0, 360);
        Vector2 offset = new Vector2(Mathf.Sin(randomDegree), Mathf.Cos(randomDegree));
        float distance = UnityEngine.Random.Range(1, 3);
        offset *= distance;

        Vector3 directionVector = new Vector3(offset.x, offset.y, 0);
        roamDestination = transform.position + directionVector;

        decisionDuration = UnityEngine.Random.Range(roamDecisionTime.x, roamDecisionTime.y);
    }

    public void EnemyDealDamage(int damageToDeal) //call with animation event
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, damageRange, playerLayers);

        foreach (Collider2D player in hitPlayers) //Also will hit feet collider, may need to revise how staying in boundary
        {
            //possibly time consuming, possible to cache these references when dealing with unknown number of players?
            player.GetComponent<Health>().LoseHealth(damageToDeal);
            player.GetComponent<Player>().ProcessHit();
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

    public void SetIsGroundAttacking(int oneOrZero) //animation events cant use functions with bool parameters
    {
        if ((oneOrZero == 1) || (oneOrZero == 0))
        {
            isGroundAttacking = oneOrZero;
        }
        else
        {
            Debug.LogError("SetIsGroundAttacking passed a value not one or zero");
        }
    }

    private void FlipSprite()
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
    {
        Gizmos.DrawWireSphere(transform.position, targetRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, damageRange);
    }
}
