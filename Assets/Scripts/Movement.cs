using System;
using System.Collections.Specialized;
using UnityEngine;

public class Movement
{
    private float speed;
    private Vector3 prevVelocity;
    private Vector3 velocity;
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
    public void setVelocityY(float velocityY) { velocity.y = velocityY; }
    public void setVelocityX(float velocityX) { velocity.x = velocityX; }
    
    private float maxJumpHeight;

    // Determined by maxJumpHeight, timeToJumpApex
    private float jumpForce;
    private float gravity;

    private float timeToJumpApex;

    // X Velocity Smoothing Variables
    private float velocityXSmoothing;
    private float targetVelocityX;

    // Acceleration
    private float accelerationTimeAirborne;
    private float accelerationTimeGrounded;

    // Faster Falling Variables
    private float gravityDown;
    private bool reachedApex = true;
    public bool ReachedApex { get { return reachedApex; } set { reachedApex = value; } }
    private float maxHeightReached = Mathf.NegativeInfinity;
    private float startHeight = Mathf.NegativeInfinity;

    // Update Variables
    private float deltaTime = 0;
    public float DeltaTime { get { return deltaTime; } set { value = deltaTime; } }

    public Movement(float speed, float maxJumpHeight, float timeToJumpApex, float accelerationTimeAirborne, float accelerationTimeGrounded)
    {
        this.speed = speed;
        this.maxJumpHeight = maxJumpHeight;
        this.timeToJumpApex = timeToJumpApex;
        this.accelerationTimeAirborne = accelerationTimeAirborne;
        this.accelerationTimeGrounded = accelerationTimeGrounded;

        gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        gravityDown = gravity * 2;

        jumpForce = 2 * maxJumpHeight / timeToJumpApex;
    }

    public void CalculateUpdate(float h, float y)
    {
        if (!reachedApex && maxHeightReached > y)
        {
            // Used ONLY for Debugging
            float delta = maxHeightReached - startHeight;
            float error = maxJumpHeight - delta;
            // There is no error calculation when jump is not full. Aka, space bar is lifted up before reaching apex
            Debug.Log("Jump Result: startHeight:" + Math.Round(startHeight, 4) + ", maxHeightReached:" + Math.Round(maxHeightReached, 4) + ", delta:" + Math.Round(delta, 4) + ", error:" + Math.Round(error, 4) + ", jumpTimer:" + deltaTime + ", gravity:" + gravity + ", jumpForce:" + jumpForce + "\n\n");


            reachedApex = true;
            gravity = gravityDown;
        }
        maxHeightReached = Mathf.Max(y, maxHeightReached);

        targetVelocityX = h * speed;
    }

    public Vector3 CalculateDeltaPosition(float acceleration, float fixedDeltaTime)
    {
        prevVelocity = velocity;

        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            acceleration);
        velocity.y += gravity * fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * fixedDeltaTime;
        return deltaPosition;
    }

    public void DoubleGravity()
    {
        gravity = gravityDown;
    }

    public void ZeroVelocityY()
    {
        velocity.y = 0;
    }

    public void Jump(float startHeight)
    {
        deltaTime = 0;
        velocity.y = jumpForce;

        // Used for faster falling
        gravity = -2 * maxJumpHeight / Mathf.Pow(timeToJumpApex, 2);
        reachedApex = false;
        maxHeightReached = Mathf.NegativeInfinity;
        this.startHeight = startHeight;
    }
}
