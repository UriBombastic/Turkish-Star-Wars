using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour //Does this need to derive from Monobehaviour?
{
    public float AttackDamageMult = 1f;
    public float AttackSpeedMult = 1f;
    public float AttackRangeMult = 1f;
    public float AttackForceMult = 1f;

    public float MoveSpeedMult = 1f;
    public float JumpForceMult = 1f;
    public float DashCooldownReduction = 1f;

}
