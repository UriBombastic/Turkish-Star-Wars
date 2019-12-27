using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicRotate : MonoBehaviour
{
    public Vector3 moveDirection;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(moveDirection);
    }
}
