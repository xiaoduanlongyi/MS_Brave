using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarPatrolState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
    }

    public override void LogicUpdate()
    {
        //chase (detect player and chase)
        if (currentEnemy.FindPlayer())
        {
            currentEnemy.SwitchState(NPCState.Chase);
        }


        //patrol (detect wall, wait and turn around)
        if (!currentEnemy.physicsCheck.isGrounded || (currentEnemy.physicsCheck.touchLeftWall && currentEnemy.faceDir.x < 0 || currentEnemy.physicsCheck.touchRightWall && currentEnemy.faceDir.x > 0))
        {
            currentEnemy.wait = true;
            currentEnemy.anim.SetBool("walk", false);
        }
        else
        {
            currentEnemy.anim.SetBool("walk", true);
        }
    }

    public override void PhysicsUpdate()
    {
       
    }

    public override void OnExit()
    {
        currentEnemy.lostTimeCounter = currentEnemy.lostTime;
        currentEnemy.anim.SetBool("walk", false);
    }  
}
