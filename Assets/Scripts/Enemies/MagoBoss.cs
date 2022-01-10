using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// This class will be used to organize the Wizard's attacks
[System.Serializable]
public class MagoAttack
{
    public UnityEvent attackEvent;
    public float executionCooldown= 1.0f; 
    public int weight;
}

// This boss is going to go through 3 different attack stages, and 1 final stage where he is getting defeated.
// Pretty much all the information for each stage is going to be inside the Stage class.
// Either this architecture is BRILLIANT, or over-relying on it will be a living nightmare.
[System.Serializable]
public class Stage
{

    public EnemySpawner enemySpawner;
    public float minAttackTime;
    public float maxAttackTime;
    public GameObject[] stageEntranceActivations;
    public GameObject[] stageEntranceDeactivations;
    public MagoAttack[] attacks;
    /// <summary>
    ///  Ripped from Enemy Spawner. Selects an attack.
    /// </summary>
    /// <returns> The attack to execute</returns>
    public MagoAttack SelectAttack()
    {
        int totalWeight = 0;
        for(int i = 0; i < attacks.Length; i++)
        {
            totalWeight += attacks[i].weight;
        }
        // +1 to convert random.range from maxExclusive to maxInclusive
        int selectedWeight = Random.Range(0, totalWeight) +1;
        for(int i = 0; i < attacks.Length; i++)
        {
            if (attacks[i].weight >= selectedWeight)
                return attacks[i]; // Return attack if within range of random weighted selection.
            else
                selectedWeight -= attacks[i].weight; // Reduce amount to go through.
        }
        Debug.LogError("Something went wrong; selected attack is null!"); // Hopefully, unreachable.
        return null;
    }
}

public class MagoBoss : GenericBoss
{
    [Header("Final Boss")]
    public int currentStage = 0;
    public float maxStamina = 100;
    public float currentStamina = 100;
    public float attackSequenceDuration = 30f; // This is just for ease of calculations . . .
    private float staminaDecreasePerSecond; // This is what will actually decrease the stamina.
    public float stunDuration = 15f; // Window player has to damage the boss once stamina is depleted. 
    public float finalStageDifficultyIncrease = 1.25f; // How much longer attack sequence lasts in final stage.
    private float currentStun = 0; // Use this for purposes of refilling stamina bar?
    public Image staminaBar;

    public float[] stageThresholds;
    public float repelRadius = 10f;
    public float repelForce = 200f;
    public GameObject RepelRings;
    private bool isFinalStage = false;

    // Minibosses
    public float[] minibossThresholds = { 1000f, 1000f }; // I know they're the same but what if I want to change them?
    public float miniBossAttackDelay = 15f;
    private bool[] hasSpawnedMiniboss = { false, false };


    [Header("Dramatic Death")]
    public GameObject smallDeathExplosion;
    public Transform explosionsCenter;
    public float smallExplosionRange = 3f;
    public int smallExplosions = 15;
    public float smallExplosionDelay = 0.2f;

    [Header("Attacks")]
    private float attackPenalty = 0f;
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public GameObject sawProjectile;
    public GameObject spearProjectile;
    public int multiSpearCount = 3;
    public float multiSpearDelay = 0.5f;
    public GameObject spellProjectile;
    public GameObject skyHammerCrosshairs;
    private GameObject spawnedCrosshairs;
    public float skyHammerStalkDuration;
    public GameObject[] LavaChunks;
    public int minLavaSpawns = 4;
    public int maxLavaSpawns = 6;
    public float LavaDelay = 1;

    [Header("Stages")]
    [SerializeField]
    public Stage[] stages;
    protected override void Start()
    {
        base.Start();
        staminaDecreasePerSecond = currentStamina / attackSequenceDuration;
    }


    // We don't want the Wizard to do any of the regular enemy stuff with states.
    // Regardless of player distance, he will be selecting from a set of attacks.
    protected override void FixedUpdate()
    {
        if (!isFinalStage) FacePlayer();
        if (state_ != State.DAMAGED && !isFinalStage)
        {
            Repel();
            DegradeStamina();
        }

    }

