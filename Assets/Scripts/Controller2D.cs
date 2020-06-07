using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private int horizontalRayCount; // 4
    [SerializeField] private int verticalRayCount; // 4
    [SerializeField] private float maxClimbAngle; // 80f
    // Used to determine which objects to collide with
    [SerializeField] private LayerMask collisionMask;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    private BoxCollider2D collider; 
    private RaycastOrigins raycastOrigins;

    private CollisionInfo collisions;
    public CollisionInfo Collisions { get { return collisions; } }

    private const float skinWidth = 0.015f; // 0.015f

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 moveDistance)
    {
        UpdateRaycastOrigins();
        collisions.Reset();

        if (moveDistance.x != 0)
        {
            HorizontalCollisions(ref moveDistance);
        }
        if (moveDistance.y != 0)
        {
            VerticalCollisions(ref moveDistance);
        }

        transform.Translate(moveDistance);
    }

    // Changes in this method effect moveDistance Move method
    void HorizontalCollisions(ref Vector3 moveDistance)
    {
        float directionX = Mathf.Sign(moveDistance.x);
        float rayLength = Mathf.Abs(moveDistance.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            // If moving down, cast from bottom left corner
            // If moving up, cast from bottom right corner
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);


            // Set x moveDistance to amount needed to move from current position to the point which the ray collided with obstacle
            if (hit)
            {
                // Handle slopes
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= maxClimbAngle)
                {
                    float distanceToSlopeStart = 0;

                    // Starting to climb new slope
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        // Setup moveDistance for ClimbSlope so that
                        // ClimbSlope uses the x value once it actually reaches the slope
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveDistance.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveDistance, slopeAngle);
                    moveDistance.x += distanceToSlopeStart * directionX;
                }

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red, 0.01f);

                // If climbing slope, don't check the rest of the rays for collisions
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    // Reduce moveDistance so that we don't go through collision
                    moveDistance.x = (hit.distance - skinWidth) * directionX;

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
        float rayLength = Mathf.Abs(moveDistance.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {
            // If moving down, cast from bottom left corner
            // If moving up, cast from top left corner
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveDistance.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);


            // Set y moveDistance to amount needed to move from current position to the point which the ray collided with obstacle
            if (hit)
            {
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red, 0.01f);

                // Reduce moveDistance so that we don't go through collision
                moveDistance.y = (hit.distance - skinWidth) * directionY;

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
            } else
            {
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.green, 0.01f);
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

    void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    // Used to remove the accumulation of gravity and collisions left/right
    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public float slopeAngle, slopeAngleOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
