using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMaster : MonoBehaviour
{
    private static ItemMaster _instance;
    public static ItemMaster Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<ItemMaster>();
            return _instance;
        }
    }
    //by default, player default values * 2
    public float maxMoveSpeed = 40f;
    public float maxJumpPower = 700f;
    public float maxDashPower = 2000f;

    public float speedIncreasePerSecond = .1f;
    public float jumpIncrease = 17.5f;
    public float dashIncrease = 40f;


    public void TrainWalking()
    {
        float moveSpeed = GameMaster.Instance._player.WalkForce;
        moveSpeed += (speedIncreasePerSecond / 60f); //assuming this will be called every frame
        moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
        GameMaster.Instance._player.WalkForce = moveSpeed;
    }

    public void TrainJumping()
    {
       // Debug.Log("Training Jumping");
        float jumpForce = GameMaster.Instance._player.JumpForce;
        jumpForce += jumpIncrease; 
        jumpForce = Mathf.Min(jumpForce, maxJumpPower);
        GameMaster.Instance._player.JumpForce = jumpForce;
    }

    public void TrainDashing()
    {
       // Debug.Log("Training Dashing");
        float dashForce = GameMaster.Instance._player.DashForce;
        dashForce += dashIncrease;
        dashForce = Mathf.Min(dashForce, maxDashPower);
        GameMaster.Instance._player.DashForce = dashForce;
    }

}
