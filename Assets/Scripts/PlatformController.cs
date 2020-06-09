using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class PlatformController : RaycastController
{
    // Inspector Variables
    [SerializeField] private LayerMask passengerMask;

    [SerializeField] private Vector3[] localWaypoints;
    private Vector3[] globalWaypoints;

    [SerializeField] private float speed;
    [SerializeField] private bool cyclic;
    [SerializeField] private float waitTime;
    [Range(0, 2)]
    [SerializeField] private float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;

    List<PassengerMovement> passengerMovement;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }

    void Update()
    {
        UpdateRaycastOrigins();

        Vector3 moveDistance = CalculatePlatformMovement();
        CalculatePassengerMovement(moveDistance);
        MovePassengers(true);
        transform.Translate(moveDistance);
        MovePassengers(false);
    }

    float Ease(float x)
    {
        // If easeAmount = 0 then a is 1. Aka: no easing, linear line
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement()
    {
        // Don't move if wait time is being handled in this frame
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        // Reset to 0 each time it reaches globalWaypoints.Length
        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(
            globalWaypoints[fromWaypointIndex],
            globalWaypoints[toWaypointIndex]);

        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);

        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(
            globalWaypoints[fromWaypointIndex], 
            globalWaypoints[toWaypointIndex], 
            easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }

        return newPos - transform.position;
    }

    void MovePassengers(bool beforeMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            // One GetComponent call per passenger
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }

            if (passenger.moveBeforePlatform == beforeMovePlatform)
            {
                passengerDictionary[passenger.transform].Move(passenger.moveDistance, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 moveDistance)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

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

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
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
                        float pushY = -SkinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
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

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 moveDistance;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform transform, Vector3 moveDistance, bool standingOnPlatform, bool moveBeforePlatform)
        {
            this.transform = transform;
            this.moveDistance = moveDistance;
            this.standingOnPlatform = standingOnPlatform;
            this.moveBeforePlatform = moveBeforePlatform;
        }
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = 0.3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);            
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);            
            }
        }
    }
}
