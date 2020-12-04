using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonEnemy : Enemy
{
    [Header("Skeleton Enemy")]
    public GameObject displaySword;
    public GameObject AttackSword;
    public float AttackRotateSpeed = 10f;
    public override void Kill()
    {
        base.Kill();
        PlayDeathSound();
        rb.constraints = RigidbodyConstraints.None;
        Destroy(healthCanvas);
        Destroy(this);
    }

    protected override IEnumerator Attack()
    {
        displaySword.SetActive(false);
        AttackSword.SetActive(true);
        yield return base.Attack();
        AttackSword.SetActive(false);
        AttackSword.SetActive(true);
        yield return null;
    }

    protected override void HandleAttacking()
    {
        Vector3 rotation = new Vector3(0, AttackRotateSpeed, 0);
        AttackSword.transform.Rotate(rotation);
    }
   
}
