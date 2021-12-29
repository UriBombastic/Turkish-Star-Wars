using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Note: has been refactored to derive from genericboss instead of Yeti enemy.
public class YetiBoss : GenericBoss
{
    [Header("Projectile")]
    public GameObject projectile;
    public Transform projectileTransform;
    public float projectileDamage;
    public float projectileImpactRange;
    public float projectileLaunchForce;

    [Header("Leaping")]
    public float leapChance = 0.25f;
    public float leapForce = 5000f;
    public float leapRange = 10f;
    public float leapDamage = 25f;
    public float leapAttackForce = 2000f;
    public bool isLeaping = false; //sub-state for leap attack
    public GameObject dustParticles;

    [Header("Berzerk")]
    public float berzerkHealthCutoff = 300f;
    public float berzerkStrenghtMultiplier = 1.2f;
    public bool isBerzerk = false;
    public GameObject berzerkParticles;
    public float deathTime = 3.0f;

    [Header("Defense")]
    public float CanStunCutoff = 300f;
    public float DamageToStun = 50f;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(AttackClock());
    }


    protected override void SelectAttack()
    {
        if(state_ == State.AGGRESSION)
        {
            StartCoroutine(Attack());
        }
        else if(state_== State.PLAYERINVIEW)
        {
            float selection = Random.Range(0f, 1f);
            if(selection > leapChance)
            {
                StartCoroutine(ProjectileAttack());
            }
            else
            {
                StartCoroutine(Leap());
            }
        }
    }

    IEnumerator ProjectileAttack()
    {
        state_ = State.ATTACKING;
        Animate("Attack");
        yield return new WaitForSeconds(BasicAttackStartup / 2);
        GameObject instantiatedProjectile = Instantiate(projectile, projectileTransform.position, projectileTransform.rotation);
        instantiatedProjectile.GetComponent<RockProjectile>().InstantiateProjectile(this, projectileDamage, projectileImpactRange, 0);
        instantiatedProjectile.GetComponent<Rigidbody>().AddForce(GetAimAngle() * projectileLaunchForce);
        state_ = State.IDLE;
        yield return new WaitForEndOfFrame();

    }

    IEnumerator Leap()
    {
        state_ = State.ATTACKING;
        Vector3 upwardsVector = transform.up * leapForce;
        rb.AddForce(upwardsVector); //leap upwards
        yield return new WaitForSeconds(BasicAttackStartup); //after having gone up...
     
        rb.AddForce(GetAimAngle() * leapForce*2);//leap towards player, midair
        isLeaping = true;
    }



    public void OnCollisionEnter()
    {
        if (state_ == State.ATTACKING && isLeaping) //check if is leaping, with reduncancy of confirming state is attack state
        {
            state_ = State.IDLE;
            isLeaping = false;
            StartCoroutine(LeapAttack());
        }

    }

    IEnumerator LeapAttack()
    {
        Instantiate(dustParticles, transform.position, transform.rotation);
        FundamentalAttack(leapDamage, leapRange, leapAttackForce, transform); //powerful, shorter range hitbox. Simulating direct impact.
        yield return new WaitForEndOfFrame(); //ensure first impact is prioritized
        FundamentalAttack(leapDamage/2, leapRange, leapAttackForce/2, transform); //bigger hitbox but less damage, simulating shockwave.
        //due to the way player handles damage: forcing a brief break before taking damage again, 
        //it is theoretically impossible that both of these hitboxes damage the player.
        yield return new WaitForEndOfFrame();
    }

    // Ripped from original Yeti enemy
    protected override void HandlePlayerInView()
    {
        base.HandlePlayerInView();
        Animate("Walk");
    }

    protected override void HandleAggression()
    {
        FacePlayer();
    }

    protected override IEnumerator HandleDamage(float damage)
    {
        if (state_ != State.DEAD)
        {
            //only stun if above berserker cutoff or large enough to override
            if (health >= CanStunCutoff || damage >= DamageToStun)
            {
                yield return base.HandleDamage(damage);
            }
            if (health <= berzerkHealthCutoff && !isBerzerk)
                EnterBerzerkMode();
        }
        yield return null;
    }

    void EnterBerzerkMode()
    {
        isBerzerk = true;
     //   PlayDeathSound(); //fearsome roar
        BasicAttackDamage *= berzerkStrenghtMultiplier;
        BasicAttackForce *= berzerkStrenghtMultiplier;
        MoveForce *= berzerkStrenghtMultiplier;
        leapForce *= berzerkStrenghtMultiplier;
        leapAttackForce *= berzerkStrenghtMultiplier;
        leapDamage *= berzerkStrenghtMultiplier;
        projectileDamage *= berzerkStrenghtMultiplier;
        projectileLaunchForce *= berzerkStrenghtMultiplier;
        minAttackTime /= berzerkStrenghtMultiplier; //attack faster
        maxAttackTime /= berzerkStrenghtMultiplier;
        berzerkParticles.SetActive(true);

    }

    public override void Kill()
    {
        state_ = State.DEAD;
        PlayDeathSound();
        StopCoroutine(AttackClock());
        ViewRange = 0;
        AttackRange = 0;
        state_ = State.IDLE;
        Animate("Die");
        StartCoroutine(DeathExplosion());
    }

    IEnumerator DeathExplosion()
    {
        yield return new WaitForSeconds(3.0f);
        GameObject BigExplosion = Instantiate(dustParticles, transform.position, transform.rotation);
        BigExplosion.transform.localScale *= 3;
        ToggleContinuityElements();
        Destroy(gameObject);
    }

    // Fuck it, override to revert to original attack telegraph
    protected override IEnumerator TelegraphAttack()
    {
        // Once attack is called, wait the startup time minus the telegraph delay.
        // TelegraphDelay seconds before attacking, telegraph the attack.
        float telegraphDelay = Mathf.Max(0, BasicAttackStartup - TelegraphDelay);
        yield return new WaitForSeconds(telegraphDelay);
        if (attackParticles != null && doSpawnAttackParticles)
            Instantiate(attackParticles, attackTransform);
    }
}
