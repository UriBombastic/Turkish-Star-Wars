using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour, IDamageable
{
    public string enemyName = "Enemy";
    public float initialHealth = 100f;
    public float health;
    public GameObject healthCanvas;
    public Image healthBar;
    public TextMeshProUGUI nameText;

    public Transform attackTransform;
    public float MoveForce;
   public float RotateSpeed = 4f;
    public float BasicAttackDamage;
    public float BasicAttackReach;
    public float BasicAttackForce;
    public float BasicAttackCooldown;
    public float BasicAttackStartup;

    public float DamageRecoverTime;

    public float ViewRange;
    public float AttackRange;

    protected Rigidbody rb;
    protected AudioSource aud;
    protected HeroController player;
    protected Transform playerTransform;
    protected Transform targetTransform;
    public List<Transform> potentialTargets;
    protected Animator animator;
    public string[] AnimationTriggers;
    public AudioClip deathSound;

    public bool DoIndiscriminateAttack = false;
    public bool DoFriendlyFire = false;
    public enum State
    {
        IDLE,
        PLAYERINVIEW,
        AGGRESSION,
        ATTACKING,
        DAMAGED
    };

    public State state_;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
        player = FindObjectOfType<HeroController>();
    }

    protected virtual void Start()
    {
        health = initialHealth;
        if (nameText != null) nameText.text = enemyName;
        playerTransform = player.transform;
        IdentifyTarget();
    }

    protected virtual void IdentifyTarget()
    {
        targetTransform = player.transform;
    }

    protected virtual void FixedUpdate() //base functions of all enemies
    {
    switch(state_)
        {
            case State.IDLE:
                HandleDistances();
                break; //basically, do nothing

            case State.PLAYERINVIEW: //once enemy is in range to follow player, eg
                HandleDistances();
                HandlePlayerInView();
                break;

            case State.AGGRESSION: //once an enemy is in range to attack
                HandleAggression();
                break;

            case State.ATTACKING: //the state of attacking itself
                HandleAttacking();
                break;

            case State.DAMAGED:
                break; //do nothing

        }

    }


    protected virtual void HandleDistances() //determine behavior based off player distance
    {
        if (targetTransform == null) return;
        Vector3 targetPosition = targetTransform.position;
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance <= AttackRange)
            state_ = State.AGGRESSION;
        else if (distance <= ViewRange)
            state_ = State.PLAYERINVIEW;
        else
            state_ = State.IDLE;
    }

    protected virtual void HandlePlayerInView()
    {
        Vector3 playerAngle = (targetTransform.position - transform.position);
        playerAngle.Normalize();
        Vector3 lookAngle = new Vector3(playerAngle.x, 0, playerAngle.z);
        Quaternion lookRotation = Quaternion.LookRotation(lookAngle);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotateSpeed);
        //rb.AddForce(playerAngle * MoveForce); //walk towards player
        rb.AddForce(transform.forward * MoveForce);
    }

    protected virtual void HandleAggression()
    {
        if(state_==State.AGGRESSION)//double check
            StartCoroutine(Attack());
    }

    protected virtual IEnumerator Attack()
    {
      //  Debug.Log(name + " is Attacking!");
        state_ = State.ATTACKING;
        yield return new WaitForSeconds(BasicAttackStartup);
        if (state_ != State.DAMAGED)
        {
            if (DoIndiscriminateAttack)
            IndiscriminateAttack(BasicAttackDamage, BasicAttackReach, BasicAttackForce, attackTransform, DoFriendlyFire);
            else
            FundamentalAttack(BasicAttackDamage, BasicAttackReach, BasicAttackForce, attackTransform);
            yield return new WaitForSeconds(BasicAttackCooldown);
            state_ = State.IDLE;
        }
    }

    protected virtual void HandleAttacking()
    {

    }


    void FundamentalAttack(float damageToDo, float radius, float attackForce, Transform t)
    {
            if (Vector3.Distance(t.position, playerTransform.position) <= radius)
            {
                Vector3 attackVector = playerTransform.position - transform.position;
                attackVector.Normalize();
                player.Damage(damageToDo, attackVector * attackForce);
            }
    }

    void IndiscriminateAttack(float damageToDo, float radius, float attackForce, Transform t, bool doAttackEnemies = true)
    {
        Collider[] hitColliders = Physics.OverlapSphere(t.position, radius);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].transform != this.transform) //don't hit yourself lol
            {
                if (!hitColliders[i].GetComponent<Enemy>() || doAttackEnemies)
                {
                    if (hitColliders[i].GetComponent<IDamageable>() != null)
                    {
                        Vector3 attackVector = hitColliders[i].transform.position - transform.position;
                        attackVector.Normalize();
                        // Debug.Log("The strike lands");
                        hitColliders[i].GetComponent<IDamageable>().Damage(damageToDo, attackVector * attackForce);
                    }
                }
            }
        }
    }

    public void Damage(float damage)
    {
        Damage(damage, Vector3.zero);
    }

    public virtual void Damage(float damage, Vector3 knockback)
    {
        health -= damage;
        UpdateHealthBar();
        PlayDamageSound();
        if (rb != null) rb.AddForce(knockback);
        if (health <= 0) Kill();
        StartCoroutine(HandleDamage(damage));
    }

    protected virtual IEnumerator HandleDamage(float damage)
    {
        // StopAllCoroutines();
        StopCoroutine(Attack());
        EnterDamage();
        yield return new WaitForSeconds(DamageRecoverTime);
        ExitDamage();
    }

    protected virtual void EnterDamage()
    {
        state_ = State.DAMAGED;
    }

    protected virtual void ExitDamage()
    {
        state_ = State.IDLE;
    }

    public void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float ratio = health / initialHealth;
        healthBar.fillAmount = ratio;
    }

    void PlayDamageSound()
    {
        aud.Play();
    }

    protected void PlayDeathSound()
    {
        if (deathSound == null) return;
        aud.volume = 1f;
        aud.clip = deathSound;
        aud.Play();
    }

    public virtual void Kill()
    {
        GameMaster.Instance.IncrementEnemyKillCount();
    }

    //essentially making it so that not every instance of animating has to do a null check, 
    //as well as allowing this method to be within the base Enemy class.
    protected void Animate(string trigger)
    {
        if (animator == null) return; //duh
       animator.SetTrigger(trigger);
    }

    protected void ResetAllAnimations(string excluded = "")
    {
        for (int i = 0; i < AnimationTriggers.Length; i++)
            if(AnimationTriggers[i] != excluded) animator.ResetTrigger(AnimationTriggers[i]);
    }
}
