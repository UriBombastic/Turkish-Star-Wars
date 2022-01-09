using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearProjectile : Projectile
{
    public float seekForce = 10f;
    public GameObject ExplodeParticles;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.AddForce(attackForce * transform.forward);
    }


    public override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
        Instantiate(ExplodeParticles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
