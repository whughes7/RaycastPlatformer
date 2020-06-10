using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
    // Inspector Variables
    [SerializeField] private float maxClimbAngle = 80f; // 80f
    [SerializeField] private float maxDescendAngle = 75f; // 75f
    // Used to determine which objects to collide with
    [SerializeField] private LayerMask collisionMask;
    public LayerMask CollisionMask { get { return collisionMask; } set { collisionMask = value; } }

    private CollisionInfo collisions;
    public CollisionInfo Collisions { get { return collisions; } }

    public override void Start()
    {
        base.Start();
        collisions.faceDir = 1;
    }

    public static Controller2D CreateController(GameObject playerObj, LayerMask collisionMask)
    {
        Controller2D controller = playerObj.AddComponent<Controller2D>();
        controller.collisionMask = collisionMask;
        return controller;
    }

    public void Move(Vector3 moveDistance, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.moveDistanceOld = moveDistance;

        if (moveDistance.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(moveDistance.x);
        }

        if (moveDistance.y < 0)
        {
            DescendSlope(ref moveDistance);
        }
        HorizontalCollisions(ref moveDistance);
        if (moveDistance.y != 0)
        {
            VerticalCollisions(ref moveDistance);
        }

        transform.Translate(moveDistance);

        if (standingOnPlatform == true)
        {
            collisions.below = true;
        }

    }

    // Changes in this method effect moveDistance Move method
    void HorizontalCollisions(ref Vector3 moveDistance)
    {
        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(moveDistance.x) + SkinWidth;

        // Change to detect wall sliding
        if (Mathf.Abs(moveDistance.x) < SkinWidth)
        {
            rayLength = 2 * SkinWidth;
        }

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            // If moving down, cast from bottom left corner
            // If moving up, cast from bottom right corner
            Vector2 rayOrigin = (directionX == -1) ? RaycastOrigin.bottomLeft : RaycastOrigin.bottomRight;
            rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);


            // Set x moveDistance to amount needed to move from current position to the point which the ray collided with obstacle
            // Edge Case 1: hit.distance != 0 ensures movement not blocked by moving platform 
            if (hit && hit.distance != 0)
            {



                //// Handle slopes
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    // Avoid edge case where, steep down slope meets climbing slope
                    // Avoid slowing down in V collision
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        moveDistance = collisions.moveDistanceOld;
                    }

                    float distanceToSlopeStart = 0;

                    // Starting to climb new slope
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        // Setup moveDistance for ClimbSlope so that
                        // ClimbSlope uses the x value once it actually reaches the slope
                        distanceToSlopeStart = hit.distance - SkinWidth;
                        moveDistance.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveDistance, slopeAngle);
                    moveDistance.x += distanceToSlopeStart * directionX;
                }

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red, 0.01f);

                // If climbing slope, don't check the rest of the rays for collisions
                // Note: slopeAngle = 90 when colliding on box while going up slope
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    // Reduce moveDistance so that we don't go through collision
                    // Edge Case 1: If inside platform, hit.distance = 0. Therefore SkinWidth * directionX would 
                    // Edge Case 1: result in a small amount of movement opposite the input direction
                    moveDistance.x = (hit.distance - SkinWidth) * directionX;

                    // Set all ray lengths to the nearest hit ray
                    // Avoids clipping scenario
                    rayLength = hit.distance;

                    // Update moveDistance on y axis
                    // Avoids spurradic jittery horizontal collision while going up slope
                    // Set y such that, we are still on the slope AFTER we move with the above x distance
                    if (collisions.climbingSlope)
                    {
                        moveDistance.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveDistance.x);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
            // Used ONLY for Debugging
            else
            {
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.green, 0.01f);
            }
        }
    }

    // Changes in this method effect moveDistance inside Move method
    void VerticalCollisions(ref Vector3 moveDistance)
    {
        float directionY = Mathf.Sign(moveDistance.y);
        float rayLength = Mathf.Abs(moveDistance.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            // If moving down, cast from bottom left corner
            // If moving up, cast from top left corner
            Vector2 rayOrigin = (directionY == -1) ? RaycastOrigin.bottomLeft : RaycastOrigin.topLeft;
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + moveDistance.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);


            // Set y moveDistance to amount needed to move from current position to the point which the ray collided with obstacle
            if (hit)
            {
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red, 0.01f);

                // Reduce moveDistance so that we don't go through collision
                moveDistance.y = (hit.distance - SkinWidth) * directionY;

                // Set all ray lengths to the nearest hit ray
                // Avoids clipping scenario
                rayLength = hit.distance;

                // Update moveDistance on y axis
                // Avoids spurradic jittery horizontal collision while going up slope
                // Set y such that, we are still on the slope AFTER we move with the above x distance
                if (collisions.climbingSlope)
                {
                    // Known: theta, y
                    // Unknown: x
                    // tan(theta) = y/x
                    // x * tan(theta) = y
                    // x = y / tan(theta)
                    // Note: * Mathf.Sign(moveDistance.x) is to keep our direction
                    moveDistance.x = moveDistance.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveDistance.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
            // Used ONLY for Debugging
            else
            {
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.green, 0.01f);
            }
        }

        // Avoids getting stuck for a frame on larger angles while on a slope
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(moveDistance.x);
            rayLength = Mathf.Abs(moveDistance.x) + SkinWidth;
            // If moving left, then bottomLeft, If moving right, then bottomRight. Add up vector, multiply y distance
            Vector2 rayOrigin = ((directionX == -1) ? RaycastOrigin.bottomLeft : RaycastOrigin.bottomRight) + Vector2.up * moveDistance.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // Collided with new slope, update move distance and slope angle
                if (slopeAngle != collisions.slopeAngle)
                {
                    moveDistance.x = (hit.distance - SkinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 moveDistance, float slopeAngle)
    {
        float slopeMoveDistance = Mathf.Abs(moveDistance.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
        
        if (moveDistance.y <= climbVelocityY)
        {
            moveDistance.y = climbVelocityY;
            moveDistance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(moveDistance.x);
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            // Allow jump
            collisions.below = true;
        }
    }
    private void DescendSlope(ref Vector3 moveDistance)
    {
        float directionX = Mathf.Sign(moveDistance.x);
        Vector2 rayOrigin = (directionX == -1) ? RaycastOrigin.bottomRight : RaycastOrigin.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                // If moving down slope, determine which way slope is facing
                // Determine if moving in same direction
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    // Check if close enough to slope
                    if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveDistance.x))
                    {
                        // Close enough for slope to be in effect
                        float slopeMoveDistance = Mathf.Abs(moveDistance.x);

                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance;
                        moveDistance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * slopeMoveDistance * Mathf.Sign(moveDistance.x);
                        moveDistance.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }


    // Used to remove the accumulation of gravity and collisions left/right
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;

        // Avoid edge case where, steep down slope meets climbing slope
        // Avoid slowing down in V collision
        public Vector3 moveDistanceOld;
        public int faceDir;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
