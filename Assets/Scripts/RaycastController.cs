using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    // Inspector Variables
    [SerializeField] private int horizontalRayCount = 4; // 4
    [SerializeField] private int verticalRayCount = 4; // 4

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    new private BoxCollider2D collider;
    private RaycastOrigins raycastOrigin;

    private const float skinWidth = 0.015f; // 0.015f

    // Getters
    public int HorizontalRayCount { get { return horizontalRayCount; } }
    public int VerticalRayCount { get { return verticalRayCount; } }
    public float HorizontalRaySpacing { get { return horizontalRaySpacing; } }
    public float VerticalRaySpacing { get { return verticalRaySpacing; } }
    public RaycastOrigins RaycastOrigin { get { return raycastOrigin; } }
    public float SkinWidth { get { return skinWidth; } }

    // Start is called before the first frame update
    public virtual void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigin.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigin.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigin.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigin.topRight = new Vector2(bounds.max.x, bounds.max.y);
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

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
