using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiBoss : YetiEnemy
{
    public float minAttackTime = 1.5f;
    public float maxAttackTime = 3.0f;
    public float leapChance = 0.25f;
    public GameObject projectile;
    public Transform projectileTransform;
    public float projectileDamage;
    public float projectileImpactRange;
    public float projectileLaunchForce;

    public float leapForce = 5000f;
    public float leapRange = 10f;
    public float leapDamage = 25f;
    public float leapAttackForce = 2000f;
    public bool isLeaping = false; //sub-state for leap attack
    public GameObject dustParticles;

    public float berzerkHealthCutoff = 300f;
    public float berzerkStrenghtMultiplier = 1.2f;
    public bool isBerzerk = false;
    public GameObject berzerkParticles;
    public float deathTime = 3.0f;

    //for game continuity
    public GameObject[] toggleOnDeath;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(AttackClock());
    }

    protected IEnumerator AttackClock()
    {
        float timeToNextAttack = Random.Range(minAttackTime, maxAttackTime);
        yield return new WaitForSeconds(timeToNextAttack);


        StartCoroutine(Attack());
        yield return new WaitForSeconds(BasicAttackCooldown); //allow time for attack to execute before even thinking of doing another attack
        StartCoroutine(AttackClock()); //begin next attack

    }

    protected override IEnumerator Attack()
    {
       // state_ = State.ATTACKING;
        SelectAttack();
        yield return new WaitForEndOfFrame();
       // state_ = State.ATTACKING;
    }

    private void SelectAttack()
    {
        if(state_ == State.AGGRESSION)
        {
            StartCoroutine(BasicAttack());
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

    IEnumerator BasicAttack() //this attack is literally just the basic yeti attack
    {
        yield return base.Attack();
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

    protected override void HandleAggression()
    {
        FacePlayer();
    }

    protected override IEnumerator HandleDamage(float damage)
    {
        yield return base.HandleDamage(damage);
        if (health <= berzerkHealthCutoff && !isBerzerk)
            EnterBerzerkMode();
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
        PlayDeathSound();
        StopCoroutine(AttackClock());
        ViewRange = 0;
        AttackRange = 0;
        state_ = State.IDLE;
        GameMaster.Instance.RegisterBossDeath();
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

    void ToggleContinuityElements()
    {
        for(int i = 0; i < toggleOnDeath.Length; i++)
            if(toggleOnDeath[i]!=null)
                toggleOnDeath[i].SetActive(!toggleOnDeath[i].activeInHierarchy);

    }
}
