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

	private Animator animator;
	private Rigidbody2D rbAI;
	private SpriteRenderer renderer;

	//This action has the effect of being near the player
	//no pre condition at the moment.
	public DeathBringerGetCloseAction()
	{
		//addPrecondition ("playerDefending", true);
		addEffect("isInMeleeRange", true);
		cost = 5f;
	}

    public void Start()
    {
		target = GameObject.FindGameObjectWithTag("Player");
		animator = gameObject.GetComponent<Animator>();
		rbAI = gameObject.GetComponent<Rigidbody2D>();
		renderer = gameObject.GetComponent<SpriteRenderer>();
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
		return MoveAgent();
	}

	private float playerDistance()
	{
		var vec = transform.position - target.transform.position;
		vec.y = 0;
		float dist = vec.magnitude;

		return dist;
	}

	private bool MoveAgent()
	{
		if (playerDistance() <= minDist)
		{
			rbAI.velocity = Vector2.zero;
			animator.SetBool("Walking", false);
			return true;
		}
		else
		{
			setSpeed(speed);

			if (initialSpeed < terminalSpeed)
			{
				initialSpeed += acceleration;
			}

			Vector3 moveDirection = target.transform.position - transform.position;

			Vector3 newPosition = moveDirection * 0.1f;

			newPosition.y = transform.position.y; //dont move the y

			rbAI.velocity = new Vector2(newPosition.x, rbAI.velocity.y);
			animator.SetBool("Walking", true);

			//This enemy is different than the player, the original sprites face different ways.
			if (rbAI.velocity.x < 0)
			{
				if (renderer.flipX) //Character facing right after code
				{
					renderer.flipX = false;
				}
			}
			else
			{
				if (!renderer.flipX) //Character facing left after code
				{
					renderer.flipX = true;
				}
			}

			return false;
		}
	}
}
