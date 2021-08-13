using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    // To explode people
    public GameObject explosionEffect;

    public bool instantKill;

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
            victim.Damage(lavaDamage, new Vector3(0, bounceForce, 0));
        }

        if(explosionEffect)
            Instantiate(explosionEffect, other.transform.position, other.transform.rotation);
    }
}
