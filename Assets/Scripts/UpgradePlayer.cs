using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradePlayer : MonoBehaviour
{

    public int Points;
    public HeroController player;
    public TextMeshProUGUI pointsText;
    public int AtkUpgradeAmount = 20;
    public int HealthUpgradeAmount = 20;
    
    void Awake()
    {
        player = FindObjectOfType<HeroController>();
    }

    void Start()
    {
        pointsText.text = Points.ToString();
    }

    public void UpgradeAttack()
    {

    }

    public void UpgradeHealth()
    {
        player.maxHealth += HealthUpgradeAmount;
        player.health += HealthUpgradeAmount;
    }

    public void UpgradeShield()
    {

    }

}
