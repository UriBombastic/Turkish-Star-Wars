using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GorillaEnemy : Enemy
{
    public GameObject stillGorilla;
    public GameObject howlingGorilla;
    public float howlTime = 1.55f;
    public float projectileLaunchForce = 50f;
    public float projectileDamage = 12f;
    public float projectileExplosionRange = 2f;
    public GameObject projectile;
    public Transform projectileTransform;
    public GameObject deathParticles;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        StartCoroutine(AttackSequence());
    }

    //simplify regular enemy controller. This enemy is a tank which relies on brute force.
    protected override void HandleDistances()
    {
        if (targetTransform == null) return;
        Vector3 targetPosition = targetTransform.position;
        float distance = Vector3.Distance(transform.position, targetPosition);
        //remove "Aggression" state
            if (distance <= ViewRange)
                state_ = State.PLAYERINVIEW;
            else
                state_ = State.IDLE;      
    }

    IEnumerator AttackSequence()
    {
        //Debug.Log("Attack Sequence");
        yield return new WaitForSeconds(BasicAttackCooldown);
        StartCoroutine(Attack());
        StartCoroutine(AttackSequence()); //effectively loop
    }

    protected override IEnumerator Attack()
    {
        //Debug.Log("Attacking");
        state_ = State.ATTACKING;
        //the model I used is stupid,
        //so this is a jank-ass method of manually invoking the animation
        SetVisualState(true);
        yield return new WaitForSeconds(BasicAttackStartup);

        SelectAttack();

        yield return new WaitForSeconds(Mathf.Max(0, howlTime-BasicAttackStartup)); //duration of howl audio clip.

        SetVisualState(false);
        state_ = State.IDLE;
    }

    void SetVisualState(bool isAttacking)
    {
        stillGorilla.SetActive(!isAttacking);
        howlingGorilla.SetActive(isAttacking);
    }

    void SelectAttack()
    {
        //calculate distance to decide attack
        Vector3 targetPosition = targetTransform.position;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= AttackRange)
            MeeleeAttack();
        else if (distance <= ViewRange)
            RangedAttack();

        //if out of view range, do nothing
    }

    void MeeleeAttack()
    {
        FundamentalAttack(BasicAttackDamage, BasicAttackReach, BasicAttackForce, attackTransform);
    }

    void RangedAttack()
    {
        GameObject instantiatedProjectile = Instantiate(projectile, projectileTransform.position, projectileTransform.rotation);
        instantiatedProjectile.GetComponent<RockProjectile>().InstantiateProjectile(this, projectileDamage, projectileExplosionRange, projectileLaunchForce, true);

        // Finds vertical angle between Gorilla and player
        float verticalDiff = (targetTransform.position.y - projectileTransform.position.y) / Vector3.Distance(projectileTransform.position, targetTransform.position);
        Debug.Log(verticalDiff);
        Vector3 aimAngle = new Vector3(transform.forward.x, transform.forward.y + verticalDiff, transform.forward.z);

        // Launch projectile
        instantiatedProjectile.GetComponent<Rigidbody>().AddForce(aimAngle * projectileLaunchForce);

    }

    public override void Kill()
    {
        base.Kill();
        if (deathParticles != null) Instantiate(deathParticles, transform.position, transform.rotation);
        Destroy(gameObject);

    }
}

// Documentation Easter Egg Time!
// I am honestly not entirely sure why I put this gorilla into the game.
// However, I went through the trouble of finding this model, and gearing it to be useable in the game,
// And even cutting audio from the original movie to absolutely blast players' audio. 
// Anyway, I'm going to double down on using this Gorilla model, and put it in the game over and over again.
