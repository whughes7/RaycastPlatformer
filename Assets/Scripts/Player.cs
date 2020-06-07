using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private float moveSpeed; //6f
    // Assign values to these to determine gravity and jumpVelocity
    [SerializeField] private float maxJumpHeight; //4f
    [SerializeField] private float timeToJumpApex; //0.4f

    [SerializeField] private Text gravityText;
    [SerializeField] private Text jumpVelocityText;

    // Start Variables
    // Determined by maxJumpHeight, timeToJumpApex
    Controller2D controller;
    float jumpVelocity;
    float gravity;

    // Debugging Variables START
    private bool reachedApex = true;
    private float startHeight = Mathf.NegativeInfinity;
    private float maxHeightReached = Mathf.NegativeInfinity;
    // Debugging Variables END

    // X Velocity Smoothing Variables
    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;
    float velocityXSmoothing;
    private float targetVelocityX;

    // Update Variables
    Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        gravityText.text = gravity.ToString();
        jumpVelocityText.text = jumpVelocity.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.collisions.below)
        {
            velocity.y = jumpVelocity;

            // Used only for debugging info START
            startHeight = transform.position.y;
            maxHeightReached = Mathf.NegativeInfinity;
            reachedApex = false;
            // Used only for debugging info END
        }

        // Used only for debugging info START
        if (!reachedApex && maxHeightReached > transform.position.y)
        {
            float delta = maxHeightReached - startHeight;
            float error = maxJumpHeight - delta;
            Debug.Log("Jump Result: startHeight:" + Math.Round(startHeight, 4) + ", maxHeightReached:" + Math.Round(maxHeightReached, 4) + ", delta:" + Math.Round(delta, 4) + ", error:" + Math.Round(error, 4) + ", timeToJumpApex:" + timeToJumpApex + ", gravity:" + gravity + ", jumpVelocity:" + jumpVelocity + "\n\n");
            reachedApex = true;
        }
        maxHeightReached = Mathf.Max(transform.position.y, maxHeightReached);
        // Used only for debugging info END

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        targetVelocityX = input.x * moveSpeed;
    }

    void FixedUpdate()
    {
        // FixedUpdate fixedDeltaTime gives us a predictable error instead of variable error
        // This is due to the fact that FixedUpdate, is well, fixed in terms of intervals between calls
        // This is good if you account for the error when predicting maxHeightReached
        // Note: I do not account for the error in this code, but rather showcase that it is there
        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.fixedDeltaTime;
        controller.Move(velocity * Time.fixedDeltaTime);

        // NOTE: Must be moved from Update to FixedUpdate 
        // otherwise velocity.y can be zeroed out before jump is even activated
        // This is due to the fact that FixedUpdate is called less frequently than Update
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
}
