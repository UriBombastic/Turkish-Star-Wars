using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    // To explode people
    public GameObject explosionEffect;

    public bool instantKill;
    public bool doBreakShield;
    public float lavaDamage = 200f; // If not instant kill
    public float bounceForce = 2000f; // Launch victim upwards

    private void OnTriggerEnter(Collider other)
    {
        IDamageable victim = other.GetComponent<IDamageable>();

        // If no damageable component, stop sequence.
        if (victim == null) return;

        // Instant Death?
        if (instantKill)
        {
            victim.Kill();
        }
        else
        {
           if(doBreakShield)
            {
                HeroController hero = other.GetComponent<HeroController>();
                if(hero)
                {
                    if(hero.state_ == HeroController.State.BLOCKING)
                    {
                        hero.ShieldPower = 0;
                        hero.BreakShield();
                    }
                }
            }
            victim.Damage(lavaDamage, new Vector3(0, bounceForce, 0));
        }

        if(explosionEffect)
            Instantiate(explosionEffect, other.transform.position, other.transform.rotation);
    }
}
