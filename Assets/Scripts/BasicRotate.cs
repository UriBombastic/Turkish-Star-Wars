using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRotate : MonoBehaviour
{
    public Vector3 moveDirection;
    void FixedUpdate()
    {
        transform.Rotate(moveDirection);
    }
}
