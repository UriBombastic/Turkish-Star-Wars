using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceGameObject : MonoBehaviour
{
    protected Transform targetTransform;
    public float rotateSpeed = 10f;
    public bool doSeekPlayer = false;

    void Awake()
    {
        if(doSeekPlayer)
            targetTransform = FindObjectOfType<HeroController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        FaceTarget();
    }

    void FaceTarget() //ripped from enemy script
    {
        if (targetTransform == null) return;
        Vector3 targetAngle = (targetTransform.position - transform.position);
        targetAngle.Normalize();
        Vector3 lookAngle = new Vector3(targetAngle.x, 0, targetAngle.z);
        Quaternion lookRotation = Quaternion.LookRotation(lookAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotateSpeed);
    }

}
