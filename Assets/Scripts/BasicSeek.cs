using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSeek : MonoBehaviour
{
    public Transform targetTransform;
    public float moveForce = 200;
    private Rigidbody rb;
    public bool doSeekPlayer = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if(doSeekPlayer)
        {
            targetTransform = FindObjectOfType<HeroController>().transform;
        }
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
