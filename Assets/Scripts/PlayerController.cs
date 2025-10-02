using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpSpeed = 2f;
    public GameObject groundObject;
    public bool isChecked;
    public GameObject[] sprites;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");

        // We want to change the velocity rather than transform
        GetComponent<Rigidbody2D>().velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);


        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            print("Jumping");
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        }
    }

    bool IsGrounded()
    {
        Collider2D groundCollider = groundObject.GetComponent<Collider2D>();
        Collider2D ourCollider = GetComponent<Collider2D>();
        return groundCollider.IsTouching(ourCollider);
    }
}
