using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour
{
    // Assign values to these to determine gravity and jumpVelocity
    float jumpHeight = 2f;
    float timeToJumpApex = 0.4f;

    // Determined by the above two variables
    float jumpVelocity;
    float gravity;

    float accelerationTimeAirborne = 0.2f;
    float accelerationTimeGrounded = 0.1f;

    float moveSpeed = 6f;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;

    [SerializeField] private Text gravityText;
    [SerializeField] private Text jumpVelocityText;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        gravityText.text = gravity.ToString();
        jumpVelocityText.text = jumpVelocity.ToString();
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
            velocity.y = jumpVelocity;
        }

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(
            velocity.x, 
            targetVelocityX,
            ref velocityXSmoothing, 
            (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
