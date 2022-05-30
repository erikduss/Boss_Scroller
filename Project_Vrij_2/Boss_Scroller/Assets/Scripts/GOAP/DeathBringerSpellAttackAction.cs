using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerSpellAttackAction : GOAPAction
{
	private bool attacked = false;
	private bool running = false;

	//This action has the effect of damaging the player
	//The only condition is that the player has to be close enough
	public DeathBringerSpellAttackAction() 
	{
		//addPrecondition ("playerDefending", true);
		addEffect("damagePlayer", true);
		cost = 200f;
	}

	public override void reset()
	{
		attacked = false;
		running = false;
		target = null;
	}

	public override bool isDone()
	{
		return attacked;
	}

	public override bool requiresInRange()
	{
		return true;
	}

	public override bool checkProceduralPrecondition(GameObject agent)
	{
		target = GameObject.FindGameObjectWithTag("Player");
		DeathBringerGOAP currA = agent.GetComponent<DeathBringerGOAP>();

		if (target != null && currA.stamina >= (cost))
		{ //to-do: make scaling system instead of magic number) 
			return true;
		}
		else
		{
			return false;
		}
	}

	public override bool perform(GameObject agent)
	{
        if (!running)
        {
			running = true;
			DeathBringerGOAP currA = agent.GetComponent<DeathBringerGOAP>();
			currA.stamina -= (cost);

			StartCoroutine(AttackDelay());
		}

		return attacked;
	}

	private IEnumerator AttackDelay()
    {
		Animator currAnim = GetComponentInParent<Animator>();
		currAnim.Play("Cast");
		yield return new WaitForSeconds(1);
		Debug.Log("Hitbox check");
		yield return new WaitForSeconds(.75f);
		currAnim.Play("Idle");
		yield return new WaitForSeconds(1f);
		running = false;
		attacked = true;
	} 
}
