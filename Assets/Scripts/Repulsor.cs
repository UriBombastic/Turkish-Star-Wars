using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Repulsor : MonoBehaviour
{
    public float repelForce = 200f;
    public float repelRadius = 5f;

    void Update()
    {
        RepelEverything();
    }

    void RepelEverything()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repelRadius);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].transform != this.transform) //don't hit yourself lol
            {
                Rigidbody rb = hitColliders[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 attackVector = hitColliders[i].transform.position - transform.position;
                    attackVector.Normalize();
                    rb.AddForce(attackVector * repelForce);
                }
            }
        }
    }
}
