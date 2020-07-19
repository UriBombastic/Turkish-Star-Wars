using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterZone : MonoBehaviour
{
    public Transform teleportDestination;

    public void OnTriggerEnter(Collider other)
    {
        if (teleportDestination == null) return;
        if(other.GetComponent<Rigidbody>())
        {
            other.transform.position = teleportDestination.position;
        }
    }
}
