using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour
{
    // Used to determine which objects to collide with
    public LayerMask collisionMask;

    const float skinWidth = 0.015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;
    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 moveDistance)
    {
        UpdateRaycastOrigins();

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
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red, 0.01f);

                moveDistance.x = (hit.distance - skinWidth) * directionX;
                // Set all ray lengths to the nearest hit ray
                // Avoids clipping scenario
                rayLength = hit.distance;
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

                moveDistance.y = (hit.distance - skinWidth) * directionY;
                // Set all ray lengths to the nearest hit ray
                // Avoids clipping scenario
                rayLength = hit.distance;
            } else
            {
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.green, 0.01f);
            }
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
}
