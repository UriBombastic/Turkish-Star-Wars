using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMove : MonoBehaviour
{
    public Vector3 moveDirection;
    void Update()
    {
        transform.Translate(moveDirection);
    }
}
