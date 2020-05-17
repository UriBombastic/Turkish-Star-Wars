using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingStation : MonoBehaviour
{
    public HeroController player;
    public bool isHealing = false;

    public float HealthIncreasePS = 1f;
    public float MaxHealthIncreasePS = .1f;

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<HeroController>();
    }


    void FixedUpdate()
    {
        if (isHealing)
            DoHealing();
    }

    public void DoHealing()
    {//assuming called every frame @ 60 fps fixedUpdate
        GameMaster.Instance.HealPlayer(HealthIncreasePS / 60); //use healplayer interface to avoid overflow
        player.maxHealth += GameMaster.StandardRounding(MaxHealthIncreasePS / 60,3); //some gimmicky potential here

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<HeroController>())
            isHealing = true;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<HeroController>())
            isHealing = false;
    }
}