    private void DegradeStamina()
    {
        currentStamina -= staminaDecreasePerSecond * Time.deltaTime;
        if (currentStamina <= 0)
        {
            StartCoroutine(Stun());
        }
        staminaBar.fillAmount = currentStamina / maxStamina;
    }


    // Todo: Actually implement
    protected IEnumerator Stun()
    {
        state_ = State.DAMAGED;
        StopCoroutine(AttackClock());
        RepelRings.SetActive(false);
        PlayDamageSound();
        Debug.Log("Entering Stun");
        yield return new WaitForSeconds(stunDuration);
        ExitStun();
        yield return null;
    }

    private void ExitStun()
    {
        if (isFinalStage) return; 
        RepelRings.SetActive(true);
        state_ = State.IDLE;
        Debug.Log("Exiting Stun");
        StartCoroutine(AttackClock());
        currentStamina = maxStamina;
    }


    protected override IEnumerator AttackClock()
    {
        if(attackPenalty != 0)
        {
            yield return new WaitForSeconds(attackPenalty);
            attackPenalty = 0;
        }
        if (state_ == State.DAMAGED) yield break;
        yield return base.AttackClock();
    }

    private void Repel()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repelRadius);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (repelConditions(hitColliders[i])) // Determine if should be repelled, then do it
            {
                Rigidbody rb = hitColliders[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 attackVector = hitColliders[i].transform.position - transform.position;
                    attackVector = new Vector3(attackVector.x, 0, attackVector.z);
                    attackVector.Normalize();
                    rb.AddForce(attackVector * repelForce);
                }
            }
        }
    }

    // Conditions to be repelled from Mago
    private bool repelConditions(Collider collider)
    {
        if (collider.transform == this.transform) return false; // If this, don't repel.
        Projectile p = collider.gameObject.GetComponent<Projectile>(); // Check if has projectile
        if (!p)
        {
            return true; // if not projectile, repel. 
        }
        else
        {
            return (!p.owner.Equals(this)); // If projectile, ensure it's not part of this.
        }
    }

    protected override void SelectAttack()
    {
        MagoAttack selectedAttack = stages[currentStage].SelectAttack();
        BasicAttackCooldown = selectedAttack.executionCooldown; // Force AttackClock to wait long enough for execution.
        selectedAttack.attackEvent.Invoke(); // Invoke method attached to attack.
        
    }

    /******************************* Attacks! ****************************/
    public void AttackDummy()
    {
        Debug.Log("Dummy Attack");
    }

    public void AttackSummonMinions()
    {
        stages[currentStage].enemySpawner.SpawnEnemies();
    }

    public void AttackSeekingSaw()
    {
        SummonSaw(leftHandTransform);
    }

    public void AttackSeekingSawDouble()
    {
        SummonSaw(leftHandTransform);
        SummonSaw(rightHandTransform);
    }

    private void SummonSaw(Transform spawnTransform)
    {
        Instantiate(sawProjectile, spawnTransform.position, spawnTransform.rotation)
        .GetComponent<SawProjectile>().InstantiateProjectile(this);
    }


    public void AttackLightningSpear()
    {
        if (state_ == State.DAMAGED) return;
        SpearProjectile projectile = Instantiate(spearProjectile, 
            rightHandTransform.position + spearProjectile.transform.position, rightHandTransform.rotation)
            .GetComponent<SpearProjectile>();
        projectile.attackForce *= (targetTransform.position - transform.position).magnitude;///spearProjectileNormalizer;
        projectile.InstantiateProjectile(this);
    }

    public void AttackMultiSpear()
    {
        StartCoroutine(AttackMultiSpearReal());
    }

    private IEnumerator AttackMultiSpearReal()
    {
        for(int i = 0; i < multiSpearCount; i++)
        {
            AttackLightningSpear();
            yield return new WaitForSeconds(multiSpearDelay);
        }
    }

    public void AttackSpell()
    {
        //TODO: some method of swearing?
        Instantiate(spellProjectile, explosionsCenter.position, explosionsCenter.rotation)
            .GetComponent<SpellProjectile>().InstantiateProjectile(this);
    }

    public void AttackSkyHammer()
    {
        StartCoroutine(AttackSkyHammerReal());
    }

    private IEnumerator AttackSkyHammerReal()
    {
        Skyhammer hammer = Instantiate(skyHammerCrosshairs, targetTransform).GetComponent<Skyhammer>();
        hammer.InstantiateProjectile(this);
        yield return new WaitForSeconds(skyHammerStalkDuration);
        hammer.Explode();
    }

    public void AttackLavaRain()
    {
        StartCoroutine(LavaRainReal());
    }

    private IEnumerator LavaRainReal()
    {
        int selectedLavaChunks = Random.Range(minLavaSpawns, maxLavaSpawns);
        for(int i = 0; i < selectedLavaChunks; i++)
        {
            if (state_ == State.DAMAGED) yield break; // Stop if damaged
            SpawnLavaChunk();
            yield return new WaitForSeconds(LavaDelay);
        }
    }

    private void SpawnLavaChunk()
    {
        GameObject lavaChunk = LavaChunks[Random.Range(0, LavaChunks.Length)];
        Vector3 spawnPosition = new Vector3(targetTransform.position.x, lavaChunk.transform.position.y, targetTransform.position.z);
        Instantiate(lavaChunk, spawnPosition, Random.rotation);
    }

    /*************************** Damage / Dying ************************/
    public override void Damage(float damage, Vector3 knockback)
    {
        health -= damage;
        UpdateHealthBar();
        PlayDamageSound();
        if (doSpawnDamageText) SpawnDamageText(damage);
        // Decide if you'll spawn the miniboss
        CheckMiniBoss();
        // All this is so the player can have the satisfaction of dealing one final death blow to the Wizard.
        if (isFinalStage)
        {
            if (health <= 0) Kill();
        }
        else
        {
            if(health <= stageThresholds[currentStage])
            {
                ExitStun();
                SwitchStage();
            }
            if (health <= 0) health = 10; // Bail 

        }
    }

    private void CheckMiniBoss()
    {
        // Only going to have minibosses in first 2 stages
        if (currentStage > 1) return;

        // Don't spawn miniboss if has already, we don't want battlefield absolutely flooded
        if (hasSpawnedMiniboss[currentStage]) return;

        if(health <= stageThresholds[currentStage] + minibossThresholds[currentStage])
        {
            stages[currentStage].enemySpawner.SpawnEnemy(true);
            hasSpawnedMiniboss[currentStage] = true;
            attackPenalty = miniBossAttackDelay;
        }
    }

    private void SwitchStage()
    {
        currentStage++;
        Debug.Log("Switching to stage " + currentStage);
        isFinalStage = (currentStage == stages.Length - 1);
        if(currentStage == 2)
        {
            staminaDecreasePerSecond /= finalStageDifficultyIncrease; // Takes longer to degrade stamina
        }

        minAttackTime = stages[currentStage].minAttackTime;
        maxAttackTime = stages[currentStage].maxAttackTime;
        // Proper activations/deactivations
        for(int i = 0; i < stages[currentStage].stageEntranceActivations.Length; i++)
        {
            stages[currentStage].stageEntranceActivations[i].SetActive(true);
        }
        for (int i = 0; i < stages[currentStage].stageEntranceDeactivations.Length; i++)
        {
            stages[currentStage].stageEntranceDeactivations[i].SetActive(false);
        }
    }

    public override void Kill()
    {
        base.Kill();
        healthCanvas.SetActive(false);
        ToggleContinuityElements();
        KillAllEnemies(); // Make sure there are none left
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        for (int i = 0; i < smallExplosions; i++)
        {
            SpawnRandomDeathExplosion();
            Animate("Die"); // Create a spasming effect?
            yield return new WaitForSeconds(smallExplosionDelay);
        }
        // yield return new WaitForSeconds(bigExplosionDelay);
        // Instantiate(bigDeathExplosion, explosionsCenter.position, explosionsCenter.rotation).transform.localScale *= 2;
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

    private void KillAllEnemies()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        for(int i = 0; i < allEnemies.Length; i++)
        {
            if(allEnemies[i] != this)
            {
                allEnemies[i].Kill();
            }
        }
    }
}
