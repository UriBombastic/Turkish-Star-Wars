using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemChoice : MonoBehaviour
{
    public HeroController hero;
    public GameObject swordActivation;
    public GameObject fistActivation;
    public GameObject nullActivation;

    private Item swordBrain;
    private Item goldenFists;

    [Header("Sword & Brain Labels")]
    public TextMeshProUGUI swordDamageLabel;
    public TextMeshProUGUI swordSpeedLabel;
    public TextMeshProUGUI swordRangeLabel;
    public TextMeshProUGUI swordMoveSpeedLabel;

    [Header("Warrior Fists Labels")]
    public TextMeshProUGUI fistDamageLabel;
    public TextMeshProUGUI fistSpeedLabel;
    public TextMeshProUGUI fistRangeLabel;
    public TextMeshProUGUI fistMoveSpeedLabel;



    private void Awake()
    {
        swordBrain = hero.allItems[3];
        goldenFists = hero.allItems[4];
    }

    private void Start()
    {
        // Initialize sword stats
        swordDamageLabel.text = "x" + swordBrain.AttackDamageMult;
        swordSpeedLabel.text = "x" + swordBrain.AttackSpeedMult;
        swordRangeLabel.text = "x" + swordBrain.AttackRangeMult;
        swordMoveSpeedLabel.text = "x" + swordBrain.MoveSpeedMult;

        // Initialize fists stats
        fistDamageLabel.text = "x" + goldenFists.AttackDamageMult;
        fistSpeedLabel.text = "x" + goldenFists.AttackSpeedMult;
        fistRangeLabel.text = "x" + goldenFists.AttackRangeMult;
        fistMoveSpeedLabel.text = "x" + goldenFists.MoveSpeedMult;
    }

    // To be accessed via buttons
    public void SelectSword()
    {
        hero.EquipItem(ItemState.SWORDANDBRAIN);
        swordActivation.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SelectFists()
    {
        hero.EquipItem(ItemState.GOLDENKNUCKLES);
        fistActivation.SetActive(true);
        gameObject.SetActive(false);
    }

    public void SelectNull()
    {
        hero.EquipItem(ItemState.NULL);
        nullActivation.SetActive(true);
        gameObject.SetActive(false);
    }
}
