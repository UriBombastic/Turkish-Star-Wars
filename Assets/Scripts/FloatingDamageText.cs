using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void SetValue(float value)
    {
        text.text = GameMaster.StandardRounding(value).ToString();
    }
}
