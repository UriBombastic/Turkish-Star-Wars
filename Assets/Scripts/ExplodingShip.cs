using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodingShip : MonoBehaviour
{
    public Vector3 moveForce;
    public GameObject explosionParticles;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        rb.AddForce(moveForce);
    }

    public void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Explode()
    {
        explosionParticles.transform.position = transform.position;
        explosionParticles.SetActive(true);
        Destroy(gameObject);
    }
}
