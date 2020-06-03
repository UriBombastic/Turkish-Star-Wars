using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradePlayer : MonoBehaviour
{

    public int Points;
    public HeroController player;
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI atkVariable;
    public TextMeshProUGUI healthVariable;
    public TextMeshProUGUI shieldVariable;
    public GameObject shieldMaxNotification;

    public float AtkUpgradeAmount = 0.2f;
    public int HealthUpgradeAmount = 20;
    public int ShieldUpgradeMax = 5;
    public float ShieldTimeUpgrade = 0.05f;
    public float ShieldDegradeUpgrade = 12f;
    public float ShieldRegenUpgrade = 12f;

    public GameObject outOfPointsMessage;
    
    void Awake()
    {
        if(player==null)
            player = FindObjectOfType<HeroController>();
    }

    void Start()
    {
        pointsText.text = Points.ToString();
        atkVariable.text = player.BasicAttackDamage * player.AttackDamageMultiplier + "";
        healthVariable.text = player.maxHealth.ToString();
        shieldVariable.text = player.ShieldLevel.ToString();
        GameMaster.Instance.HealPlayer(); //sister demanded that this be a thing
    }

    public void UpgradeAttack()
    {
        if (Points <= 0) return;
        player.AttackDamageMultiplier += AtkUpgradeAmount;
        atkVariable.text = player.BasicAttackDamage * player.AttackDamageMultiplier + "";
        SubtractPoint();
    }

    public void UpgradeHealth()
    {
        if (Points <= 0) return;
        player.maxHealth += HealthUpgradeAmount;
        player.health += HealthUpgradeAmount;
        healthVariable.text = player.maxHealth.ToString();
        SubtractPoint();
    }

    public void UpgradeShield()
    {
        if (Points <= 0) return;
        if(player.ShieldLevel >= ShieldUpgradeMax)
        {
            shieldVariable.gameObject.SetActive(false);
            shieldMaxNotification.SetActive(true);
            return;
        }

        player.maxShieldPower += ShieldTimeUpgrade;
        player.shieldDegradeFactor += ShieldDegradeUpgrade;
        player.shieldRegenFactor -= ShieldRegenUpgrade;
        player.ShieldLevel++;
        shieldVariable.text = player.ShieldLevel.ToString();
        SubtractPoint();
    }

    void SubtractPoint() //lmaooo instead of copy-pasting 2 lines I made a whole method
    {
        Points--;
        pointsText.text = Points.ToString();
        if (Points <= 0) outOfPointsMessage.SetActive(true);
    }

    public void OnNextLevel()
    {
        GameMaster.UploadPlayer();
        GameMaster.LoadNextLevel();
    }

}
