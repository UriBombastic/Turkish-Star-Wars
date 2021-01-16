using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericBoss : Enemy
{

    [Header("Generic Boss Fundamentals")]
    public float minAttackTime = 1.5f;
    public float maxAttackTime = 3.0f;
    protected float selection;
    public GameObject telegraphObject;

    //for game continuity
    public GameObject[] toggleOnDeath;

    protected override void Start()
    {
        if (!healthBar) healthBar = GameMaster.Instance.bossHealthBar;
        if (!nameText) nameText = GameMaster.Instance.bossNameDisplay;
        base.Start();
        StartCoroutine(AttackClock());

    }

    protected IEnumerator AttackClock()
    {
        float timeToNextAttack = Random.Range(minAttackTime, maxAttackTime);
        yield return new WaitForSeconds(timeToNextAttack);


        StartCoroutine(Attack());
        yield return new WaitForSeconds(BasicAttackCooldown); //allow time for attack to execute before even thinking of doing another attack
        StartCoroutine(AttackClock()); //begin next attack

    }
    protected override IEnumerator Attack()
    {
        SelectAttack();
        yield return new WaitForEndOfFrame();
    }
   
   protected virtual void SelectAttack()
    {
        //This will change in each boss.
    }
    //Effectively overwrites enemy aggression state where it attacks automatically
    protected override void HandleAggression()
    {
        HandleDistances();
        FacePlayer();
    }


    //Typically, when you kill a boss, things happen. Make said things happen.
    void ToggleContinuityElements()
    {
        for (int i = 0; i < toggleOnDeath.Length; i++)
            if (toggleOnDeath[i] != null)
                toggleOnDeath[i].SetActive(!toggleOnDeath[i].activeInHierarchy);

    }

    public override void Kill()
    {
        state_ = State.DEAD;
        PlayDeathSound();
        StopCoroutine(AttackClock());
        ViewRange = 0;
        AttackRange = 0;
    }

    //Genericized version of TelegraphAttack to abide by DRY principle
    protected IEnumerator TelegraphAttack(GameObject obj, float time)
    {
        obj.SetActive(true);
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
        yield return null;
    }

    protected override IEnumerator TelegraphAttack()
    {
        yield return TelegraphAttack(telegraphObject, TelegraphDelay);
    }


}
