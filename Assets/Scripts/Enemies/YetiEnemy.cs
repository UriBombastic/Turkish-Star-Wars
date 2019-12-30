using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiEnemy : Enemy
{
    public override void Kill()
    {
        base.Kill();
        PlayDeathSound();
        rb.constraints = RigidbodyConstraints.None;
        Destroy(healthCanvas);
        //  Animate("Die");
        Destroy(animator);
        Destroy(this);

    }

    protected override void HandleDistances()
    {
        base.HandleDistances();
        Vector3 playerPosition = playerTransform.position;
        float distance = Vector3.Distance(transform.position, playerPosition);
        if (distance > ViewRange && distance > AttackRange)
            Animate("Idle");
    }

    protected override void HandlePlayerInView()
    {
        base.HandlePlayerInView();
        Animate("Walk");
    }

    protected override IEnumerator Attack()
    {
        Animate("Attack");
        yield return base.Attack();
        Animate("Idle"); //exit attack state and return to idle
        yield return null;
    }

    protected override void EnterDamage()
    {
        base.EnterDamage();
        Animate("Damage");
    }

    protected override void ExitDamage()
    {
        base.ExitDamage();
        Animate("Idle");
    }
}
