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

    // Faster Falling Variables
    private float gravityDown;
    private bool reachedApex = true;
    public bool ReachedApex { get { return reachedApex; } set { reachedApex = value; } }
    private float maxHeightReached = Mathf.NegativeInfinity;
    private float startHeight = Mathf.NegativeInfinity;

    // Wall Jump Variables
    private float wallSlideSpeedMax;
    private float wallStickTime;
    private float timeToWallUnstick = 0;
    private Vector2 wallJumpClimb;
    private Vector2 wallJumpOff;
    private Vector2 wallLeap;

    // Update Variables
    private float deltaTime = 0;
    public float DeltaTime { get { return deltaTime; } set { value = deltaTime; } }

    public Movement(
        float speed, 
        float maxJumpHeight, 
        float timeToJumpApex, 
        float wallStickTime,
        float wallSlideSpeedMax,
        Vector2 wallJumpClimb,
        Vector2 wallJumpOff,
        Vector2 wallLeap
        )
    {
        this.speed = speed;
        this.maxJumpHeight = maxJumpHeight;
        this.timeToJumpApex = timeToJumpApex;
        this.wallStickTime = wallStickTime;
        this.wallSlideSpeedMax = wallSlideSpeedMax;
        this.wallJumpClimb = wallJumpClimb;
        this.wallJumpOff = wallJumpOff;
        this.wallLeap = wallLeap;

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


    public Vector3 CalculateVelocity(float fixedDeltaTime, float y)
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

        velocity.y += gravity * fixedDeltaTime;
        Vector3 deltaPosition = (prevVelocity + velocity) * 0.5f * fixedDeltaTime;
        return deltaPosition;
    }

    public void CalculateVelocityX(float x, float acceleration)
    {
        prevVelocity = velocity;

        targetVelocityX = x * speed;

        velocity.x = Mathf.SmoothDamp(
            velocity.x,
            targetVelocityX,
            ref velocityXSmoothing,
            acceleration);

    }

    public void CalculateWallSlide(float x, int wallDirX, float fixedDeltaTime)
    {
        if (velocity.y < -wallSlideSpeedMax)
        {
            velocity.y = -wallSlideSpeedMax;
        }

        if (timeToWallUnstick > 0)
        {
            velocityXSmoothing = 0;
            velocity.x = 0;

            if (x != wallDirX && x != 0)
            {
                timeToWallUnstick -= fixedDeltaTime;
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
        else
        {
            timeToWallUnstick = wallStickTime;
        }
    }

    public void CalculateWallJump(float x, int wallDirX)
    {
        if (wallDirX == x)
        {
            velocity.x = -wallDirX * wallJumpClimb.x;
            velocity.y = wallJumpClimb.y;
        }
        else if (x == 0)
        {
            velocity.x = -wallDirX * wallJumpOff.x;
            velocity.y = wallJumpOff.y;
        }
        else
        {
            velocity.x = -wallDirX * wallLeap.x;
            velocity.y = wallLeap.y;
        }
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
