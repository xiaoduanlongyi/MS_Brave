
using UnityEngine;

public class BeePatrolState : BaseState
{
    private Vector3 target;
    private Vector3 moveDir;

    public override void OnEnter(Enemy enemy)
    {
        currentEnemy = enemy;
        currentEnemy.currentSpeed = currentEnemy.normalSpeed;
        target = enemy.GetNewPoint();
    }

    public override void LogicUpdate()
    {
        //chase (detect player and chase)
        if (currentEnemy.FindPlayer())
        {
            currentEnemy.SwitchState(NPCState.Chase);
        }

        //check if already move to target position
        if (Mathf.Abs(target.x - currentEnemy.transform.position.x)<0.1f
            && Mathf.Abs(target.y-currentEnemy.transform.position.y)<0.1f)
        {
            //wait and get a new target position
            currentEnemy.wait = true;
            target = currentEnemy.GetNewPoint();
        }

        moveDir = (target - currentEnemy.transform.position).normalized;

        //modify the facing direction
        if (moveDir.x > 0)
            currentEnemy.transform.localScale = new Vector3(-1, 1, 1);
        if (moveDir.x < 0)
            currentEnemy.transform.localScale = new Vector3(1, 1, 1);
    }

    public override void  PhysicsUpdate()
    {
        if (!currentEnemy.isHurt && !currentEnemy.isDead && !currentEnemy.wait)
        {
            currentEnemy.rb.velocity = moveDir * currentEnemy.currentSpeed * Time.deltaTime;
        }
        else
        {
            currentEnemy.rb.velocity = Vector2.zero;
        }
    }

    public override void OnExit()
    {

    }
}
