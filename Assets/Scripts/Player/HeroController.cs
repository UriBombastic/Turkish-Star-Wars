using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class HeroController : MonoBehaviour, IDamageable
{
    //health
    public float maxHealth = 100f;
    public float health = 100f;
    //movement
    public float WalkForce = 20f;
    public float JumpForce = 500f;
    public float DashForce = 2000f;
    public float DashCoolDown = 1.0f;

    //attacks
    public GameObject AttackParticleEffect;
    public float AttackDamageMultiplier = 1.0f; //variable to be upgraded
    public float BasicAttackStartDelay = 0.0f;
    public float BasicAttackSpeed = 2f; //basic attacks per second
    public float BasicAttackDamage = 10f;
    public float BasicAttackRadius = 3.0f;
    public float BasicAttackForce = 100f;
    public float JumpAttackDelay = 1.0f;
    public float JumpAttackDamage = 15f;
    public float JumpAttackRadius = 4.5f;
    public float JumpAttackForce = 400f;
    public float DashAttackDelay = 1.0f; //cooldown invoked by Dash Attack
    public float DashAttackDamage = 15f;
    public float DashAttackRadius = 4.0f;
    public float DashAttackForce = 1000f;
    public float CounterRadius = 4.0f;
    public float CounterForce = 850f;
    public float recoverTime = 0.5f;
    public bool isRecovering = false;
    public Transform BasicAttackTransform;
    public Transform JumpAttackTransform;

    //Shielding
    public Image shieldImage;
    public int ShieldLevel = 0;
    public float maxOpacity = 0.5f;
    public float ShieldPower = 0f;
    public float shieldDegradeFactor = 60f; //frames to reduce Shield Power by 1.0. 
    public float maxShieldPower = 1.25f;
    public float shieldRegenFactor = 120f;//frames to regenerate Shield Power by 1.0
    //public float maxShieldTime = 0.25f;
    // public float ShieldCoolDown = 0.25f;
    // private bool canShield = true;

    //Health
    public Image HealthBar;
    public TextMeshProUGUI HealthText;
    
    //child components
    private Rigidbody rb;
    private AudioSource aud;
    
    //movement shit
    [SerializeField]
    private float h, v;
    //State enum
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
        ShieldPower = maxShieldPower;
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GameMaster.TogglePause();

    }

    void FixedUpdate()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        HandleInput();
        RegenerateShield();
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
        //if (!canShield) return; //hard cooldown replaced with continuous regeneration
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
        FundamentalAttack(BasicAttackDamage * AttackDamageMultiplier, BasicAttackRadius, BasicAttackForce, BasicAttackTransform);
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
        FundamentalAttack(JumpAttackDamage *AttackDamageMultiplier, JumpAttackRadius, JumpAttackForce, JumpAttackTransform);
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
        FundamentalAttack(DashAttackDamage*AttackDamageMultiplier, DashAttackRadius, DashAttackForce, BasicAttackTransform);
    }

    IEnumerator BlockTiming()
    {
        state_ = State.BLOCKING;
        shieldImage.gameObject.SetActive(true);
        Color col = shieldImage.color;

        //initialization. Deprecated.
      /*  col.a = maxOpacity;
        shieldImage.color = col;
       ShieldPower = 1.0f;
        shieldImage.transform.localScale = new Vector3(1, 0.5f, 1);
        yield return new WaitForSeconds(maxShieldTime);
        */

        //degrade shield
        while(state_ == State.BLOCKING && ShieldPower > 0f)
        {
            ShieldPower -= 1 / shieldDegradeFactor;
            float shieldPowerCapped = Mathf.Min(1.0f, ShieldPower); //prevent shield power from being > 1
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
      //  ShieldPower = 0;
      //  StartCoroutine(BlockCoolDown());
    }

    void RegenerateShield()
    {
        //regenerate only if not blocking and not maxxed out
        if (state_ == State.BLOCKING || ShieldPower >= maxShieldPower) return; 

        ShieldPower += 1 / shieldRegenFactor;
        ShieldPower = Mathf.Min(ShieldPower, maxShieldPower); //prevent from even slightest overflow
        
    }

    //deprecated
    /*IEnumerator BlockCoolDown()
    {
        canShield = false;
        yield return new WaitForSeconds(ShieldCoolDown);
        canShield = true;
    }*/


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
        if (ShieldPower == 1)
            Counter();
        rb.AddForce(knockbackToDo);
        if (isRecovering) return;
        float realDamageToDo = damageToDo * (1f - ShieldPower);
        health -= realDamageToDo;
        if (realDamageToDo > 0)
            StartCoroutine(DamageDelay());
        UpdateHealthBar();
       // PlayDamageSound();

        if (health <= 0) Kill();
    }

    public IEnumerator DamageDelay()
    {
        isRecovering = true;
        yield return new WaitForSeconds(recoverTime);
        isRecovering = false;
    }

    void UpdateHealthBar()
    {
        HealthText.text = Mathf.Round(health*10)/10 + " / " + maxHealth;
        HealthBar.fillAmount = health / maxHealth;
    }

    public void Kill()
    {
        Debug.Log("You are Dead!");
        rb.constraints = RigidbodyConstraints.None;
        GameMaster.Instance.HandleDeath();
    }

    public void Blind(bool doBlind)
    {
        GetComponent<LookDirectionController>().camera.gameObject.SetActive(doBlind);
    }
}

