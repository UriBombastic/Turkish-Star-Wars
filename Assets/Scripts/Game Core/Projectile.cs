using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public IAttacker owner;
    public float damageToDo;
    public float radius;
    public float attackForce;
    public bool doSpecial = false;

    public void InstantiateProjectile(IAttacker owner)
    {
        this.owner = owner;
    }

   public void InstantiateProjectile(IAttacker owner, float damageToDo, float radius, float attackForce, bool doSpecial = false)
    {
        this.owner = owner;
        this.damageToDo = damageToDo;
        this.radius = radius;
        this.attackForce = attackForce;
        this.doSpecial = doSpecial;
        enabled = true;
    }

    public virtual void OnCollisionEnter()
    {
        if (!enabled) return;
        //execute attack by entity which spawned projectile at location of projecile upon collision
        owner.FundamentalAttack(damageToDo, radius, attackForce, transform);
        enabled = false;
    }
}
