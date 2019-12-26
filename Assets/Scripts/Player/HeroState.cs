using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroState : MonoBehaviour
{
    public HeroState() { }
    public virtual void handleInput() { }
    public virtual void update() { }
}
