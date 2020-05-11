using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class extends enemy solely for the fact that the Player needs to be able to hit it.
public class PunchingBag : Enemy
{
    public float DamageUpgradePerHit = 1.01f;

    protected override void FixedUpdate()
    {
     //do nothing; rocks won't attack you.

    }

    protected override void IdentifyTarget()
    {
        //also do nothing

    }

    public override void Damage(float damage, Vector3 knockback)
    {
        base.Damage(damage, Vector3.zero); //ignore knockback
        player.AttackDamageMultiplier *= DamageUpgradePerHit; //increase player's attack damage
    }

    protected override IEnumerator HandleDamage(float damage)
    {
        yield return null; //nope don't do this either
    }

    public override void Kill()
    {
        PlayDeathSound();
        Destroy(gameObject);
        //TODO: explode
    }

    protected override void SpawnDamageText(float damage)
    {
        GameObject damageText = Instantiate(floatingDamageText, transform.position, Quaternion.Euler(0,0,0));
        damageText.GetComponent<FloatingDamageText>().SetValue(damage);
    }

}
