using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiEnemy : Enemy
{
    public float CanStunCutoff = 100f;
    public float DamageToStun = 25f;

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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        //target neutralized
        if (targetTransform == null || (!targetTransform.GetComponent<HeroController>() && !targetTransform.GetComponent<Bystander>()))
            IdentifyTarget();

        //Debug.Log(state_);
    }

    protected override void HandleDistances()
    {
        base.HandleDistances();

        Vector3 playerPosition = targetTransform.position;
        float distance = Vector3.Distance(transform.position, playerPosition);
            
        if (distance > ViewRange)
        {
           if(distance > AttackRange)
            Animate("Idle");
            IdentifyTarget();
        }
    }

    protected override void HandlePlayerInView()
    {
        base.HandlePlayerInView();
        Animate("Walk");
    }

    protected override IEnumerator Attack()
    {
        Debug.Log("Beggining Attack Animation");
        Animate("Attack");
        yield return base.Attack();
        Animate("Idle"); //exit attack state and return to idle
        yield return null;
    }


    protected override IEnumerator HandleDamage(float damage)
    {
        //only stun if above berserker cutoff or large enough to override
        if (health >= CanStunCutoff || damage >= DamageToStun) 
        {
            yield return base.HandleDamage(damage);
        }
        targetTransform = playerTransform; //automatically target player when damaged
    }


    protected override void EnterDamage()
    {
   
      //  if (health < CanStunCutoff || state_ == State.ATTACKING) return; //do not stun lol
        base.EnterDamage();
        Animate("Damage");

    }

    protected override void ExitDamage()
    {
        //if (health > CanStunCutoff)
       // { 
            base.ExitDamage();
            Animate("Idle");
       // }

    }

    protected override void IdentifyTarget()
    {
        Bystander[] bystanders = FindObjectsOfType<Bystander>();

        if(bystanders.Length==0)
        {
            base.IdentifyTarget(); //target transform is player transform
            return;
        }
        potentialTargets = new List<Transform>();
        potentialTargets.Add(playerTransform); //add player, of course
        //add bystanders to potential targets
        for (int i = 0; i < bystanders.Length; i++)
        {
            float targetDistance = (bystanders[i].transform.position - transform.position).magnitude;
            if(targetDistance < ViewRange)
                potentialTargets.Add(bystanders[i].transform);
        }

        int randomSelection = Random.Range(0, potentialTargets.Count);

        targetTransform = potentialTargets[randomSelection];
        Debug.Log(targetTransform.name);
    }
}
