using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMove : MonoBehaviour
{
    public Vector3 moveDirection;
    public bool useTransformForward;
    void FixedUpdate()
    {
        if (useTransformForward)
        {
            transform.Translate(new Vector3(transform.forward.x * moveDirection.x, transform.forward.y * moveDirection.y, transform.forward.z * moveDirection.z));
        }
        else
        {
            transform.Translate(moveDirection);
        }
    }
}
