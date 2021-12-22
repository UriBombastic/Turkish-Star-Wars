using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A simple script to group an array of enemies under a single health bar
public class CollectiveEnemy : Enemy
{
    [Header("The Collective Part")]
    public Enemy[] components;
    public bool isBoss = false;
    protected override void Start()
    {
        initialHealth = SumComponentsHealth(true);
        if(isBoss)
        {
            if (!healthBar) healthBar = GameMaster.Instance.bossHealthBar;
            if (!nameText) nameText = GameMaster.Instance.bossNameDisplay;
        }
        base.Start();
    }

    protected override void FixedUpdate()
    {
        health = SumComponentsHealth();
        UpdateHealthBar();
        if (health <= 0) Kill(); 
    }

    private float SumComponentsHealth(bool isInit = false)
    {
        float totalHealth = 0;
        for (int i = 0; i < components.Length; i++)
            totalHealth += (isInit) ? components[i].initialHealth : Mathf.Max(components[i].health, 0);
        return totalHealth;
    }

    public override void Kill()
    {
        GameMaster.Instance.IncrementEnemyKillCount();
        if (isBoss) GameMaster.Instance.RegisterKillBoss();
        Destroy(this);
    }
}
