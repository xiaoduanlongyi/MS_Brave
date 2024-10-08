using UnityEngine;

public class SnailSkillState : BaseState
{
    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = 0;
        currentEnemy.anim.SetBool("walk", false);
        currentEnemy.anim.SetBool("hide", true);
        currentEnemy.anim.SetTrigger("skill");

        //when hide reset losttimecounter to avoid enter and exit patrol state repetitiously
        currentEnemy.lostTimeCounter = currentEnemy.lostTime;

        //snail becomes invulnaerable when enter into skill state
        currentEnemy.GetComponent<Character>().isInvulnerable = true;
        currentEnemy.GetComponent<Character>().invulnerableCounter = currentEnemy.lostTimeCounter;
    }

    public override void LogicUpdate()
    {
        //end hide
        if (currentEnemy.lostTimeCounter <= 0)
        {
            currentEnemy.SwitchState(NPCState.Patrol);
        }

        //将无敌时间倒数和hide时间倒数同步
        currentEnemy.GetComponent<Character>().invulnerableCounter = currentEnemy.lostTimeCounter;
    }

    public override void PhysicsUpdate()
    {

    }

    public override void OnExit()
    {
        currentEnemy.anim.SetBool("hide", false);
        currentEnemy.GetComponent<Character>().isInvulnerable = false;
    }
}
