using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellProjectile : Projectile
{
    public float velocityMax = 10f;
    private Rigidbody rb;
    private DecalDestroyer decal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        decal = GetComponent<DecalDestroyer>();
    }
    private void FixedUpdate()
    {
        if (((Enemy)owner).state_ == Enemy.State.DAMAGED) Destroy(gameObject); // Die if owner is damaged
        if (rb.velocity.magnitude > velocityMax)
        {
            rb.velocity = rb.velocity.normalized * velocityMax;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HeroController hero = other.gameObject.GetComponent<HeroController>();
        if(!hero) // Don't bother if not hero collider
        {
            return;
        }
        else
        {
            hero.Damage(damageToDo, Vector3.zero);
            die();
        }
    }

    private void die()
    {
        decal.enabled = true;
        rb.velocity = Vector3.zero;
        Destroy(this);
    }
}
