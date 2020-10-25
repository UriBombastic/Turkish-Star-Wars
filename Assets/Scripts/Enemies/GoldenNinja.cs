﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenNinja :GenericBoss
{
    public enum AttackState
    {
        NONE,
        SPINDASH,
        PROXIMITY,
        PROJECTILE
    }

    public AttackState attackState_;

    public GameObject telegraphObject;

    public float spinDashChance = 0.25f;
    public float spinDashDamage = 20f;
    public float spinDashForce = 3000f;
    public float spinDashStartup = 1.0f;
    public float spinDashDuration = 0.5f;
    public float spinSpeed = 0;
    public float spinAcceleration = 1f;
    public float spinDashShockwaveReach = 5.0f;
    public float maxSpinSpeed = 60f;

    public int proximityAttackCount = 5;
    public float flurryStartupTime = 1.0f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        transform.Rotate(new Vector3(0, spinSpeed, 0));
    }

    protected override void SelectAttack()
    {
        selection = Random.Range(0, 1f);
        if (selection > spinDashChance)
        {
            if (state_ == State.AGGRESSION)
            {
                StartCoroutine(ProximityFlurry());
            }
            else if (state_ == State.PLAYERINVIEW)
            {

            }
        }
        else
        {
            StartCoroutine(SpinDashSequence());
        }
    }

    IEnumerator SpinDashSequence()
    {
        StartCoroutine(Accelerate());
        state_ = State.AGGRESSION;

        yield return new WaitForSeconds(spinDashStartup);
        StopCoroutine(Accelerate());
        state_ = State.ATTACKING;
        attackState_ = AttackState.SPINDASH;
        SpinDashAttack();
        yield return new WaitForSeconds(spinDashDuration);
        state_ = State.IDLE;
        spinSpeed = 0;
        yield return null;
    }

    IEnumerator Accelerate()
    {
        Debug.Log("Accelerating");
        while(spinSpeed < maxSpinSpeed)
        {
            Debug.Log("Adding Force");
            spinSpeed += spinAcceleration;
            yield return new WaitForSeconds(1 / 60);
        }
        yield return null;
    }

    void SpinDashAttack()
    {
        Vector3 AttackVector = (playerTransform.position - transform.position).normalized;
        rb.AddForce(AttackVector * spinDashForce);
    }

    protected override void HandleAggression()
    {
        HandleDistances();
        //FacePlayer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (attackState_ == AttackState.SPINDASH && state_ == State.ATTACKING)
        {
            //if is player
            if (collision.transform == playerTransform)
            {
                FundamentalAttack(spinDashDamage, spinDashShockwaveReach, 0f, transform);
                state_ = State.IDLE; //To prevent second collision or whatever
            }
        }
    }

    IEnumerator ProximityFlurry()
    {
        attackState_ = AttackState.PROXIMITY;
        StartCoroutine(TelegraphAttack());
        yield return new WaitForSeconds(TelegraphDelay);
        for(int i = 0; i < proximityAttackCount; i++)
        {
            BasicAttack();
            yield return new WaitForSeconds(BasicAttackStartup);
        }
        attackState_ = AttackState.NONE;
        state_ = State.IDLE;
        yield return null;
    }
   

    protected override IEnumerator TelegraphAttack()
    {
        telegraphObject.SetActive(true);
        yield return new WaitForSeconds(TelegraphDelay);
        telegraphObject.SetActive(false);
        yield return null;
    }
}
