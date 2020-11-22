using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenNinja :GenericBoss
{
   
    public enum AttackState
    {
        NONE,
        SPINDASH,
        PROXIMITY,
        PROJECTILE,
        TELEPORT
    }
    [Header("Golden Ninja")]
    public AttackState attackState_;
    public GameObject telegraphObject;

    public int maxConsecutiveAttacks = 5;
    public int currentConsecutiveAttacks = 0;
    public float restTime = 5.0f;
    public GameObject restIndicator;

    [Header("Spindash")]
    public float spinDashChance = 0.25f;
    public float spinDashDamage = 20f;
    public float spinDashForce = 3000f;
    public float spinDashStartup = 1.0f;
    public float spinDashDuration = 0.5f;
    public float spinSpeed = 0;
    public float spinAcceleration = 1f;
    public float spinDashShockwaveReach = 5.0f;
    public float maxSpinSpeed = 60f;

    [Header("Proximity Attack")]
    public int proximityAttackCount = 5;
    public float flurryStartupTime = 1.0f;
    public float particleSpawnRadius = 0.5f;

    [Header("Projectiles")]
    public int projectileCount = 5;
    public GameObject projectile;
    public Transform projectileTransform;
    public float projectileDelay = 0.5f;
    public float projectileDamage = 15f;
    public float projectileImpactRange = 3;
    public float projectileLaunchForce = 2000;

    [Header("Teleportation")]
    public float teleportAttackChance = 0.25f;
    public float teleportSeekTime = 1.0f;
    public float teleportProjection = 1.0f;
    public float slashParticleDuration = 1.0f;
    public float teleportDamage = 25f;
    public float teleportAttackRange = 7f;
    public float teleportAttackForce = 400f;
    public GameObject teleportMarker;
    public GameObject slashParticles;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        transform.Rotate(new Vector3(0, spinSpeed, 0));
    }

    protected override void SelectAttack()
    {
        if (currentConsecutiveAttacks < maxConsecutiveAttacks && state_ != State.DAMAGED)
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
                    selection = Random.Range(0, 1f);
                    if (selection > teleportAttackChance)
                    {
                        StartCoroutine(ProjectileSequence());
                    }
                    else
                    {
                        StartCoroutine(TeleportSequence());
                    }
                }
            }
            else
            {
                StartCoroutine(SpinDashSequence());
            }
            currentConsecutiveAttacks++;
        }
        else
        {
            StartCoroutine(RestSequence());
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
           // Debug.Log("Adding Force");
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

    protected override IEnumerator TelegraphAttack()
    {
        telegraphObject.SetActive(true);
        yield return new WaitForSeconds(TelegraphDelay);
        telegraphObject.SetActive(false);
        yield return null;
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
            SpawnRandomAttackParticle();
            yield return new WaitForSeconds(BasicAttackStartup);
        }
        attackState_ = AttackState.NONE;
        state_ = State.IDLE;
        yield return null;
    }

    void SpawnRandomAttackParticle()
    {    
        Vector3 attackPosition = attackTransform.position;
        float newX = Random.Range(-AttackRange, AttackRange);
        float newY = Random.Range(-AttackRange, AttackRange);
        Vector3 newAttackPosition = new Vector3(newX, newY, 0);
        Instantiate(attackParticles, attackTransform);
        attackParticles.transform.position = newAttackPosition;
    }
   
    IEnumerator ProjectileSequence()
    {
        attackState_ = AttackState.PROJECTILE;
        StartCoroutine(TelegraphAttack());
        yield return new WaitForSeconds(TelegraphDelay);
        for(int i = 0; i < projectileCount; i++)
        {
            SpawnProjectile();
            yield return new WaitForSeconds(projectileDelay);
        }
        attackState_ = AttackState.NONE;
        state_ = State.IDLE;
        yield return null;
    }

    void SpawnProjectile()
    {
        GameObject instantiatedProjectile = Instantiate(projectile, projectileTransform.position, projectileTransform.rotation); ;
        instantiatedProjectile.GetComponent<RockProjectile>().InstantiateProjectile(this, projectileDamage, projectileImpactRange, 0);
        Vector3 angle = GetAimAngle();
        angle.y = 0f;
        instantiatedProjectile.GetComponent<Rigidbody>().AddForce(angle * projectileLaunchForce);
    }

    IEnumerator TeleportSequence()
    {
        attackState_ = AttackState.TELEPORT;
        GameObject currentTeleportMarker = Instantiate(teleportMarker, playerTransform);
        yield return new WaitForSeconds(teleportSeekTime);
        currentTeleportMarker.transform.SetParent(null);
        yield return new WaitForSeconds(teleportProjection);
        transform.position = currentTeleportMarker.transform.position;
        Destroy(currentTeleportMarker);
        FundamentalAttack(teleportDamage, teleportAttackRange, teleportAttackForce, attackTransform);
        slashParticles.SetActive(true);
        yield return new WaitForSeconds(slashParticleDuration);
        slashParticles.SetActive(false);
        attackState_ = AttackState.NONE;
        state_ = State.IDLE;
        yield return null;
    }
    
    IEnumerator RestSequence()
    {
        state_ = State.DAMAGED;
        restIndicator.SetActive(true);
        yield return new WaitForSeconds(restTime);
        restIndicator.SetActive(false);
        state_ = State.IDLE;
        currentConsecutiveAttacks = 0;
        yield return null;
    }

    public override void Kill()
    {
        base.Kill();
        Destroy(healthCanvas);
        rb.constraints = RigidbodyConstraints.None;
        Destroy(this);
    }


}
