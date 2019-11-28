using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroStateMachine : MonoBehaviour
{

    public float WalkForce = 20f;
    public float JumpForce = 500f;
    public float BasicAttackStartDelay = 0.0f;
    public float BasicAttackSpeed = 2f; //basic attacks per second
    private Rigidbody rb;
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

                if(Input.GetKeyDown(KeyCode.Mouse0))
                {

                }
                break;

            case State.WALKING:
                if(!CheckForMove())
                {
                   state_ = State.IDLE;
                }

                CheckForJump();
                break;

            case State.JUMPING:

                CheckForMove();

                break;

            case State.DASHING:
                break;

            case State.ATTACKING:
                break;

            case State.JUMPATTACK:
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

    void Walk(float x, float z)
    {

        Vector3 WalkVector = transform.forward * z + transform.right * x;
        rb.AddForce(WalkVector * WalkForce);
    }

    void CheckForJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Jump();
    }

    void Jump()
    {
        rb.AddForce(new Vector3(0f, JumpForce, 0f));
        state_ = State.JUMPING;
    }

    void OnCollisionEnter(Collision col)
    {
        if (state_ == State.JUMPING)
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
        //Something more elaborate here
        Debug.Log("Attack!");
    }
}
