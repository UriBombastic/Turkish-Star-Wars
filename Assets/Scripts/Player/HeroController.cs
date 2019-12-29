using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class HeroController : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float health = 100f;
    public float WalkForce = 20f;
    public float JumpForce = 500f;
    public float DashForce = 2000f;
    public float DashCoolDown = 1.0f;
    public GameObject AttackParticleEffect;
    public float BasicAttackStartDelay = 0.0f;
    public float BasicAttackSpeed = 2f; //basic attacks per second
    public float JumpAttackDelay = 1.0f;
    public float DashAttackDelay = 1.0f; //cooldown invoked by Dash Attack
    public float BasicAttackDamage = 10f;
    public float BasicAttackRadius = 3.0f;
    public float BasicAttackForce = 100f;
    public float JumpAttackDamage = 15f;
    public float JumpAttackRadius = 4.5f;
    public float JumpAttackForce = 400f;
    public float DashAttackDamage = 15f;
    public float DashAttackRadius = 4.0f;
    public float DashAttackForce = 1000f;
    public float CounterRadius = 4.0f;
    public float CounterForce = 850f;
    public Transform BasicAttackTransform;
    public Transform JumpAttackTransform;

    //Shielding
    public Image shieldImage;
    public float maxOpacity = 0.5f;
    public float ShieldPower = 0f;
    public float maxShieldTime = 0.25f;
    public float shieldDegradeFactor = 60f;
    public float ShieldCoolDown = 0.25f;
    private bool canShield = true;

    //Health
    public Image HealthBar;
    public TextMeshProUGUI HealthText;

    private Rigidbody rb;
    private AudioSource aud;
    [SerializeField]
    private float h, v;
    public enum State
    {
        IDLE,
        WALKING,
        JUMPING,
        DASHING,
        ATTACKING,
        JUMPATTACK,
        DASHATTACK,
        BLOCKING,
        GRABBING
    };
    
    public State state_;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        HealthText.text = health + " / " + maxHealth;
        HealthBar.fillAmount = health / maxHealth;
    }

    void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        HandleInput();
    }

    void HandleInput()
    {
        switch(state_)
        {
            case State.IDLE:
                if( h != 0 || v != 0)
                {
                    state_ = State.WALKING;
                }

                CheckForJump();
                CheckForAttack();
                CheckForBlock();
                break;

            case State.WALKING:
                if(!CheckForMove())
                {
                   state_ = State.IDLE;
                }

                CheckForJump();
                CheckForDash();
                CheckForAttack();
                CheckForBlock();
                break;

            case State.JUMPING:

                CheckForMove();
                CheckForDash();
                CheckForJumpAttack();
                CheckForBlock();
                break;

            case State.DASHING:
                CheckForDashAttack();
                break;

            case State.ATTACKING:
                CheckForMove();
                CheckForJump();
                break;

            case State.JUMPATTACK:
                CheckForMove();
                break;

            case State.DASHATTACK:
                break;

            case State.BLOCKING:
                if (!Input.GetKey(KeyCode.Mouse1))
                    BreakShield();
                if (CheckForAttack() || CheckForJump())
                    BreakShield();
                break;

            case State.GRABBING:
                break;
        }
    }

    bool CheckForMove()
    {
        if (h != 0 || v != 0)
        {
            Walk(h, v);
            return true;
        }

        return false;
    }

    bool CheckForJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
            return true;
        }
        return false;
    }

   void CheckForDash()
    {
        if (v == 0) return;
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            StartCoroutine(DashTiming(v));
    }

    bool CheckForAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(BasicAttackTiming());
            return true;
        }
        return false;
    }

    void CheckForJumpAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(JumpAttackTiming());
        }
    }

    void CheckForDashAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(DashAttackTiming());
        }
    }

    void CheckForBlock()
    {
        if (!canShield) return;
        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(BlockTiming());
        }
    }

    void Walk(float x, float z)
    {

        Vector3 WalkVector = transform.forward * z + transform.right * x;
        rb.AddForce(WalkVector * WalkForce);
    }

    void Jump()
    {
        rb.AddForce(new Vector3(0f, JumpForce, 0f));
        state_ = State.JUMPING;
    }

    IEnumerator DashTiming(float z)
    {
        state_ = State.DASHING;
        Vector3 DashVector = transform.forward * z;
        rb.AddForce(DashVector * DashForce);
        yield return new WaitForSeconds(DashCoolDown);
        state_ = State.WALKING;
    }

    void OnCollisionEnter(Collision col)
    {
        if (state_ == State.JUMPING || state_ == State.JUMPATTACK)
            state_ = State.IDLE;
    }

    IEnumerator BasicAttackTiming()
    {
        state_ = State.ATTACKING;
        yield return new WaitForSeconds(BasicAttackStartDelay);
        BasicAttack();
        yield return new WaitForSeconds(1/BasicAttackSpeed);
        state_ = State.IDLE;

    }

    void BasicAttack()
    {
        PlayAttackAudio();
        FundamentalAttack(BasicAttackDamage, BasicAttackRadius, BasicAttackForce, BasicAttackTransform);
    }

    IEnumerator JumpAttackTiming()
    {
        state_ = State.JUMPATTACK;
        JumpAttack();
        yield return new WaitForSeconds(JumpAttackDelay);
        if(state_ == State.JUMPATTACK) state_ = State.JUMPING;

    }

    void JumpAttack()
    {
        PlayAttackAudio();
        FundamentalAttack(JumpAttackDamage, JumpAttackRadius, JumpAttackForce, JumpAttackTransform);
    }

    IEnumerator DashAttackTiming()
    {
        state_ = State.DASHATTACK;
        DashAttack();
        yield return new WaitForSeconds(DashAttackDelay);
        state_ = State.IDLE;

    }

    void DashAttack()
    {
        PlayAttackAudio();
        FundamentalAttack(DashAttackDamage, DashAttackRadius, DashAttackForce, BasicAttackTransform);
    }

    IEnumerator BlockTiming()
    {
        state_ = State.BLOCKING;
        shieldImage.gameObject.SetActive(true);
        Color col = shieldImage.color;
        col.a = maxOpacity;
        shieldImage.color = col;
        ShieldPower = 1.0f;
        shieldImage.transform.localScale = new Vector3(1, 0.5f, 1);
        yield return new WaitForSeconds(maxShieldTime);

        while(state_ == State.BLOCKING && ShieldPower > 0f)
        {
            ShieldPower -= 1 / shieldDegradeFactor;
            col.a = maxOpacity * ShieldPower;
            shieldImage.color = col;
            shieldImage.transform.localScale = new Vector3(ShieldPower, ShieldPower/2, 1);
            yield return new WaitForSecondsRealtime(1 / 60f); //frame by frame update;
        }

        BreakShield();

    }

    void BreakShield()
    {
        state_ = State.IDLE;
        shieldImage.gameObject.SetActive(false);
        ShieldPower = 0;
        StartCoroutine(BlockCoolDown());
    }

    IEnumerator BlockCoolDown()
    {
        canShield = false;
        yield return new WaitForSeconds(ShieldCoolDown);
        canShield = true;
    }

    void Counter()
    {
        Debug.Log("Counter!");
        FundamentalAttack(0, CounterRadius, CounterForce, BasicAttackTransform);
    }

  void FundamentalAttack(float damageToDo, float radius, float attackForce, Transform t)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>(); 
        for(int i = 0; i < enemies.Length; i++)
        {
            Transform enemyTransform = enemies[i].transform;
            if(Vector3.Distance(t.position, enemyTransform.position)<= radius)
            {
             //   Debug.Log(enemies[i].name);
                Vector3 attackVector = enemyTransform.position - transform.position;
                attackVector.Normalize();
                enemies[i].Damage(damageToDo, attackVector * attackForce);
            }

        }
        if (AttackParticleEffect != null) Instantiate(AttackParticleEffect, t);
    }

    void PlayAttackAudio()
    {
        aud.Play();
    }


    //IDamageable requirements. We don't want to be invincible now
    public void Damage(float damageToDo, Vector3 knockbackToDo)
    {
        health -= damageToDo * (1f-ShieldPower);
        if (ShieldPower == 1)
            Counter(); 
        UpdateHealthBar();
       // PlayDamageSound();
        rb.AddForce(knockbackToDo);
        if (health <= 0) Kill();
    }

    void UpdateHealthBar()
    {
        HealthText.text = health + " / " + maxHealth;
        HealthBar.fillAmount = health / maxHealth;
    }

    public void Kill()
    {
        Debug.Log("You are Dead!");
    }
}

