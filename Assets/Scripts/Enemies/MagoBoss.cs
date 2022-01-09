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
        int selectedWeight = Random.Range(0, totalWeight + 1);
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
    private float currentStun = 0; // Use this for purposes of refilling stamina bar?
    public Image staminaBar;
    public float[] stageThresholds;
    private bool isFinalStage = false;

    // Minibosses
    public float[] minibossThresholds = { 1000f, 1000f }; // I know they're the same but what if I want to change them?
    private bool[] hasSpawnedMiniboss = { false, false };

    [Header("Dramatic Death")]
    public GameObject smallDeathExplosion;
    public Transform explosionsCenter;
    public float smallExplosionRange = 3f;
    public int smallExplosions = 15;
    public float smallExplosionDelay = 0.2f;

    [Header("Attacks")]
    public Transform leftHandTransform;
    public Transform rightHandTransform;
    public GameObject sawProjectile;
    public GameObject spearProjectile;

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
        if(!isFinalStage) FacePlayer();
        if(state_ != State.DAMAGED)
        {
            DegradeStamina();
        }
    }

    private void DegradeStamina()
    {
        currentStamina -= staminaDecreasePerSecond * Time.deltaTime;
        if(currentStamina <=0)
        {
            StartCoroutine(Stun());
        }
    }

    // Todo: Actually implement
    protected IEnumerator Stun()
    {
        state_ = State.DAMAGED;
        StopCoroutine(AttackClock());
        yield return null;
    }


    protected override void SelectAttack()
    {
        MagoAttack selectedAttack = stages[currentStage].SelectAttack();
        BasicAttackCooldown = selectedAttack.executionCooldown; // Force AttackClock to wait long enough for execution.
        selectedAttack.attackEvent.Invoke(); // Invoke method attached to attack.
        
    }

    /******************************* Attacks! ****************************/
    public void AttackSummonMinions()
    {
        stages[currentStage].enemySpawner.SpawnEnemies();
    }

    public void AttackSeekingSaw()
    {
        
        Instantiate(sawProjectile, leftHandTransform.position, leftHandTransform.rotation)
            .GetComponent<SawProjectile>().InstantiateProjectile(this);
    }

    public void AttackLightningSpear()
    {
        SpearProjectile projectile = Instantiate(spearProjectile, 
            rightHandTransform.position + spearProjectile.transform.position, rightHandTransform.rotation)
            .GetComponent<SpearProjectile>();
        projectile.attackForce *= (targetTransform.position - transform.position).magnitude;///spearProjectileNormalizer;
        projectile.InstantiateProjectile(this);
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
        }
    }

    private void SwitchStage()
    {
        currentStage++;
        Debug.Log("Switching to stage " + currentStage);
        isFinalStage = (currentStage == stages.Length - 1);
        if(isFinalStage)
        {
            Debug.Log("On Final Stage");
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
