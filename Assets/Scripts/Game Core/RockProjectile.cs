using UnityEngine;

public class RockProjectile : Projectile
{

    public GameObject explosionParticles;
    public float maxForceMultiplier = 1.25f;

    public override void OnCollisionEnter(Collision other)
    {
        if (!enabled) return;
        float trueMultiplier = (doSpecial) ? maxForceMultiplier : 1f;
        owner.FundamentalAttack(damageToDo * trueMultiplier, radius * trueMultiplier,
            attackForce * trueMultiplier, transform);

        if (doSpecial)
        {
            Instantiate(explosionParticles, transform.position, transform.rotation); //explosion effect
            Destroy(gameObject);
        }
        enabled = false;
    }

}
