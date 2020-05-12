using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemEquipInteractionPoint : MonoBehaviour
{
    public bool playerInRange;
    public TextMeshProUGUI DisplayText;
    public GameObject ItemGameObject;
    public string itemName;
    public ItemState itemState;

    void Start()
    {
        DisplayText.text = "Press 'R' to equip " + itemName;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<HeroController>())
            Toggle(true);
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<HeroController>())
            Toggle(false);
    }

    public void Toggle(bool tf)
    {
        playerInRange = tf;
      //  DisplayText.gameObject.SetActive(tf);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(KeyCode.R))
            ToggleEquip();
    }

    public void ToggleEquip()
    {
        HeroController player = FindObjectOfType<HeroController>();
        bool equipped = !(player.itemState == ItemState.NULL);

        if(equipped)
        {
            player.EquipItem(ItemState.NULL); //remove item if equipped
        }
        else
        {
            player.EquipItem(itemState); //if not equipped give player item in question
        }

        DisplayText.text = "Press 'R' to " + ((equipped) ? "unequip " : "equip ") + itemName;
        ItemGameObject.SetActive(equipped);
    }
}
