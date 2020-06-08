using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YetiBoss : YetiEnemy
{
    public float minAttackTime = 1.5f;
    public float maxAttackTime = 3.0f;
    public float leapChance = 0.25f;
    public float leapForce = 5000f;
    public float leapRange = 10f;
    public float leapDamage = 25f;
    public bool isLeaping = false; //sub-state for leap attack

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
        yield return null;
    }

    IEnumerator Leap()
    {
        state_ = State.ATTACKING;
        Vector3 upwardsVector = transform.up * leapForce;
        rb.AddForce(upwardsVector); //leap upwards
        yield return new WaitForSeconds(BasicAttackStartup); //after having gone up...
        //determine player angle
        Vector3 playerAngle = (targetTransform.position - transform.position);
        playerAngle.Normalize();
        Vector3 aimAngle = new Vector3(playerAngle.x, playerAngle.y, playerAngle.z);
        rb.AddForce(aimAngle * leapForce*2);//leap towards player, midair
        isLeaping = true;
    }

    public void OnCollisionEnter()
    {
        state_ = State.IDLE;
        isLeaping = false;

    }

    IEnumerator LeapAttack()
    {
        yield return null;
    }

    protected override void HandleAggression()
    {
        FacePlayer();
    }




    public override void Kill()
    {
        base.Kill();
    }
}
