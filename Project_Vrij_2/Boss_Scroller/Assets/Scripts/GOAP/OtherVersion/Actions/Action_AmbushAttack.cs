using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_AmbushAttack : Action_Base
{
    List<System.Type> SupportedGoals = new List<System.Type>(new System.Type[] { typeof(Goal_AmbushAttack) });

    Goal_AmbushAttack spellAttackGoal;
	private bool isRunning = false;
	private bool waiting = false;

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
        spellAttackGoal = (Goal_AmbushAttack)LinkedGoal;

        OnTick();
	}

    public override void OnDeactivated()
    {
        if (!waiting)
        {
			StartCoroutine(WaitForCoroutine());
		}
    }

    public override void OnTick()
    {
		if(spellAttackGoal.deathbringer.rbAI.velocity.magnitude > 0)
        {
            spellAttackGoal.deathbringer.rbAI.velocity = Vector2.zero;
            spellAttackGoal.deathbringer.animator.SetBool("Walking", false);
        }

		spellAttackGoal.deathbringer.attackFatigueTimer.StartTimer(spellAttackGoal.deathbringer.RandomFatigueTime(10.05f + spellAttackGoal.deathbringer.attackFatigueTime, 11.05f + spellAttackGoal.deathbringer.attackFatigueTime));
		spellAttackGoal.deathbringer.walkFatigueTimer.StartTimer(spellAttackGoal.deathbringer.walkFatigueTime);
		isRunning = true;
		StartCoroutine(AmbushAttack());
	}

	private IEnumerator WaitForCoroutine()
    {
		waiting = true;
        while (isRunning)
        {
			yield return null;
        }
		waiting = false;
		base.OnDeactivated();

		spellAttackGoal = null;
	}

	//Duration: 6.05 seconds
	private IEnumerator AmbushAttack()
	{
		//teleport location
		Vector3 teleportLocation = new Vector3(transform.position.x, transform.position.y, 0);

		float minX = -10;
		float maxX = 10;

		float minimumTeleportLength = 4f;

		bool canTeleportLeft = false;
		bool canTeleportRight = false;

		if (spellAttackGoal.deathbringer.player.transform.position.x > (minX + minimumTeleportLength)) canTeleportLeft = true;
		if (spellAttackGoal.deathbringer.player.transform.position.x < (maxX - minimumTeleportLength)) canTeleportRight = true;

		if (canTeleportRight && canTeleportLeft)
		{
			int rand = Random.Range(0, 2);

			if (rand == 0) //We're teleporting to the left Side
			{
				float randomX = Random.Range((spellAttackGoal.deathbringer.player.transform.position.x - minimumTeleportLength), (spellAttackGoal.deathbringer.player.transform.position.x - (minimumTeleportLength + 1)));
				teleportLocation.x = randomX;
			}
			else
			{
				float randomX = Random.Range((spellAttackGoal.deathbringer.player.transform.position.x + minimumTeleportLength), (spellAttackGoal.deathbringer.player.transform.position.x + (minimumTeleportLength + 1)));
				teleportLocation.x = randomX;
			}
		}
		else if (canTeleportLeft)
		{
			float randomX = Random.Range((spellAttackGoal.deathbringer.player.transform.position.x - minimumTeleportLength), (spellAttackGoal.deathbringer.player.transform.position.x - (minimumTeleportLength + 1)));
			teleportLocation.x = randomX;
		}
		else
		{
			float randomX = Random.Range((spellAttackGoal.deathbringer.player.transform.position.x + minimumTeleportLength), (spellAttackGoal.deathbringer.player.transform.position.x + (minimumTeleportLength + 1)));
			teleportLocation.x = randomX;
		}

		yield return StartCoroutine(Teleport(teleportLocation));

		FaceThePlayer();

		yield return StartCoroutine(Melee());
	}

	private void FaceThePlayer()
	{
		Vector3 direction = spellAttackGoal.deathbringer.player.transform.position - transform.position;

		float dir = direction.x;

		//This enemy is different than the player, the original sprites face different ways.
		if (dir < 0)
		{
			if (spellAttackGoal.deathbringer.renderer.flipX) //Character facing right after code
			{
				spellAttackGoal.deathbringer.renderer.flipX = false;
			}
		}
		else
		{
			if (!spellAttackGoal.deathbringer.renderer.flipX) //Character facing left after code
			{
				spellAttackGoal.deathbringer.renderer.flipX = true;
			}
		}
	}

	private IEnumerator Melee()
	{
		spellAttackGoal.deathbringer.rbAI.velocity = Vector2.zero;
		//spellAttackGoal.deathbringer.audioManager.PlayDeathBringerAttackSound();
		spellAttackGoal.deathbringer.animator.Play("Attack");
		spellAttackGoal.deathbringer.currentStrength = spellAttackGoal.deathbringer.strength;

		yield return new WaitForSeconds(1.1f);
		//spellAttackGoal.deathbringer.audioManager.PlayDeathBringerSwingSound();
		spellAttackGoal.deathbringer.meleeAttackTrigger.transform.gameObject.SetActive(true);

		if (!spellAttackGoal.deathbringer.renderer.flipX)
		{
			spellAttackGoal.deathbringer.meleeAttackTrigger.offset = new Vector2(-0.54f, -0.078f);
		}
		else
		{
			spellAttackGoal.deathbringer.meleeAttackTrigger.offset = new Vector2(0.54f, -0.078f);
		}
		yield return new WaitForSeconds(0.1f);
		if (!spellAttackGoal.deathbringer.renderer.flipX)
		{
			spellAttackGoal.deathbringer.meleeAttackTrigger.offset = new Vector2(-0.34f, -0.078f);
		}
		else
		{
			spellAttackGoal.deathbringer.meleeAttackTrigger.offset = new Vector2(0.34f, -0.078f);
		}
		yield return new WaitForSeconds(0.1f);
		if (!spellAttackGoal.deathbringer.renderer.flipX)
		{
			spellAttackGoal.deathbringer.meleeAttackTrigger.offset = new Vector2(-0.24f, -0.01f);
		}
		else
		{
			spellAttackGoal.deathbringer.meleeAttackTrigger.offset = new Vector2(0.24f, -0.01f);
		}
		yield return new WaitForSeconds(0.1f);
		spellAttackGoal.deathbringer.meleeAttackTrigger.transform.gameObject.SetActive(false);
		yield return new WaitForSeconds(.45f);
		
		yield return new WaitForSeconds(0.5f);
		isRunning = false;
	}

	private IEnumerator Teleport(Vector3 teleportLocation)
	{
		spellAttackGoal.deathbringer.rbAI.velocity = Vector2.zero;
		//spellAttackGoal.deathbringer.audioManager.PlayDeathBringerTeleportSound();
		spellAttackGoal.deathbringer.animator.Play("Teleport Start");
		yield return new WaitForSeconds(0.5f);
		spellAttackGoal.deathbringer.boxCollider.enabled = false;
		yield return new WaitForSeconds(0.5f);
		transform.position = teleportLocation;
		spellAttackGoal.deathbringer.renderer.enabled = false;
		yield return new WaitForSeconds(1);
		spellAttackGoal.deathbringer.renderer.enabled = true;
		spellAttackGoal.deathbringer.animator.Play("Teleport End");
		//spellAttackGoal.deathbringer.audioManager.PlayDeathBringerTeleportSound();
		yield return new WaitForSeconds(0.5f);
		spellAttackGoal.deathbringer.boxCollider.enabled = true;
		yield return new WaitForSeconds(0.5f);
	}
}
