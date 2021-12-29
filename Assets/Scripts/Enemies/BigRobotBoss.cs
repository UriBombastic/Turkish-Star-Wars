using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigRobotBoss : YetiBoss
{
    [Header("BigRobot Boss")]
    public int maxLeaps = 2; // Leap Multiple times!

    private int attackIterations = 1; // Helps slow down the attack clock
    private int currentLeaps = 2; // Starts at 2 so he doesn't leap multiple times off the bat
    public bool isFirstAttack = true;

    [Header("Dramatic Death")]
    public GameObject deathAnimation; // This will be a custscene which literally advances the level.
    public GameObject smallDeathExplosion;
    public GameObject bigDeathExplosion;
    public Transform explosionsCenter;
    public float smallExplosionRange = 3f;
    public int smallExplosions = 10;
    public float smallExplosionDelay = 0.2f;
    public float bigExplosionDelay = 3.0f;
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
                StartCoroutine(ProjectileAttack());
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
