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
        SHIELD
    }

    [Header("HeroEnemy Essentials")]
    public bool doStealPlayerStats = true;
    public AttackState attackState;

    [Header("Visual Elements")]
    public GameObject shieldIndicator;
    public GameObject projectionLight;

    [Header("Dash Attack")]
    public float dashAttackDamage = 30f;
    public float dashAttackForce = 1250f;
    public float dashAttackRadius = 5f; 

    [Header("Counter")]
    public float counterDamage = 5f;
    public float counterForce = 700;
    public float counterRadius = 4;
    public float shieldMinTime = 0.25f;
    public float shieldTime = 0.0f;
    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(attackState == AttackState.SHIELD)
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
        if(IsHeroAttacking())
        {
            attackState = AttackState.SHIELD;
            shieldIndicator.SetActive(true);
        }
        else if(shieldTime >= shieldMinTime)
        {
            LowerShield();
        }
        HandleDistances();
    }

    void LowerShield()
    {
        attackState = AttackState.NONE;
        shieldIndicator.SetActive(false);
        shieldTime = 0f;
    }
    public override void Damage(float damage, Vector3 knockback)
    {
        if (state_ != State.ATTACKING)
        {
            Counter();
        }
        else
        {
            Debug.Log(player.state_);
            Debug.Log(shieldTime);
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
        FundamentalAttack(counterDamage, counterRadius, counterForce, attackTransform);
    }
}
