using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public bool doAutomatic = false;

    public void Start()
    {
        if (doAutomatic)
            GameMaster.Instance.EndLevel();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HeroController>())
            GameMaster.Instance.EndLevel();
    }
}
