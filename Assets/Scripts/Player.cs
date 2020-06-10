using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private float moveSpeed = 6f; // 6f
    public float MoveSpeed { get { return moveSpeed; } }
    // Assign values to these to determine gravity and jumpVelocity
    [SerializeField] private float maxJumpHeight = 4f; // 4f
    [SerializeField] private float timeToJumpApex = 0.4f; // 0.4f
    // X Velocity Smoothing Variables
    [SerializeField] private float accelerationTimeAirborne = 0.2f; // 0.2f
    [SerializeField] private float accelerationTimeGrounded = 0.1f; // 0.1f
    [SerializeField] private float wallSlideSpeedMax = 3;
    [SerializeField] private Vector2 wallJumpClimb;
    [SerializeField] private Vector2 wallJumpOff;
    [SerializeField] private Vector2 wallLeap;
    [SerializeField] private float wallStickTime = 0.25f;

    private Input input;
    private enum Input
    {
        SpaceUp,
        SpaceDown,
        None
    }

    //[SerializeField] private Text gravityText = null;
    //[SerializeField] private Text jumpVelocityText = null;

    // Start Variables
    public Controller2D controller;
    public Movement movement;

    // Interfaces
    public IUnityService UnityService;


    // Utility for Unit Tests
    public static Player CreatePlayer(GameObject playerObj, Controller2D controller)
    {
        Player player = playerObj.AddComponent<Player>();
        player.controller = controller;
        return player;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();
        movement = new Movement(
            moveSpeed, 
            maxJumpHeight, 
            timeToJumpApex,
            wallStickTime,
            wallSlideSpeedMax,
            wallJumpClimb,
            wallJumpOff,
            wallLeap
            );

        if (UnityService == null)
            UnityService = new UnityService();

        //gravityText.text = gravity.ToString();
        //jumpVelocityText.text = jumpForce.ToString();
    }

    // Input dependent variables should be checked here because
    // Update is called more frequently than FixedUpdate()
    void Update()
    {
        float inputX = UnityService.GetAxisRaw("Horizontal");
        int wallDirX = (controller.Collisions.left) ? -1 : 1;

        movement.CalculateVelocityX(
            inputX,
            (controller.Collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne
            );

        bool wallSliding = false;
        if ((controller.Collisions.left || controller.Collisions.right) && !controller.Collisions.below && movement.Velocity.y < 0)
        {
            wallSliding = true;
            movement.CalculateWallSlide(inputX, wallDirX, UnityService.GetFixedDeltaTime());
        }

        if (UnityService.GetKeyDown(KeyCode.Space))
        {
            if (wallSliding)
            { 
                movement.CalculateWallJump(inputX, wallDirX);

            }
            if (controller.Collisions.below)
            {
                movement.Jump(transform.position.y);
            }
        }

        if (UnityService.GetKeyUp(KeyCode.Space))
        {
            movement.DoubleGravity();
        }

        controller.Move(
            movement.CalculateVelocity(
                    UnityService.GetFixedDeltaTime(),
                    transform.position.y
                )
            );

        // Removes the accumulation of gravity
        if (controller.Collisions.above || controller.Collisions.below)
        {
            movement.ZeroVelocityY();
        }
    }
}
