using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class HeroController : MonoBehaviour, IDamageable
{
    #region declarations
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

    public ItemState itemState;
    public Item[] allItems;
    public GameObject [] itemDisplays;
    #endregion

    #region initialization
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
        EquipItem(itemState);
    }
    #endregion

    #region updates
    //updates
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            GameMaster.TogglePause();
        UpdateHealthBar();

        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        HandleItem();
        HandleInput();

    }

    void FixedUpdate()
    {
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
                CheckForBlock();
                break;

            case State.ATTACKING:
                CheckForMove();
                CheckForJump();
                CheckForBlock();
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

    void HandleItem()
    {
        switch(itemState)
        {
            case ItemState.NULL:
                break;

            case ItemState.HEAVYBOOTS:
                HandleHeavyBoots();
                break;

            case ItemState.LIGHTNINGSWORD:
                break;

            case ItemState.GOLDENKNUCKLES:
                break;
        }
    }

    #endregion

    #region Items
    public void EquipItem(ItemState iState)
    {
        this.itemState = iState; //in case this is being accessed externally. Should be the ONLY reference to iState.
        for (int i = 0; i < itemDisplays.Length; i++)
            itemDisplays[i].SetActive(i == (int)itemState);
    }

    public Item GetCurrentItem()
    {
        return allItems[(int)itemState];
    }

    void HandleHeavyBoots()
    {
        //this void is essentially called in synch with walking.
        //If not walking, don't bother with it.
        if (state_ == State.WALKING)
        {

            ItemMaster.Instance.TrainWalking();

            if (CheckForJump(false)) //check for jump without invoking jump
                ItemMaster.Instance.TrainJumping();
        }

        if(state_ == State.WALKING || state_== State.JUMPING)
            if (CheckForDash(false))
             ItemMaster.Instance.TrainDashing();

    }

    #endregion

    #region checks
    //checks
    bool CheckForMove()
    {
        if (h != 0 || v != 0)
        {
            Walk(h, v);
            return true;
        }

        return false;
    }

    bool CheckForJump(bool doAction = true)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(doAction)Jump();
            return true;
        }
        return false;
    }

   bool CheckForDash(bool doAction = true)
    {
        if (v == 0) return false;
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if(doAction)StartCoroutine(DashTiming(v));
            return true;
        }

        return false;
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
    #endregion

    #region executions
    //executions
    void Walk(float x, float z)
    {

        Vector3 WalkVector = transform.forward * z + transform.right * x;
        rb.AddForce(WalkVector * WalkForce * GetCurrentItem().MoveSpeedMult);
    }

    void Jump()
    {
        rb.AddForce(new Vector3(0f, JumpForce * GetCurrentItem().JumpForceMult, 0f));
        state_ = State.JUMPING;
    }

    IEnumerator DashTiming(float z)
    {
        state_ = State.DASHING;
        Vector3 DashVector = transform.forward * z;
        rb.AddForce(DashVector * DashForce * GetCurrentItem().MoveSpeedMult);
        yield return new WaitForSeconds(DashCoolDown / GetCurrentItem().DashCooldownReduction);
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
        yield return new WaitForSeconds(1/(BasicAttackSpeed*GetCurrentItem().AttackSpeedMult));
        state_ = State.IDLE;

    }

    void BasicAttack()
    {
        PlayAttackAudio();
        float actualAttackDamage = BasicAttackDamage * AttackDamageMultiplier * GetCurrentItem().AttackDamageMult;
        float actualAttackRadius = BasicAttackRadius * GetCurrentItem().AttackRangeMult;
        float actualAttackForce = BasicAttackForce * GetCurrentItem().AttackForceMult;
        FundamentalAttack(actualAttackDamage, actualAttackRadius, actualAttackForce, BasicAttackTransform);
    }

    IEnumerator JumpAttackTiming()
    {
        state_ = State.JUMPATTACK;
        JumpAttack();
        yield return new WaitForSeconds(JumpAttackDelay/GetCurrentItem().AttackSpeedMult);
        if(state_ == State.JUMPATTACK) state_ = State.JUMPING;

    }

    void JumpAttack()
    {
        PlayAttackAudio();
        float actualAttackDamage = JumpAttackDamage * AttackDamageMultiplier * GetCurrentItem().AttackDamageMult;
        float actualAttackRadius = JumpAttackRadius * GetCurrentItem().AttackRangeMult;
        float actualAttackForce = JumpAttackForce * GetCurrentItem().AttackForceMult;
        FundamentalAttack(actualAttackDamage, actualAttackRadius, actualAttackForce, JumpAttackTransform);
    }

    IEnumerator DashAttackTiming()
    {
        state_ = State.DASHATTACK;
        DashAttack();
        yield return new WaitForSeconds(DashAttackDelay/GetCurrentItem().AttackSpeedMult);
        state_ = State.IDLE;

    }

    void DashAttack()
    {
        PlayAttackAudio();
        float actualAttackDamage = DashAttackDamage * AttackDamageMultiplier * GetCurrentItem().AttackDamageMult;
        float actualAttackRadius = DashAttackRadius * GetCurrentItem().AttackRangeMult;
        float actualAttackForce = DashAttackForce * GetCurrentItem().AttackForceMult;
        FundamentalAttack(actualAttackDamage, actualAttackRadius, actualAttackForce, BasicAttackTransform);
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
            col.a = maxOpacity * shieldPowerCapped;
            shieldImage.color = col;
            shieldImage.transform.localScale = new Vector3(shieldPowerCapped, shieldPowerCapped/2, 1);
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

    void Counter()
    {
        Debug.Log("Counter!");
        FundamentalAttack(0, CounterRadius, CounterForce, BasicAttackTransform);
    }
    #endregion

    #region Attack/Damage
    //Attack and Damage
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
        float adjustedShieldPower = (state_==State.BLOCKING)? Mathf.Min(1, ShieldPower) : 0f;

        if (adjustedShieldPower >= 1)
            Counter();
        rb.AddForce(knockbackToDo);
        if (isRecovering) return;
        float realDamageToDo = damageToDo * Mathf.Max(0,(1f - adjustedShieldPower)); //prevent negative damage
        health -= realDamageToDo;
        if (realDamageToDo > 0)
            StartCoroutine(DamageDelay());
       // UpdateHealthBar(); //this may as well be in update?
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
    #endregion

    #region misc
    //Additional functions
    public void Blind(bool doBlind)
    {
        GetComponent<LookDirectionController>().camera.gameObject.SetActive(doBlind);
    }
    #endregion
}

