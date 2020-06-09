using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    // Inspector Variables
    [SerializeField] private LayerMask passengerMask;
    [SerializeField] private Vector3 move;

    public override void Start()
    {
        base.Start();
    }

    void Update()
    {
        UpdateRaycastOrigins();

        Vector3 moveDistance = move * Time.deltaTime;
        MovePassengers(moveDistance);
        transform.Translate(moveDistance);
    }

    void MovePassengers(Vector3 moveDistance)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();

        float directionX = Mathf.Sign(moveDistance.x);
        float directionY = Mathf.Sign(moveDistance.y);

        // Vertical moving platform
        if (moveDistance.y != 0)
        {
            float rayLength = Mathf.Abs(moveDistance.y) + SkinWidth;

            for (int i = 0; i < VerticalRayCount; i++)
            {
                // If moving down, cast from bottom left corner
                // If moving up, cast from top left corner
                Vector2 rayOrigin = (directionY == -1) ? RaycastOrigin.bottomLeft : RaycastOrigin.topLeft;
                rayOrigin += Vector2.right * (VerticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);

                // Passenger found
                // How far to move passenger
                // Close gap between passenger and platform
                // Then, move passenger by the rest of the moveDistance
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? moveDistance.x : 0;
                        float pushY = moveDistance.y - (hit.distance - SkinWidth) * directionY;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (moveDistance.x != 0)
        {
            float rayLength = Mathf.Abs(moveDistance.x) + SkinWidth;

            for (int i = 0; i < HorizontalRayCount; i++)
            {
                // If moving down, cast from bottom left corner
                // If moving up, cast from bottom right corner
                Vector2 rayOrigin = (directionX == -1) ? RaycastOrigin.bottomLeft : RaycastOrigin.bottomRight;
                rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);

                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = moveDistance.x - (hit.distance - SkinWidth) * directionX;
                        float pushY = 0;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }

        // Passenger on top of horizontally or downward moving platform
        // Fixes jittery movement when downward moving platform is fast
        if (directionY == -1 || moveDistance.y == 0 && moveDistance.x != 0)
        {
            float rayLength = SkinWidth * 2;

            for (int i = 0; i < VerticalRayCount; i++)
            {
                // If moving down, cast from bottom left corner
                // If moving up, cast from top left corner
                Vector2 rayOrigin =  RaycastOrigin.topLeft + Vector2.right * (VerticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);

                // Passenger found
                // How far to move passenger
                // Close gap between passenger and platform
                // Then, move passenger by the rest of the moveDistance
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = moveDistance.x;
                        float pushY = moveDistance.y;

                        hit.transform.Translate(new Vector3(pushX, pushY));
                    }
                }
            }
        }
    }
}
