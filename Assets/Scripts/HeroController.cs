using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class HeroController : MonoBehaviour, IDamageable
{

    public float WalkForce = 20f;
    public float JumpForce = 500f;
    public float DashForce = 2000f;
    public float DashCoolDown = 1.0f;
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
    public Transform BasicAttackTransform;
    public Transform JumpAttackTransform;

   // public AudioClip attackSound;
    //public AudioClip jumpSound;

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

                break;

            case State.WALKING:
                if(!CheckForMove())
                {
                   state_ = State.IDLE;
                }

                CheckForJump();
                CheckForDash();
                CheckForAttack();
                break;

            case State.JUMPING:

                CheckForMove();
                CheckForDash();
                CheckForJumpAttack();
                break;

            case State.DASHING:
                CheckForDashAttack();
                break;

            case State.ATTACKING:
                CheckForMove();
                break;

            case State.JUMPATTACK:
                CheckForMove();
                break;

            case State.DASHATTACK:
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

    void CheckForJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

   void CheckForDash()
    {
        if (v == 0) return;
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            StartCoroutine(DashTiming(v));
    }

    void CheckForAttack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartCoroutine(BasicAttackTiming());
        }
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

  void FundamentalAttack(float damageToDo, float radius, float attackForce, Transform t)
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>(); 
        for(int i = 0; i < enemies.Length; i++)
        {
            Transform enemyTransform = enemies[i].transform;
            if(Vector3.Distance(t.position, enemyTransform.position)<= radius)
            {
                Debug.Log(enemies[i].name);
                Vector3 attackVector = enemyTransform.position - transform.position;
                attackVector.Normalize();
                enemies[i].Damage(damageToDo, attackVector * attackForce);
            }

        }

    }

    void PlayAttackAudio()
    {
        aud.Play();
    }


    //IDamageable requirements. We don't want to be invincible now
    public void Damage(float damageToDo, Vector3 knockbackToDo)
    {

    }

    public void Kill()
    {

    }
}

