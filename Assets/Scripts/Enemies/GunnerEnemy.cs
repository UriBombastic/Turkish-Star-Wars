using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunnerEnemy : GenericBoss
{
    [Header("Gunner Robot")]
    public int maxConsecutiveAttacks = 4;
    public int currentConsecutiveAttacks = 0;
    public float restTime = 3.0f;
    public GameObject restIndicator;

    [Header("Proximity Attack")]
    public GameObject proximityParticles;
    public GameObject proximityTelegraph;
    public float proximityStartup = 0.75f;
    public float proximityDamage = 15;
    public float proximityRadius = 4.0f;
    public float proximityForce = 1350;
    public float proximityAttackDuration = 1.0f;

    [Header("Projectile")]
    public int projectileCount = 4;
    public GameObject projectile;
    public Transform projectileTransform;
    public float projectileDamage = 10f;
    public float projectileDelay = 0.5f;
    public float projectileImpactRange = 3;
    public float projectileLaunchForce = 250;


    protected override void SelectAttack()
    {
        if(currentConsecutiveAttacks < maxConsecutiveAttacks && state_ != State.DAMAGED)
        {
         
            // If player is coming up too close, automatically unleash slam attack
            if(state_==State.AGGRESSION)
            {
                StartCoroutine(ProximityAttackSequence());
            }
            // Fire
            else if (state_ == State.PLAYERINVIEW)
            {
                StartCoroutine(ProjectileSequence(projectile, projectileCount));
            }
            currentConsecutiveAttacks++;
        }
        else
        {
            StartCoroutine(RestSequence());
        }
    }


    IEnumerator ProximityAttackSequence()
    {
        state_ = State.ATTACKING;
        StartCoroutine(TelegraphAttack(proximityTelegraph, proximityStartup));
        yield return new WaitForSeconds(proximityStartup);
        Instantiate(proximityParticles, transform);
        FundamentalAttack(proximityDamage, proximityRadius, proximityForce, transform);
        yield return new WaitForSeconds(proximityAttackDuration);
        state_ = State.IDLE;
    }

    IEnumerator ProjectileSequence(GameObject projectile, int projectileCount)
    {
        state_ = State.ATTACKING;
        StartCoroutine(TelegraphAttack());
        yield return new WaitForSeconds(TelegraphDelay);
        for(int i = 0; i < projectileCount; i++)
        {
            GameObject instantiatedProjectile = Instantiate(projectile, projectileTransform.position, projectileTransform.rotation);
            instantiatedProjectile.GetComponent<RockProjectile>().InstantiateProjectile(this, projectileDamage, projectileImpactRange, 0, true);
            instantiatedProjectile.GetComponent<Rigidbody>().AddForce(projectileTransform.forward * projectileLaunchForce);
            yield return new WaitForSeconds(projectileDelay);
        }
        state_ = State.IDLE;
        yield return null;

    }

    IEnumerator RestSequence()
    {
        StartCoroutine(base.GenericRestSequence(restIndicator, restTime));
        currentConsecutiveAttacks = 0;
        yield return null;
    }

}
