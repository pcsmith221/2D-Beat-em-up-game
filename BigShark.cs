using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigShark : Enemy
    // inherits from Enemy class to override deal damage method
{
    public override void EnemyDealDamage(int damageToDeal)
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, damageRange, playerLayers);

        foreach (Collider2D player in hitPlayers)
        {
            //possibly time consuming, possible to cache these references when dealing with unknown number of players?
            player.GetComponent<Health>().LoseHealth(damageToDeal);
            player.GetComponent<Player>().ProcessHit();
            player.GetComponent<Player>().Knockback(Mathf.Sign(transform.localScale.x));
        }
    }
}
