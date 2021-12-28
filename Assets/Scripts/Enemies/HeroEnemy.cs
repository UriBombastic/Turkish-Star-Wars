using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEnemy : GenericBoss
{
    public enum AttackState
    {
        NONE,
        BASIC,
        DASH,
        SHIELD,
        PROJECTILE,
        SLAM
    }

    [Header("HeroEnemy Essentials")]
    public bool doStealPlayerStats = true;
    public AttackState attackState;
    public float healthMult = 2.0f;

    [Header("Visual Elements")]
    public GameObject shieldIndicator;
    public GameObject attackSword;
    public float AttackRotateSpeed = 10f;


    [Header("Dash Attack")]
    public float dashAttackDamage = 30f;
    public float dashAttackForce = 600f;
    public float dashAttackRadius = 5f;
    public float dashForce = 1250f;
    public float dashStartup = 1.0f;
    // public float dashChance = 0.5f;
    public float dashDuration = 2.0f;

    [Header("Counter")]
    public float counterDamage = 5f;
    public float counterForce = 700;
    public float counterRadius = 4;
    public float shieldMinTime = 0.25f;
    public float shieldTime = 0.0f;

    [Header("Projectile")]
    public float projectileChance = 0.2f;
    public GameObject projectile;
    public float projectileDamage = 20f;
    public float projectileStartup = 0.5f;
    public float projectileImpactRange = 3.0f;
    public float projectileAttackDuration = 1.0f;
    public float projectileDisplacement = 2.5f;

    [Header("Slam Attack")]
    public float slamChance= 0.2f;
    public GameObject slamParticles;
    public GameObject slamTelegraph;
    public float slamStartup = 0.75f;
    public float slamDamage = 30;
    public float slamRadius = 5.0f;
    public float slamForce = 1350;
    public float slamAttackDuration = 1.0f;

    [Header("Continuity Elements")]
    public GameObject wolfBoss;
    public GameObject lightningSword;


    //[Header("Misc")]
    //public float back
    protected override void Start()
    {
        if (doStealPlayerStats)
            StealPlayerStats();
        base.Start();

    }

    void StealPlayerStats()
    {
        initialHealth = player.maxHealth * healthMult;
        projectileDamage *= player.AttackDamageMultiplier;
        dashAttackDamage *= player.AttackDamageMultiplier;
        BasicAttackDamage *= player.AttackDamageMultiplier;

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (attackState == AttackState.SHIELD)
        {
            shieldTime += Time.deltaTime;
        }

    }

    protected override void HandlePlayerInView()
    {
        base.HandlePlayerInView();
        LowerShield();
    }

    protected override void HandleAggression()
    {
        FacePlayer();
        if (attackState == AttackState.DASH)
        {
            Debug.Log("Dash Attack");
            FundamentalAttack(dashAttackDamage, dashAttackRadius, dashAttackForce, attackTransform);
            attackState = AttackState.NONE;
            state_ = State.ATTACKING;
        }
        else
        {
            if (IsHeroAttacking())
            {
                attackState = AttackState.SHIELD;
                shieldIndicator.SetActive(true);
            }
            else if (shieldTime >= shieldMinTime)
            {
                attackState = AttackState.NONE;
                LowerShield();
            }
        }
        HandleDistances();
    }

    protected override void HandleAttacking()
    {
        base.HandleAttacking();
        if (attackState == AttackState.BASIC || attackState == AttackState.DASH)
        {
            Vector3 rotation = new Vector3(0, AttackRotateSpeed, 0);
            attackSword.transform.Rotate(rotation);
        }

    }

    void LowerShield()
    {
        shieldIndicator.SetActive(false);
        shieldTime = 0f;
    }
    public override void Damage(float damage, Vector3 knockback)
    {
        if (state_ != State.ATTACKING && state_ != State.DAMAGED && state_ != State.DEAD)
        {
            Counter();
        }
        else
        {
            base.Damage(damage, knockback);
        }
    }

    bool IsHeroAttacking()
    {
        return (player.state_ == HeroController.State.ATTACKING ||
            player.state_ == HeroController.State.JUMPATTACK ||
            player.state_ == HeroController.State.DASHATTACK);
    }

    void Counter()
    {
        Debug.Log(state_);
        //This is to prevent a god damn feedback loop which literally crashes Unity
        if (state_ == State.DAMAGED || state_ == State.ATTACKING || player.state_ == HeroController.State.BLOCKING)
            return;
        Debug.Log("Countering!");
        FundamentalAttack(counterDamage, counterRadius, counterForce, attackTransform);
    }

    protected override void SelectAttack()
    {
        selection = Random.Range(0, 1f);
        if (state_ == State.AGGRESSION) //Within basic attack range
        {
            if (selection <= slamChance)
            {
                StartCoroutine(SlamAttack());
            }
            else
            {
                StartCoroutine(BasicAttackSequence());
            }
        }
        else if (state_ == State.PLAYERINVIEW)
        {
            if (selection <= projectileChance)
            {
                StartCoroutine(ProjectileAttack());
            }
            else
            {
                StartCoroutine(DashAttack());
            }
        }
    }

    IEnumerator BasicAttackSequence()
    {
        state_ = State.ATTACKING;
        StartCoroutine(TelegraphAttack());
        attackState = AttackState.BASIC;
        yield return new WaitForSeconds(BasicAttackStartup);
        BasicAttack();
        yield return new WaitForSeconds(BasicAttackCooldown);
        attackState = AttackState.NONE;
        state_ = State.IDLE;
        Debug.Log("Attack Complete");
    }

    IEnumerator DashAttack()
    {

        StartCoroutine(TelegraphAttack());
        yield return new WaitForSeconds(dashStartup);
        //state_ = State.ATTACKING;
        attackState = AttackState.DASH;
        rb.AddForce(dashForce * GetAimAngle());
        //Attack will be executed if player is in range
        yield return new WaitForSeconds(dashDuration);
        state_ = State.IDLE;
        attackState = AttackState.NONE;

    }
    IEnumerator ProjectileAttack()
    {
        state_ = State.ATTACKING;
        attackState = AttackState.PROJECTILE;
        StartCoroutine(TelegraphAttack());
        yield return new WaitForSeconds(projectileStartup);
        SpawnProjectile(projectile, attackTransform, projectileDamage, 
            projectileImpactRange, BasicAttackForce, projectileDisplacement * transform.forward);
        yield return new WaitForSeconds(projectileAttackDuration);
        state_ = State.IDLE;
        attackState = AttackState.NONE;

    }

    IEnumerator SlamAttack()
    {
        state_ = State.ATTACKING;
        StartCoroutine(TelegraphAttack(slamTelegraph, slamStartup));
        yield return new WaitForSeconds(slamStartup);
        attackState = AttackState.SLAM;
        Instantiate(slamParticles, transform);
        FundamentalAttack(slamDamage, slamRadius, slamForce, transform);
        yield return new WaitForSeconds(slamAttackDuration);
        state_ = State.IDLE;
        attackState = AttackState.NONE;
    }

    public override void Kill()
    {
        rb.constraints = RigidbodyConstraints.None;
        base.Kill();
        Instantiate(wolfBoss, transform.position, transform.rotation);
        Instantiate(lightningSword, transform.position, transform.rotation);
        for (int i = 0; i < toggleOnDeath.Length; i++)
            toggleOnDeath[i].SetActive(!toggleOnDeath[i].activeInHierarchy);
        
        Destroy(gameObject);
        // Trigger Wolf because fuck you that's why

    }

}