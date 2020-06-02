using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagoEnemyPassive : MonoBehaviour //not ACTUALLY an enemy so doesn't have to derive from base enemy class
{
    protected Transform playerTransform;
    public float rotateSpeed = 10f;
    public float repelForce = 200f;
    public float repelRadius = 5f;
    public bool doRotate = true;
    void Start()
    {
        playerTransform = FindObjectOfType<HeroController>().transform;
    }

    void Update()
    {
        if(doRotate)FacePlayer();
        RepelEverything();
    }

    void FacePlayer() //ripped from enemy script
    {
        if (playerTransform == null) return;
        Vector3 playerAngle = (playerTransform.position - transform.position);
        playerAngle.Normalize();
        Vector3 lookAngle = new Vector3(playerAngle.x, 0, playerAngle.z);
        Quaternion lookRotation = Quaternion.LookRotation(lookAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
    }

    void RepelEverything()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, repelRadius);
        for(int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].transform != this.transform) //don't hit yourself lol
            {
                Rigidbody rb = hitColliders[i].GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 attackVector = hitColliders[i].transform.position - transform.position;
                        attackVector.Normalize();
                        // Debug.Log("The strike lands");
                        rb.AddForce(attackVector * repelForce);
                    }
                
            }
        }

    }
}
