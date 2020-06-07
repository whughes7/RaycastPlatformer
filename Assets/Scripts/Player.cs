using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private float moveSpeed; // 6f
    // Assign values to these to determine gravity and jumpVelocity
    [SerializeField] private float maxJumpHeight; // 4f
    [SerializeField] private float timeToJumpApex; // 0.4f
    // X Velocity Smoothing Variables
    [SerializeField] private float accelerationTimeAirborne; // 0.2f
    [SerializeField] private float accelerationTimeGrounded; // 0.1f

    [SerializeField] private Text gravityText;
    [SerializeField] private Text jumpVelocityText;

    // Start Variables
    Controller2D controller;
    // Determined by maxJumpHeight, timeToJumpApex
    private float jumpForce;
    private float gravity;


    // Used only for debugging info (Except for OPTIONAL FEATURE: Faster falling section) START
    private float maxHeightReached = Mathf.NegativeInfinity;
    private float startHeight = Mathf.NegativeInfinity;
    // OPTIONAL FEATURE: Faster falling
    private bool reachedApex = true;
    // Used only for debugging info (Except for OPTIONAL FEATURE: Faster falling section) START

    // X Velocity Smoothing Variables
    private float velocityXSmoothing;
    private float targetVelocityX;

    // OPTIONAL FEATURE: Faster falling
    //private float gravityDown;

    // Update Variables
    private float jumpTimer = 0;
    Vector3 velocity;
    Vector3 prevVelocity;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        // OPTIONAL FEATURE: Faster falling
        //gravityDown = gravity * 2;

        jumpForce = 2 * maxJumpHeight / timeToJumpApex;

        gravityText.text = gravity.ToString();
        jumpVelocityText.text = jumpForce.ToString();
    }

    // Input dependent variables should be checked here because
    // Update is called more frequently than FixedUpdate()
    void Update()
    {

        // OPTIONAL FEATURE: Faster falling
        //if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    gravity = gravityDown;
        //}

        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            Jump();
        }

        // Used only for debugging info (Except for OPTIONAL FEATURE: Faster falling section) START
        if (!reachedApex && maxHeightReached > transform.position.y)
        {
            float delta = maxHeightReached - startHeight;
            float error = maxJumpHeight - delta;
            Debug.Log("Jump Result: startHeight:" + Math.Round(startHeight, 4) + ", maxHeightReached:" + Math.Round(maxHeightReached, 4) + ", delta:" + Math.Round(delta, 4) + ", error:" + Math.Round(error, 4) + ", jumpTimer:" + jumpTimer + ", gravity:" + gravity + ", jumpForce:" + jumpForce + "\n\n");
            reachedApex = true;
            // OPTIONAL FEATURE: Faster falling
            //gravity = gravityDown;
        }
        maxHeightReached = Mathf.Max(transform.position.y, maxHeightReached);
        // Used only for debugging info (Except for OPTIONAL FEATURE: Faster falling section) END


        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetVelocityX = input.x * moveSpeed;
    }

    // Movement and Physics dependent variables should exist here because
    // FixedUpdate (fixedDeltaTime) is called at a predictable interval which gives us 
    // independence from FPS and maintians predictability/reliability across multiple devices
    void FixedUpdate()
    {
        if (!controller.collisions.below && !reachedApex)
        {
            jumpTimer += Time.fixedDeltaTime;
        }

        prevVelocity = velocity;

        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * Time.fixedDeltaTime;
        controller.Move(deltaPosition);

        // Removes the accumulation of gravity
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        // Removes the continuous collision force left/right
        if (controller.collisions.left || controller.collisions.right)
        {
            velocity.x = 0;
        }
    }

    private void Jump()
    {
        jumpTimer = 0;
        velocity.y = jumpForce;


        // Used only for debugging info (Except for OPTIONAL FEATURE: Faster falling section) START
        maxHeightReached = Mathf.NegativeInfinity;
        startHeight = transform.position.y;
        reachedApex = false; // NOTE: If using optional feature. Don't have two reachedApex = false; parts
        // OPTIONAL FEATURE: Faster falling
        //gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        //reachedApex = false;
        // Used only for debugging info (Except for OPTIONAL FEATURE: Faster falling section) START
    }
}
