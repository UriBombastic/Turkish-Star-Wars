using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioReverser : MonoBehaviour
{
    public AudioSource victim;
    public float multPitch = -1.0f;
    public void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<HeroController>())
            victim.pitch = multPitch;
    }
}
