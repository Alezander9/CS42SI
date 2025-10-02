using UnityEngine;

public class BetterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    // public GameObject groundObject;

    private Rigidbody2D rb;
    // private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // CheckGrounded();
        // if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        // {
        //     print("Jumping");
        //     rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        // }
    }

    // void CheckGrounded()
    // {
    //     if (groundObject != null)
    //     {
    //         Collider2D groundCollider = groundObject.GetComponent<Collider2D>();
    //         Collider2D playerCollider = GetComponent<Collider2D>();
            
    //         if (groundCollider != null && playerCollider != null)
    //         {
    //             isGrounded = playerCollider.IsTouching(groundCollider);
    //         }
    //     }
    // }
}
