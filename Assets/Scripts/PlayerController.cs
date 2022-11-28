using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float walkAcceleration = 1f;
    public float airAcceleration = 0.25f;
    public float brakeDeceleration = 5f;
    public float jumpHeight = 5f;
    public int maxAirJumps = 2;
    int jumpsLeft;
    Vector2 velocity;
    BoxCollider2D boxCollider;
    Transform[] children;
    bool onGround;



    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        children = GetComponentsInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        float acceleration = onGround ? walkAcceleration : airAcceleration;
        float deceleration = onGround ? brakeDeceleration : 0;

        // Horizontal movement
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
        {
            velocity = new Vector2(Mathf.MoveTowards(velocity.x, moveInput * maxSpeed, acceleration), velocity.y);
            foreach(Transform child in children)
            {
                if(child == transform)
                {
                    continue;
                }
                child.localPosition = new Vector2(Mathf.Abs(child.localPosition.x) * Mathf.Sign(moveInput), child.localPosition.y); // Mirror children so that model "faces" the right direction
            }
        }
        else
        {
            velocity = new Vector2(Mathf.MoveTowards(velocity.x, moveInput * maxSpeed, deceleration), velocity.y);
        }

        // Vertical movement
        if (onGround)
        {
            velocity.y = 0;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y)); // Jump absolute height regardless of gravity
            }

        }
        else
        {
            if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
            {
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
                jumpsLeft--;
            }
        }
        velocity.y += Physics2D.gravity.y * Time.deltaTime; // Gravity
        
        
        transform.Translate(velocity * Time.deltaTime);

        onGround = false;

        // Collision management
        Collider2D[] collisions = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        foreach(Collider2D hit in collisions)
        {
            if(hit == boxCollider) // Skip checking if colliding with self
            {
                continue;
            }
            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);
            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);
                
                if(Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                {
                    onGround = true;
                    jumpsLeft = maxAirJumps;
                }
            }
        }
    }
}
