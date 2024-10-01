using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float horizontal;
    [SerializeField] private float speed = 5f;
    private float jumpPower = 12f;
    private bool isFacingRight = true;

    //Wall Sliding
    private bool isWallSliding;
    private float wallSlidingSpeed = 1f;

    //Wall Jumping
    [SerializeField] private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    //private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(15f, 10f);

    //Dashing
    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 90f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 0.1f;
    
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Animator anim;
    private TrailRenderer tr;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        tr = GetComponent<TrailRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if(isDashing)
            return;

        horizontal = Input.GetAxisRaw("Horizontal");

        if(Input.GetKeyDown(KeyCode.Q))
        {
            attack();
        }

        WallSlide();
        WallJump();

        //if(!isWallJumping)
            Flip();

        //Set animator parameter
        anim.SetBool("run", horizontal != 0);
        anim.SetBool("grounded", isGrounded());

        if(Input.GetKeyDown(KeyCode.Space))   //Input.GetMouseButtonDown(0)
        {
            jump();
            anim.SetTrigger("jump");
        }
        if(Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        if(Input.GetMouseButtonDown(1) && canDash && horizontal != 0)
        {
            StartCoroutine(Dash());
        }
    }

    
    void FixedUpdate()
    {
        if(isDashing)
            return;

        if(!isWallJumping)
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private void jump()
    {
        if(isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            anim.SetTrigger("jump");
        }
    }

    

    private void attack()
    {
        anim.SetTrigger("attack");
    }

    private void Flip()
    {
        if(isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void WallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            //CancelInvoke(nameof(StopWallJumping));
        }
        else if(isGrounded() || horizontal != 0)
                isWallJumping = false;
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.Space) && wallJumpingCounter > 0f && !isGrounded())
        {
            isWallJumping = true;
           // rb.gravityScale = 0.5f * rb.gravityScale;
            rb.velocity = new Vector2(wallJumpingDirection * 28, 0);
            Vector2 wallJumpForce = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            //rb.velocity = Vector2.zero;
            rb.AddForce(wallJumpForce, ForceMode2D.Impulse);
            wallJumpingCounter = 0f;

            if(transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
            //Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    // private void StopWallJumping()
    // {
    //     isWallJumping = false;
    // }
    

    private void WallSlide()
    {
        if(onWall() && !isGrounded())
        {
            isWallSliding = true;

            if(Input.GetKey(KeyCode.S))
            {
                wallSlidingSpeed = 6f;
            }else
            {
                wallSlidingSpeed = 1f;
            }

            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            
            anim.SetTrigger("onWall");
        }
        else
        {
            isWallSliding = false;
        }
    }

    public bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center,
        boxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }

    public bool canThrowShuriken()
    {
        return !onWall();
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;

        if(isGrounded())
            anim.SetTrigger("slide");

        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }
}
