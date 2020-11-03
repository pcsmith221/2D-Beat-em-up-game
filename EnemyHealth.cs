using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Add serialized sound/visual effects to use on take damage 
public class EnemyHealth : MonoBehaviour
{
    //configuration 
    [SerializeField] int maxHealth = 100;
    [SerializeField] float gravityScaleWhenThrown = 1.5f;
    [SerializeField] float recoveryTime = 2f; 
    [SerializeField] bool isGrabbable = true;
    [SerializeField] bool staggerOnHit = true;
    [SerializeField] int hitGroundDamage = 10;
    public float grabPositionXOffset = 1f;
    public Vector2 distanceThrownByPlayer = new Vector2(25f, 25f);

    //state
    int currentHealth;
    bool isGrabbed = false;
    bool isBeingThrown = false;
    float yThrownPosition;
    float timeCanMoveAgain = 0f;
    
    //cache
    Animator animator;
    Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isBeingThrown)
        {
            OnLanding();
        }
    }

    public void ThrowEnemy(float signOfX)
    {
        animator.SetBool("isGrabbed", false);
        animator.SetBool("isThrown", true);
        isBeingThrown = true;

        yThrownPosition = transform.position.y;

        rb.gravityScale = gravityScaleWhenThrown;
        rb.velocity = new Vector2(distanceThrownByPlayer.x * signOfX, distanceThrownByPlayer.y);
    }

    private void OnLanding()
    {
        if (transform.position.y <= yThrownPosition)
        {
            timeCanMoveAgain = Time.time + recoveryTime;
            animator.SetBool("isThrown", false);
            isBeingThrown = false;

            rb.gravityScale = 0;
            rb.velocity = new Vector2(0, 0);
            TakePhysicsDamage(hitGroundDamage);
        }
    }

    public bool IsRecovered()
    {
        if(Time.time >= timeCanMoveAgain)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetIsGrabbed(bool grab)
    {
        isGrabbed = grab;
        if (isGrabbed)
        {
            animator.SetBool("isGrabbed", true);
        }
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if(staggerOnHit)
        {
            animator.SetTrigger("hit");
        }
        
        if (currentHealth <= Mathf.Epsilon)
        {
            Die();
        }
    }

    private void TakePhysicsDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= Mathf.Epsilon)
        {
            Die();
        }
    }

    private void Die()
    {
        //Debug.Log(name + " died");

        animator.SetBool("isDead", true);

        if (isGrabbed)
        {
            animator.SetBool("isGrabbed", false);
        }

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
}
