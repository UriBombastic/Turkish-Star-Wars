using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//in Level 04 (cave) you get bonus points for rescuing the innocent bystanders who the yetis torment.
//My decision on how to handle this was to make a unique script, 
//and slap it on the UpgradePlayer object to be activated at the same time
public class LevelSpecialScriptCave : MonoBehaviour
{
    public UpgradePlayer upgradePlayer;

  void Awake()
    {
        if (upgradePlayer == null)
            upgradePlayer = FindObjectOfType<UpgradePlayer>();

        //gotta beat UpgradePlayer to its own display initialization, so this is in awake.
        upgradePlayer.Points += CalculateBonusPoints(); //add bonus points for suriving bystanders
    }

    public int CalculateBonusPoints()
    {
        Enemy[] survivingEnemies = FindObjectsOfType<Enemy>();
        if (survivingEnemies.Length > 0) return 0;
        Bystander[] survivingBystanders = FindObjectsOfType<Bystander>();
        //if bystander array is empty, award no bonus points.
        return (survivingBystanders == null || survivingBystanders.Length == 0)
            ? 0 : survivingBystanders.Length / 3 + 1; //if even one survives, award bonus points (max 5).
    }
}
