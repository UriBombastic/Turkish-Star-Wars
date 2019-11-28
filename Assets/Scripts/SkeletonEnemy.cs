using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonEnemy : Enemy
{
    public override void Kill()
    {
        PlayDeathSound();
        rb.constraints = RigidbodyConstraints.None;
        Destroy(healthCanvas);
        Destroy(this);
    }
}
