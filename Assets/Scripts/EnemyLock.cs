using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple script that's dependent on an array of Enemies
/// </summary>
public class EnemyLock : MonoBehaviour
{
    public Enemy[] dependents;
    // public UnityEvent endAction;
    public int deathLimit; // Once this many enemies have died, deactivate

    private void Start()
    {
        // Do not exceed lenght of enemy array
        deathLimit = Mathf.Min(deathLimit, dependents.Length);
    }
    // Update is called once per frame
    void Update()
    {
        int numDead = 0;
       for(int i = 0; i < dependents.Length; i++)
        {
            if(dependents[i] ==  null || dependents[i].health <= 0)
            {
                numDead++;
            }
        }

       // Maybe refactor for generic unity event?
       if(numDead >= deathLimit)
        {
            gameObject.SetActive(false);
        }
    }
}
