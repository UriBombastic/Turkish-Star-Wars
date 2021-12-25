using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSeek : MonoBehaviour
{
    public Transform targetTransform;
    public float moveForce = 200;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Only seek if active
        if(targetTransform.gameObject.activeInHierarchy)
        {
            Vector3 moveAngle = targetTransform.position - transform.position;
            moveAngle = moveAngle.normalized;
            rb.AddForce(moveAngle * moveForce);
        }
    }
}
