using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private CapsuleCollider2D coll;
    private PlayerController playerController;
    private Rigidbody2D rb;

    [Header("Status")]
    public bool isGrounded;
    public bool touchLeftWall;
    public bool touchRightWall;
    public bool onWall;

    [Header("Check Parameter")]
    public bool manual;
    public bool isPlayer;
    public float checkRadius;
    public LayerMask groundLayer;
    public Vector2 bottomOffset;
    public Vector2 leftOffset;
    public Vector2 rightOffset;

    public void Awake()
    {
        coll = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (!manual)
        {
            rightOffset = new Vector2((coll.bounds.size.x + coll.offset.x) / 2, coll.bounds.size.y / 2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
        }
        if (isPlayer)
            playerController = GetComponent<PlayerController>();
    }

    public void Update()
    {
        Check();
    }

    public void Check()
    {
        //check if grounded
        if(onWall)
            isGrounded = Physics2D.OverlapCircle((Vector2)transform.position +
                new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y),checkRadius, groundLayer);
        else
            isGrounded = Physics2D.OverlapCircle((Vector2)transform.position +
                new Vector2(bottomOffset.x * transform.localScale.x, 0f), checkRadius, groundLayer);

        //change right and left offset when turn around
        rightOffset = new Vector3((coll.bounds.size.x / 2 + coll.offset.x * transform.localScale.x), coll.offset.y);
        leftOffset = new Vector3(-(coll.bounds.size.x / 2 - coll.offset.x * transform.localScale.x), rightOffset.y);

        //check wall
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(leftOffset.x, leftOffset.y), checkRadius, groundLayer);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(rightOffset.x, rightOffset.y), checkRadius, groundLayer);

        //on the wall
        if(isPlayer)
            onWall = (touchLeftWall && playerController.inputDirection.x < 0f || touchRightWall && playerController.inputDirection.x > 0f) && rb.velocity.y < 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x, bottomOffset.y), checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(leftOffset.x, leftOffset.y), checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(rightOffset.x, rightOffset.y), checkRadius);
    }
}
