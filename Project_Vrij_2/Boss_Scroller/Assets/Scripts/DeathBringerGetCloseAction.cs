using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerGetCloseAction : GOAPAction
{
	private bool isClose = false;
	private float minDist = 1.5f;

	public int speed;
	protected float terminalSpeed;
	protected float initialSpeed;
	protected float acceleration;

	//This action has the effect of being near the player
	//no pre condition at the moment.
	public DeathBringerGetCloseAction()
	{
		//addPrecondition ("playerDefending", true);
		addEffect("isInMeleeRange", true);
		cost = 5f;
	}

	public override void reset()
	{
		isClose = false;
		target = null;
	}

	public override bool isDone()
	{
		return isClose;
	}

	public override bool requiresInRange()
	{
		return false;
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
	public void setSpeed(float val)
	{
		terminalSpeed = val / 10;
		initialSpeed = (val / 10) / 2;
		acceleration = (val / 10) / 4;
		return;
	}

	public override bool perform(GameObject agent)
	{
		target = GameObject.FindGameObjectWithTag("Player");

		float dist = Vector3.Distance(transform.position, target.transform.position);

		DeathBringerGOAP currA = agent.GetComponent<DeathBringerGOAP>();
		currA.stamina -= (cost);

		Animator currAnim = GetComponentInParent<Animator>();

		if (dist <= minDist)
		{
			currAnim.SetBool("Walking", false);
			isClose = true;
			return isClose;
		}
		else
		{
			currAnim.SetBool("Walking", true);

			Vector3 moveDirection = target.transform.position - transform.position;

			setSpeed(speed);

			if (initialSpeed < terminalSpeed)
			{
				initialSpeed += acceleration;
			}

			Vector3 newPosition = moveDirection * initialSpeed * Time.deltaTime;

			newPosition.y = transform.position.y; //dont move the y

			transform.position += newPosition;

			return false;
		}
	}
}
