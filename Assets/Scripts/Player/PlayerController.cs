 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Monitor Event")]
    public SceneLoadEventSO loadEvent;
    public VoidEventSO afterSceneLoadedEvent;

    public PlayerInputControl inputControl;
    public Vector2 inputDirection;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private CapsuleCollider2D coll;
    private PlayerAnimation playerAnimation;
    private Character character;

    [Header("Basic Movement")]
    public float speed;
    private float runSpeed;
    private float walkSpeed => speed / 2.5f;
    public float jumpForce;
    public float wallJumpForce;
    public float hurtForce;
    public float slideDistance;
    public float slideSpeed;
    public int slidePowerCost;

    private Vector2 originalSize;
    private Vector2 originalOffset;

    [Header("Physical Material")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;

    [Header("Status")]
    public bool isCrouch;
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool wallJump;
    public bool isSlide;

    private void Awake()
    {
        //调取component
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        coll = GetComponent<CapsuleCollider2D>();
        playerAnimation = GetComponent<PlayerAnimation>();
        character = GetComponent<Character>();

        originalSize = coll.size;
        originalOffset = coll.offset;

        inputControl = new PlayerInputControl();
        //jump
        inputControl.Gameplay.Jump.started += Jump;

        //force walk
        #region Force Walk
        runSpeed = speed;

        inputControl.Gameplay.WalkButton.performed += ctx =>
        {
            if (physicsCheck.isGrounded)
                speed = walkSpeed;
        };

        inputControl.Gameplay.WalkButton.canceled += ctx =>
        {
            if(physicsCheck.isGrounded)
                speed = runSpeed;
        };
        #endregion

        //attack
        inputControl.Gameplay.Attack.started += PlayerAttack;

        //slide
        inputControl.Gameplay.Slide.started += Slide;
    }

    private void OnEnable()
    {
        inputControl.Enable();
        loadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        inputControl.Disable();
        loadEvent.LoadRequestEvent -= OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
    }

    private void Update()
    {
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();
        CheckState();
    }

    private void FixedUpdate()
    {
        if(!isHurt && !isAttack && !isSlide)
            Move(); 
    }

    //When scene is loading, dsable player control
    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        inputControl.Gameplay.Disable();
    }

    //when scene is loaded, enable player control
    private void OnAfterSceneLoadedEvent()
    {
        inputControl.Gameplay.Enable();
    }

    public void Move()
    { 
        //normal move
        if(!isCrouch && !wallJump)
            rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);

        int faceDir = (int)transform.localScale.x;

        //flip 
        if (inputDirection.x < 0)
            faceDir = -1;
        if (inputDirection.x > 0)
            faceDir = 1;
        transform.localScale = new Vector3(faceDir, 1, 1);

        //crouch
        isCrouch = inputDirection.y < -0.5f && physicsCheck.isGrounded;
        if (isCrouch)
        {
            //change the size and offset of collider
            coll.size = new Vector2(0.7f, 1.5f);
            coll.offset = new Vector2(-0.05f, 0.75f);
        }
        else
        {
            //set back the orignial size and offset of collider
            coll.size = originalSize;
            coll.offset = originalOffset;
        }
    }

    //jump
    private void Jump(InputAction.CallbackContext obj)
    {
        if (physicsCheck.isGrounded) //jump from ground
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            GetComponent<AudioDefination>()?.PlayAudioClip(); //play jump fx

            //stop slide coroutine
            isSlide = false;
            StopAllCoroutines();
        }
        else if (physicsCheck.onWall) //jump from wall
        {
            rb.AddForce(new Vector2(-inputDirection.x, 2f) * wallJumpForce, ForceMode2D.Impulse);
            wallJump = true;
            GetComponent<AudioDefination>()?.PlayAudioClip(); //play jump fx
        }
    }

    //attack
    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        playerAnimation.PlayAttack();
        isAttack = true;
    }

    //slide
    private void Slide(InputAction.CallbackContext obj)
    {
        if (!isSlide && physicsCheck.isGrounded && character.currentPower >= slidePowerCost)
        {
            isSlide = true;
            //slide target position
            var targetPos = new Vector3(transform.position.x + slideDistance * transform.localScale.x, transform.position.y);

            //switch player's layer to avoid enemies' attack
            gameObject.layer = LayerMask.NameToLayer("Enemy");
            StartCoroutine(TriggerSlide(targetPos));

            character.OnSlide(slidePowerCost);
        }
    }

    private IEnumerator TriggerSlide(Vector3 target)
    {
        do
        {
            yield return null;

            //during slide, cliff stop
            if (!physicsCheck.isGrounded)
                break;

            //during slide, wall stop
            if (physicsCheck.touchLeftWall && transform.localScale.x < 0f || physicsCheck.touchRightWall && transform.localScale.x > 0f)
            {
                isSlide = false;
                break;
            }

            rb.MovePosition(new Vector2(transform.position.x + slideSpeed * transform.localScale.x, transform.position.y));

        } while (MathF.Abs(target.x - transform.position.x) > 0.2f);

        isSlide = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }



    #region Unity Event
    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero; //reset velocity to clear out inertia effect
        Vector2 dir = new Vector2(transform.position.x - attacker.transform.position.x, 0).normalized; //bounce back direction
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse); //bounce back
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
    }
    #endregion

    private void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGrounded ? normal : wall;

        //lower the speed of sliding on wall
        if (physicsCheck.onWall)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2f);
        else
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);

        //after death enemies can't attack player
        if (isDead || isSlide)
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        else
            gameObject.layer = LayerMask.NameToLayer("Player");

        if (wallJump && rb.velocity.y < 8f)
        {
            wallJump = false;
        }
    }
}
