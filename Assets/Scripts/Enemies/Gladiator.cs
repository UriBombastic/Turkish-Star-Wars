using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gladiator : Enemy, IDamageable
{

    protected override void Start()
    {
        base.Start();
        DoIndiscriminateAttack = true;
    }

    protected override void HandleDistances()
    {
        base.HandleDistances();
        Vector3 targetPosition = targetTransform.position;
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > ViewRange)
            IdentifyTarget();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!targetTransform.GetComponent<HeroController>() && !targetTransform.GetComponent<Gladiator>())
            IdentifyTarget();
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
        if (potentialTargets.Count == 0) return;
        targetTransform = potentialTargets[Random.Range(0, potentialTargets.Count)];
    }

    public override void Kill()
    {
        base.Kill();
        PlayDeathSound();
        rb.constraints = RigidbodyConstraints.None;
        Destroy(healthCanvas);
        Destroy(this);
    }

    protected override IEnumerator Attack()
    {
        yield return new WaitForSeconds(Random.Range(0f, BasicAttackStartup));
       yield return base.Attack();
        yield return null;
    }
}
