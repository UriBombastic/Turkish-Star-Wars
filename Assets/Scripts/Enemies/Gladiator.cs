using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gladiator : Enemy, IDamageable
{
    public Transform animatedTransform;
    protected override void Start()
    {
        base.Start();
        DoIndiscriminateAttack = true;
    }

    protected override void HandleDistances()
    {
        base.HandleDistances();
        if (targetTransform != null)
        {
            Vector3 targetPosition = targetTransform.position;
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance > ViewRange)
                IdentifyTarget();
        }
        else
            IdentifyTarget();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (targetTransform == null || (!targetTransform.GetComponent<HeroController>() && !targetTransform.GetComponent<Gladiator>()))
            IdentifyTarget();

        animatedTransform.rotation = transform.rotation;
    }
    protected override void IdentifyTarget()
    {
        potentialTargets = new List<Transform>();
        Gladiator [] otherGladiators = FindObjectsOfType<Gladiator>();
        for(int i = 0; i < otherGladiators.Length;i ++)
        {

            if (otherGladiators[i] != this)
            {
                float targetDistance = (otherGladiators[i].transform.position - transform.position).magnitude;
                if(targetDistance < ViewRange)
                    potentialTargets.Add(otherGladiators[i].transform);
            }
        }
        float playerDistance = (player.transform.position - transform.position).magnitude;
        if (playerDistance < ViewRange)
            potentialTargets.Add(player.transform);
        if (potentialTargets.Count == 0)
        {
            Animate("Idle");
            return;
        }

        targetTransform = potentialTargets[Random.Range(0, potentialTargets.Count)];
    }

    public override void Kill()
    {
        base.Kill();
        PlayDeathSound();
        rb.constraints = RigidbodyConstraints.None;
        Destroy(healthCanvas);
        Animate("Die");
        Destroy(this);
    }

    protected override IEnumerator Attack()
    {
        //randomly select attack
        int attackNumber = Random.Range(1, 4);
        Animate("Attack" + attackNumber);
        yield return new WaitForSeconds(Random.Range(0f, BasicAttackStartup));
       yield return base.Attack();
        Animate("Idle");
        yield return null;
    }

    protected override void EnterDamage()
    {
        base.EnterDamage();
        int damageNumber = Random.Range(1, 3);
        Animate("Damage"+damageNumber);
    }

    protected override void ExitDamage()
    {
        base.ExitDamage();
        Animate("Idle");
    }

    protected override void HandlePlayerInView()
    {
        base.HandlePlayerInView();
        Animate("Run");
    }
}
