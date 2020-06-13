using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaTrigger : MonoBehaviour
{
    public GameObject[] objectsToActivate;
    public bool doShutoff = true;
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HeroController>())
            if (objectsToActivate.Length > 0)
                Execute(); 
    }

    private void Execute()
    {
        for (int i = 0; i < objectsToActivate.Length; i++)
            objectsToActivate[i].SetActive(true);

        if(doShutoff)gameObject.SetActive(false);
    }
}
