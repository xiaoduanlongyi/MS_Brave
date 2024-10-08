using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(PhysicsCheck))]
public class Enemy : MonoBehaviour
{
    [HideInInspector]public Animator anim;
    [HideInInspector]public Rigidbody2D rb;
    [HideInInspector]public PhysicsCheck physicsCheck; 

    [Header("Basic Parameter")]
    public float normalSpeed;
    public float chaseSpeed;
    public float currentSpeed;
    public Vector3 faceDir;
    public float hurtForce;
     
    public Transform attacker;
    [HideInInspector]public Vector3 spawnPoint;

    [Header("Detect")]
    public Vector2 centerOffset;
    public Vector2 checkSize;
    public float checkDistance;
    public LayerMask attackLayer;

    [Header("Timer")]
    public float waitTime;
    public float waitTimeCounter;
    public bool wait;
    public float lostTime;
    public float lostTimeCounter;

    [Header("Status")]
    public bool isHurt;
    public bool isDead;

    //enemies' states
    protected BaseState currentState;
    protected BaseState patrolState;
    protected BaseState chaseState;
    protected BaseState skillState;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>(); 
        currentSpeed = normalSpeed;
        waitTimeCounter = waitTime;
        spawnPoint = transform.position;
    }

    private void OnEnable()
    {
        currentState = patrolState;
        currentState.OnEnter(this);
    }

    private void Update()
    {
        faceDir = new Vector3(-transform.localScale.x, 0, 0);

        currentState.LogicUpdate();

        TimeCounter();
    }

    private void FixedUpdate()
    {
        if (!isHurt && !isDead && !wait)
        Move();
        currentState.PhysicsUpdate();
    }

    private void OnDisable()
    {
        currentState.OnExit();
    }

    public virtual void Move()
    {
        //在snail premove和recover阶段不让他移动
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("SnailPreMove") &&
            !anim.GetCurrentAnimatorStateInfo(0).IsName("SnailRecover"))
            rb.velocity = new Vector2(currentSpeed * faceDir.x * Time.deltaTime, rb.velocity.y);
    }

    //计时器
    public void TimeCounter()
    {
        if (wait) //wait before turn around
        {
            waitTimeCounter -= Time.deltaTime;
            if (waitTimeCounter <= 0)
            {
                wait = false;
                waitTimeCounter = waitTime;
                //turn around
                transform.localScale = new Vector3(faceDir.x, 1, 1);
            }
        }

        if (!FindPlayer() && lostTimeCounter > 0) //time counter on losing chase status, lose chase code see "boarchasestate"
        {
            lostTimeCounter -= Time.deltaTime;
        }
        else if (FindPlayer()) //refill losttime counter when detect player again
        {
            lostTimeCounter = lostTime;
        }
    }

    //put a box overlap in front of the enemy's direction to detect player
    public virtual bool FindPlayer()
    {
        return Physics2D.BoxCast(transform.position + (Vector3)centerOffset, checkSize, 0, faceDir, checkDistance, attackLayer);
    }

    //transition between different states
    public void SwitchState(NPCState state)
    {
        var newState = state switch
        {
            NPCState.Patrol => patrolState,
            NPCState.Chase => chaseState,
            NPCState.Skill => skillState,
            _ => null
        };

        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter(this);
    }

    public virtual Vector3 GetNewPoint()
    {
        return transform.position; 
    }

    #region 事件执行方法
    public void OnTakeDamage(Transform attackTrans)
    {
        attacker = attackTrans;
        //turn around
        if (attackTrans.transform.position.x - transform.position.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        if (attackTrans.transform.position.x - transform.position.x < 0)
            transform.localScale = new Vector3(1, 1, 1);
        //when attacked, bounce back
        isHurt = true;
        anim.SetTrigger("hurt");
        Vector2 dir = new Vector2(transform.position.x - attackTrans.transform.position.x, 0).normalized;
        rb.velocity = new Vector2(0, rb.velocity.y);
        StartCoroutine(OnHurt(dir));
    }

    private IEnumerator OnHurt(Vector2 dir)
    {
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.45f);
        isHurt = false;
    }

    public void OnDie()
    {
        gameObject.layer = 2;
        anim.SetBool("dead", true);
        isDead = true;
    }

    public void DestroyAfterAnimation()
    {
        Destroy(this.gameObject);
    }
    #endregion

    public virtual void OnDrawGizmosSelected() //draw findplayer
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3)centerOffset + new Vector3(checkDistance * -transform.localScale.x, 0, 0), 0.2f);
    }

}
