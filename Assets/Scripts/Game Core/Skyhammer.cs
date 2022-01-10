using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skyhammer : Projectile
{
    public float delay = 1.5f;
    public float deathDelay = 0.5f;
    public GameObject Explosion;
    public GameObject pillar;
    public void Explode()
    {
        StartCoroutine(ExplodeReal());
    }

    private IEnumerator ExplodeReal()
    {
        transform.SetParent(null);
        yield return new WaitForSeconds(delay);
        Enemy ownerAsEnemy = (Enemy)owner;
        if(!ownerAsEnemy)
        {
            Debug.LogError("Owner is null!");
            yield break;
        }
        ownerAsEnemy.IndiscriminateAttack(damageToDo, radius, attackForce, transform, true);
        Instantiate(Explosion, transform.position, transform.rotation);
        pillar.SetActive(true);
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);

    }


}
