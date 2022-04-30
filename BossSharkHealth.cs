using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSharkHealth : EnemyHealth
{

    public override void Die()
    {
        base.Die();
        FindObjectOfType<GameManager>().BossDefeated();
    }


    public override void Knockback()
    // Big shark enemies cannot be knocked back
    {
        return;
    }
}
