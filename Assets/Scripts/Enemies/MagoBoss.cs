using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This class will be used to organize the Wizard's attacks
[System.Serializable]
public class MagoAttack
{
    public UnityEvent attackEvent;
    public float executionCooldown= 1.0f; 
    public int weight;
}

[System.Serializable]
public class AttackPack
{
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

        int selectedWeight = Random.Range(0, totalWeight);
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
    public float[] stageThresholds;
    public EnemySpawner stageSpawners;
    public float stageAttackTimeMins;
    public float stageAttackTimeMaxes;
    public GameObject[] stageTransitActivators;
    public GameObject[] stageTransitDeactivators;

    [Header("Attacks")]
    [SerializeField]
    public AttackPack[] stages;
    // We don't want the Wizard to do any of the regular enemy stuff with states.
    // Regardless of player distance, he will be selecting from a set of attacks.
    protected override void FixedUpdate() 
    {
        FacePlayer();

    }
}
