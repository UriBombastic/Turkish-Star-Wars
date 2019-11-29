using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour, IDamageable
{
    public string name = "Enemy";
    public float initialHealth = 100f;
    public float health;
    public GameObject healthCanvas;
    public Image healthBar;
    public TextMeshProUGUI nameText;

    public Transform attackTransform;
    public float MoveForce;
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
    public AudioClip deathSound;

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
    }

    void Start()
    {
        health = initialHealth;
        if (nameText != null) nameText.text = name;
        player = FindObjectOfType<HeroController>();
        playerTransform = player.transform;
    }

    protected virtual void Update() //base functions of all enemies
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
        Vector3 playerPosition = playerTransform.position;
        float distance = Vector3.Distance(transform.position, playerPosition);
        if (distance <= AttackRange)
            state_ = State.AGGRESSION;
        else if (distance <= ViewRange)
            state_ = State.PLAYERINVIEW;
        else
            state_ = State.IDLE;
    }

    protected virtual void HandlePlayerInView()
    {
        Vector3 playerAngle = (playerTransform.position - transform.position);
        playerAngle.Normalize();
        rb.AddForce(playerAngle * MoveForce); //walk towards player
    }

    protected virtual void HandleAggression()
    {
        StartCoroutine(Attack());
    }

    protected virtual IEnumerator Attack()
    {
        Debug.Log(name + " is Attacking!");
        state_ = State.ATTACKING;
        yield return new WaitForSeconds(BasicAttackStartup);
        if (state_ != State.DAMAGED)
        {
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


    public void Damage(float damage)
    {
        Damage(damage, Vector3.zero);
    }

    public void Damage(float damage, Vector3 knockback)
    {
        health -= damage;
        UpdateHealthBar();
        PlayDamageSound();
        if (rb != null) rb.AddForce(knockback);
        if (health <= 0) Kill();
        StartCoroutine(handleDamage(damage));
    }

    protected virtual IEnumerator handleDamage(float damage)
    {
        state_ = State.DAMAGED;
        yield return new WaitForSeconds(DamageRecoverTime);
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
        Debug.Log("Oh no, I am dead.\n" + name);
        Destroy(gameObject);
    }
}
