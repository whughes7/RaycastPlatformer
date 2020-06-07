using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Path.GUIFramework;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float maxJumpHeight;
    [SerializeField] private float timeToJumpApex;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float accelerationTimeAirborne;
    [SerializeField] private float accelerationTimeGrounded;

    Vector3 velocity;
    Vector3 prevVelocity;

    private float maxHeightReached = Mathf.NegativeInfinity;
    private float startHeight = Mathf.NegativeInfinity;
    private float velocityXSmoothing;
    private bool reachedApex = true;

    float jumpForce;
    float gravity;
    private float gravityDown;

    private float jumpTimer = 0;

    Controller2D controller;

    [SerializeField] private Text gravityText;
    [SerializeField] private Text jumpVelocityText;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        gravityDown = gravity * 2;

        //gravityMultiplier = 
        jumpForce = 2 * maxJumpHeight / timeToJumpApex;

        gravityText.text = gravity.ToString();
        jumpVelocityText.text = jumpForce.ToString();
    }

    // Update is called once per frame
    void Update()
    {
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

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            Jump();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Debug.Log("min gravity"); 
            gravity = gravityDown;
        }

        if (!controller.collisions.below && !reachedApex)
        {
            jumpTimer += Time.fixedDeltaTime;
        }

        if (!reachedApex && maxHeightReached > transform.position.y)
        {
            float delta = maxHeightReached - startHeight;
            float error = maxJumpHeight - delta;
            Debug.Log("Jump Result: startHeight:" + Math.Round(startHeight, 4) + ", maxHeightReached:" + Math.Round(maxHeightReached, 4) + ", delta:" + Math.Round(delta,4) + ", error:" + Math.Round(error, 4) + ", jumpTimer:" + jumpTimer + ", gravity:" + gravity + ", jumpForce:" + jumpForce +"\n\n");
            reachedApex = true;
            gravity = gravityDown;
        }

        maxHeightReached = Mathf.Max(transform.position.y, maxHeightReached);

        prevVelocity = velocity;

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * Time.fixedDeltaTime;
        controller.Move(deltaPosition);
    }

    private void Jump()
    {
        jumpTimer = 0;
        maxHeightReached = Mathf.NegativeInfinity;
        gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        velocity.y = jumpForce;
        startHeight = transform.position.y;
        reachedApex = false;
    }
}
