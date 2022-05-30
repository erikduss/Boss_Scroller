using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_MoveToAttackRange : Action_Base
{
    List<System.Type> SupportedGoals = new List<System.Type>(new System.Type[] { typeof(Goal_MoveToAttackRange) });

    Goal_MoveToAttackRange moveToGoal;

    public override List<System.Type> GetSupportedGoals()
    {
        return SupportedGoals;
    }

    public override float GetCost()
    {
        return 0f;
    }

    public override void OnActivated(Goal_Base _linkedGoal)
    {
        base.OnActivated(_linkedGoal);
        
        // cache the chase goal
        moveToGoal = (Goal_MoveToAttackRange)LinkedGoal;

		MoveAgent();
	}

    public override void OnDeactivated()
    {
        base.OnDeactivated();

		moveToGoal = null;
    }

    public override void OnTick()
    {
		MoveAgent();
	}


	private float playerDistance()
	{
		var vec = transform.position - moveToGoal.deathbringer.player.transform.position;
		vec.y = 0;
		float dist = vec.magnitude;

		return dist;
	}
	private bool MoveAgent()
	{
		if (playerDistance() <= moveToGoal.deathbringer.minDist)
		{
			moveToGoal.deathbringer.rbAI.velocity = Vector2.zero;
			moveToGoal.deathbringer.animator.SetBool("Walking", false);
			return true;
		}
		else
		{
			Vector3 moveDirection = moveToGoal.deathbringer.player.transform.position - transform.position;

			Vector3 newPosition = moveDirection;

			float fixedSpeed;

			if (moveDirection.x > 0)
			{
				fixedSpeed = moveToGoal.deathbringer.speed;
			}
			else
			{
				fixedSpeed = -moveToGoal.deathbringer.speed;
			}

			newPosition.y = transform.position.y; //dont move the y

			moveToGoal.deathbringer.rbAI.velocity = new Vector2(fixedSpeed, moveToGoal.deathbringer.rbAI.velocity.y);
			moveToGoal.deathbringer.animator.SetBool("Walking", true);

			//This enemy is different than the player, the original sprites face different ways.
			if (moveToGoal.deathbringer.rbAI.velocity.x < 0)
			{
				if (moveToGoal.deathbringer.renderer.flipX) //Character facing right after code
				{
					moveToGoal.deathbringer.renderer.flipX = false;
				}
			}
			else
			{
				if (!moveToGoal.deathbringer.renderer.flipX) //Character facing left after code
				{
					moveToGoal.deathbringer.renderer.flipX = true;
				}
			}

			return false;
		}
	}
}
