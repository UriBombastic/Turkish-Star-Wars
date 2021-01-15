using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfEnemy : Enemy
{
    public GameObject telegraphObject;
    public float telegraphLinger = 0.5f;
    public float initializationTime = 2.0f;
    public GameObject initializationParticles;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(InitializationAnimation());

    }

    IEnumerator InitializationAnimation()
    {
        state_ = State.DAMAGED; //To basically freeze functions
        GameObject particles = Instantiate(initializationParticles, transform);
        particles.GetComponent<DecalDestroyer>().lifeTime = initializationTime;
        yield return new WaitForSeconds(initializationTime);
        state_ = State.IDLE; //Begin attack sequences.

    }
    protected override IEnumerator TelegraphAttack()
    {
        telegraphObject.SetActive(true);
        yield return new WaitForSeconds(BasicAttackStartup);
        Instantiate(attackParticles, attackTransform);
        yield return new WaitForSeconds(telegraphLinger);
        telegraphObject.SetActive(false);
    }

    public override void Damage(float damage, Vector3 knockback)
    {
        Debug.Log("Considering damage");
        if (player.itemState == ItemState.NULL) return; //MUST have the sword to hurt this bastard
        Debug.Log(damage);
        base.Damage(damage, knockback);
    }
    public override void Kill()
    {
        //TODO: dramatic explosion, level end
        base.Kill();
    }

}
