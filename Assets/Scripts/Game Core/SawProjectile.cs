using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawProjectile : Projectile
{
    public float moveForce = 20f;
    public float maximumNegativeAngle = -0.1f;
    private Transform targetTransform;
    private Rigidbody rb;

    public override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<SawProjectile>()) return; // Don't collide with others
        base.OnCollisionEnter(other);
        Destroy(gameObject);
    }

    private void Awake()
    {
        targetTransform = FindObjectOfType<HeroController>().transform;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (targetTransform.gameObject.activeInHierarchy)
        {
            Vector3 moveAngle = new Vector3(
                targetTransform.position.x - transform.position.x, 
                Mathf.Max(targetTransform.position.y - transform.position.y, maximumNegativeAngle),
                targetTransform.position.z - transform.position.z);
            moveAngle = moveAngle.normalized;
            rb.AddForce(moveAngle * moveForce);
        }
    }
}
