using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeChaseState : BaseState
{
    private Attack attack;
    private Vector3 target;
    private Vector3 moveDir;
    private bool isAttack;
    private float attackRateCounter = 0;

    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.chaseSpeed;
        attack = currentEnemy.GetComponent<Attack>();

        //when chase, reset losttime counter to avoid enter and exit state repetitiously
        currentEnemy.lostTimeCounter = currentEnemy.lostTime;

        currentEnemy.anim.SetBool("chase", true);
    }

    public override void LogicUpdate()
    {
        //end chase state
        if (currentEnemy.lostTimeCounter <= 0)
        { 
            currentEnemy.SwitchState(NPCState.Patrol);
        }

        //Time Counter
        attackRateCounter -= Time.deltaTime;

        //designated position is the player's position
        target = new Vector3(currentEnemy.attacker.position.x, currentEnemy.attacker.position.y + 1.5f, 0);

        //check if within attack range
        if (Mathf.Abs(target.x - currentEnemy.transform.position.x)<= attack.attackRange
            && Mathf.Abs(target.y - currentEnemy.transform.position.y) <= attack.attackRange)
        {
            //attack
            isAttack = true;
            if(!currentEnemy.isHurt) //enemy can be bounced back by an attack
                currentEnemy.rb.velocity = Vector2.zero;

            //Time Counter to attack
            if (attackRateCounter <= 0)
            {
                currentEnemy.anim.SetTrigger("attack");
                attackRateCounter = attack.attackRate;
            }
        }
        else //超出attack range继续chase
        {
            isAttack = false;
        } 

        moveDir = (target - currentEnemy.transform.position).normalized;

        //modify the facing direction
        if (moveDir.x > 0)
            currentEnemy.transform.localScale = new Vector3(-1, 1, 1);
        if (moveDir.x < 0)
            currentEnemy.transform.localScale = new Vector3(1, 1, 1);
    }

    public override void PhysicsUpdate()
    {
        //move
        if (!currentEnemy.isHurt && !currentEnemy.isDead && !isAttack)
        {
            currentEnemy.rb.velocity = moveDir * currentEnemy.currentSpeed * Time.deltaTime;
        }
    }

    public override void OnExit()
    {
        currentEnemy.anim.SetBool("chase", false);
    }
}
