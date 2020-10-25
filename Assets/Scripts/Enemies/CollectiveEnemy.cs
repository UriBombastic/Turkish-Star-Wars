using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectiveEnemy : Enemy
{
    public Enemy[] components;

    protected override void Start()
    {
        initialHealth = SumComponentsHealth(true);
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
            totalHealth += (isInit) ? components[i].initialHealth : components[i].health;
        return totalHealth;
    }
}
