using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringer : Enemy
{
    public override void FlipSprite()
    {
        base.ReverseFlipSprite();
    }
}
