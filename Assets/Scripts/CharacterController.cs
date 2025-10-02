using System.Collections;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Rigidbody2D rb;

    [Header("Movement")]
    public float speed = 10f;
    public float jumpForce = 15f;
    public float slideSpeed = 5f;
    public float wallJumpLerp = 10f;
    public float dashSpeed = 20f;

    [Header("Jump Feel")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Collision Detection")]
    public LayerMask groundLayer;
    public Vector2 bottomOffset = new Vector2(0, -0.5f);
    public Vector2 rightOffset = new Vector2(0.4f, 0);
    public Vector2 leftOffset = new Vector2(-0.4f, 0);
    public float collisionRadius = 0.25f;

    [Header("State")]
    public bool canMove = true;
    public bool wallGrab;
    public bool wallSlide;
    public bool isDashing;

    private bool onGround;
    private bool onWall;
    private bool onRightWall;
    private bool onLeftWall;
    private int wallSide;
    private bool wallJumped;
    private bool groundTouch;
    private bool hasDashed;
    private int side = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckCollisions();
        HandleJumpPhysics();

        float x = Input.GetAxis("Horizontal");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");

        // Core movement
        Walk(new Vector2(x, 0));
        HandleWallGrab(x);
        HandleWallSlide(x);
        HandleJump();
        HandleDash(xRaw, yRaw);
        
        // State management
        HandleGroundTouch();
        UpdateFacingDirection(x);
    }

    void CheckCollisions()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);
        onWall = onRightWall || onLeftWall;
        wallSide = onRightWall ? -1 : 1;
    }

    void HandleJumpPhysics()
    {
        // Fall faster and allow variable jump height
        if (rb.velocity.y < 0)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }

    void Walk(Vector2 dir)
    {
        if (!canMove || wallGrab) return;

        // Lerp movement after wall jump for smooth momentum shift
        if (!wallJumped)
            rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        else
            rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(dir.x * speed, rb.velocity.y), wallJumpLerp * Time.deltaTime);
    }

    void HandleWallGrab(float x)
    {
        if (onWall && Input.GetButton("Fire3") && canMove) {
            wallGrab = true;
            wallSlide = false;
        }
        if (Input.GetButtonUp("Fire3") || !onWall || !canMove) {
            wallGrab = false;
            wallSlide = false;
        }

        if (wallGrab && !isDashing) {
            rb.gravityScale = 0;
            if (x > 0.2f || x < -0.2f)
                rb.velocity = new Vector2(rb.velocity.x, 0);

            // Climb slower when moving up
            float speedModifier = Input.GetAxis("Vertical") > 0 ? 0.5f : 1f;
            rb.velocity = new Vector2(rb.velocity.x, Input.GetAxis("Vertical") * speed * speedModifier);
        } else {
            rb.gravityScale = 3;
        }
    }

    void HandleWallSlide(float x)
    {
        if (onWall && !onGround && x != 0 && !wallGrab) {
            wallSlide = true;
            if (!canMove) return;

            // Keep horizontal velocity if bouncing off wall
            bool pushingWall = (rb.velocity.x > 0 && onRightWall) || (rb.velocity.x < 0 && onLeftWall);
            float push = pushingWall ? 0 : rb.velocity.x;
            rb.velocity = new Vector2(push, -slideSpeed);
        } else {
            wallSlide = false;
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump")) {
            if (onGround)
                Jump(Vector2.up);
            else if (onWall && !onGround)
                WallJump();
        }
    }

    void Jump(Vector2 dir)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;
    }

    void WallJump()
    {
        if ((side == 1 && onRightWall) || (side == -1 && !onRightWall))
            side *= -1;

        // Brief movement lock for cleaner wall jump
        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(0.1f));

        Vector2 wallDir = onRightWall ? Vector2.left : Vector2.right;
        Jump((Vector2.up / 1.5f + wallDir / 1.5f));
        wallJumped = true;
    }

    void HandleDash(float xRaw, float yRaw)
    {
        if (Input.GetButtonDown("Fire1") && !hasDashed && (xRaw != 0 || yRaw != 0)) {
            hasDashed = true;
            rb.velocity = Vector2.zero;
            rb.velocity += new Vector2(xRaw, yRaw).normalized * dashSpeed;
            StartCoroutine(DashCoroutine());
        }
    }

    void HandleGroundTouch()
    {
        // Reset abilities on landing
        if (onGround && !groundTouch) {
            hasDashed = false;
            isDashing = false;
            groundTouch = true;
        }
        if (!onGround && groundTouch)
            groundTouch = false;

        if (onGround && !isDashing)
            wallJumped = false;
    }

    void UpdateFacingDirection(float x)
    {
        if (wallGrab || wallSlide || !canMove) return;

        if (x > 0)
            side = 1;
        else if (x < 0)
            side = -1;
    }

    IEnumerator DashCoroutine()
    {
        StartCoroutine(GroundDashReset());
        rb.gravityScale = 0;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(0.3f);

        rb.gravityScale = 3;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDashReset()
    {
        // Allow dash refresh if we touch ground mid-dash
        yield return new WaitForSeconds(0.15f);
        if (onGround)
            hasDashed = false;
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
    }
}
