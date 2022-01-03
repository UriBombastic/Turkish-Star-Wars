using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigRobotBoss : YetiBoss
{

    [Header("BigRobot Boss")]
    public int maxLeaps = 2; // Leap Multiple times!

    private int attackIterations = 1; // Helps slow down the attack clock
    private int currentLeaps = 2; // Starts at 2 so he doesn't leap multiple times off the bat
    public bool isFirstAttack = true;

    [Header("Multishot")]
    public Transform[] multishotOrigins;
    public GameObject multishotProjectile;
    public float multishotChance = .35f;
    public float multiShotDamage = 15;
    public float multiShotLaunchForce = 2500;


    [Header("Defense")]
    public GameObject ShieldIndicator;
    public GameObject stunIndicator;
    public AudioSource stunAnnouncer;
    public Image shieldHealthIndicator;
    public float shieldDamageReduction = 35f;
    public float maxShieldHealth = 900f;
    public float currentShieldHealth = 900f;
    public float shieldHealthLostPerSecond = 3f;
    public float stunTime = 5f;
    public float shieldBreakStunTime = 10f;

    [Header("Dramatic Death")]
    public GameObject deathAnimation; // This will be a custscene which literally advances the level.
    public GameObject smallDeathExplosion;
    public GameObject bigDeathExplosion;
    public Transform explosionsCenter;
    public float smallExplosionRange = 3f;
    public int smallExplosions = 10;
    public float smallExplosionDelay = 0.2f;
    public float bigExplosionDelay = 3.0f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (state_ != State.DAMAGED)
        {
            DegradeShield();
            UpdateShieldHealthBar();
        }
    }

    protected override IEnumerator AttackClock()
    {
        float timeToNextAttack = Random.Range(minAttackTime, maxAttackTime);
        yield return new WaitForSeconds(timeToNextAttack);


        SelectAttack();
        yield return new WaitForSeconds(BasicAttackCooldown * attackIterations); //allow time for attack to execute before even thinking of doing another attack
        StartCoroutine(AttackClock()); //begin next attack

    }

    protected override void SelectAttack()
    {
        Debug.Log("Selecting Attack");
        // Always start by leaping, for dramatic effect. 
        if(isFirstAttack)
        {
            StartCoroutine(Leap());
            isFirstAttack = false;
        }
        if (state_ == State.AGGRESSION)
        {
            StartCoroutine(Attack());
        }
        else if (state_ == State.PLAYERINVIEW)
        {
            float selection = Random.Range(0f, 1f);
            if (selection > leapChance)
            {
                // Reroll! Kind of jank
                selection = Random.Range(0f, 1f);
                if (selection > multishotChance)
                {
                    StartCoroutine(ProjectileAttack());
                }
                else
                {

                    Multishot();
                }
                attackIterations = 1;
            }
            else
            {
                currentLeaps = 0;
                StartCoroutine(Leap());
                attackIterations = maxLeaps;
            }
        }
    }

    protected override IEnumerator LeapAttack()
    {
        yield return base.LeapAttack();
        currentLeaps++;

        if(currentLeaps < maxLeaps)
        {
            // Do it again!
            StartCoroutine(Leap());
        }
    }

    protected void Multishot()
    {
       for(int i = 0; i < multishotOrigins.Length; i++)
        {
            GameObject instantiatedProjectile = Instantiate(multishotProjectile, multishotOrigins[i].position, multishotOrigins[i].rotation);
            instantiatedProjectile.GetComponent<RockProjectile>().InstantiateProjectile(this, multiShotDamage, projectileImpactRange, 0, false);
            instantiatedProjectile.GetComponent<Rigidbody>().AddForce(GetAimAngle() * multiShotLaunchForce);
        }
    }

    protected override void EnterBerzerkMode()
    {
        base.EnterBerzerkMode();
        maxLeaps++;
    }

    private void DegradeShield()
    {
        currentShieldHealth -= shieldHealthLostPerSecond * Time.deltaTime;
        if(currentShieldHealth <= 0)
        {
            StartCoroutine(Stun(stunTime, false));
        }
    }

    private void UpdateShieldHealthBar()
    {
        shieldHealthIndicator.fillAmount = currentShieldHealth / maxShieldHealth;
    }

    IEnumerator Stun(float stunTime, bool indicateDamage = false)
    {
        state_ = State.DAMAGED;
        StopCoroutine(AttackClock());
        ShieldIndicator.SetActive(false);
        stunIndicator.SetActive(true);

        // Make it obvious
        if (indicateDamage)
        {
            Animate("Damage");
            stunAnnouncer.Play();
        }
        yield return new WaitForSeconds(stunTime);

        state_ = State.IDLE;
        StartCoroutine(AttackClock());
        ShieldIndicator.SetActive(true);
        stunIndicator.SetActive(false);
        currentShieldHealth = maxShieldHealth;
    }

    public override void Damage(float damage, Vector3 knockback)
    {
        if (state_ != State.DAMAGED)
        {
            currentShieldHealth -= damage;
            // Earn extra stun time by forcefully breaking Shield
            if (currentShieldHealth <= 0)
            {
                StartCoroutine(Stun(shieldBreakStunTime, true));
            }
            damage = Mathf.Max(0, damage - shieldDamageReduction); // Reduce by shield
        }
        
        base.Damage(damage, knockback);
    }

    public override void Kill()
    {
        state_ = State.DEAD;
        PlayDeathSound();
        StopCoroutine(AttackClock());
        ViewRange = 0;
        AttackRange = 0;
        Animate("Die");
        deathAnimation.SetActive(true);
        deathAnimation.transform.SetParent(null); // Set to top level of Hierarchy so doesn't get destroyed with this
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        for(int i = 0; i < smallExplosions; i++)
        {
            SpawnRandomDeathExplosion();
            Animate("Die"); // Create a spasming effect?
            yield return new WaitForSeconds(smallExplosionDelay);
        }
        yield return new WaitForSeconds(bigExplosionDelay);
        Instantiate(bigDeathExplosion, explosionsCenter.position, explosionsCenter.rotation).transform.localScale *=2;
        GameMaster.Instance.HealPlayer();
        Destroy(gameObject);
    }

    private void SpawnRandomDeathExplosion()
    {
        GameObject explosion = Instantiate(smallDeathExplosion, explosionsCenter.position, explosionsCenter.rotation);
        // Randomly change position
        Vector3 newPosition = new Vector3(
            explosion.transform.position.x + Random.Range(-smallExplosionRange, smallExplosionRange),
            explosion.transform.position.y + Random.Range(-smallExplosionRange, smallExplosionRange),
            explosion.transform.position.z + Random.Range(-smallExplosionRange, smallExplosionRange)
            );
        explosion.transform.position = newPosition;
    }

}
