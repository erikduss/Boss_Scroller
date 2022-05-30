using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Enemy : MonoBehaviour, IGOAP {

	public Animator animator;
	public Rigidbody2D rbAI;
	public BoxCollider2D boxCollider;
	public GameObject target;

	public SpriteRenderer renderer;

	[HideInInspector] public int health;
	[HideInInspector] public int strength;
	[HideInInspector] public int speed;
	[HideInInspector] public float stamina;
	[HideInInspector] public float regenRate;
	protected float minDist = 5f;
	protected bool loop = false;
	protected float maxStamina;

	// Use this for initialization
	void Start () 
	{
		target = GameObject.FindGameObjectWithTag("Player");
	}
	
	// Update is called once per frame
	public virtual void Update () 
	{
		if (stamina <= maxStamina) {
			Invoke ("passiveRegen", 1.0f);
		} else {
			stamina = maxStamina;
		}
	}

	public abstract void passiveRegen();

	public HashSet<KeyValuePair<string, object>> getWorldState(){
		HashSet<KeyValuePair<string, object>> worldData = new HashSet<KeyValuePair<string, object>> ();
		worldData.Add (new KeyValuePair<string, object> ("damagePlayer", false)); //to-do: change player's state for world data here
		worldData.Add (new KeyValuePair<string, object> ("evadePlayer", false));
		return worldData;
	}

	public abstract HashSet<KeyValuePair<string, object>> createGoalState ();

	public void planFailed (HashSet<KeyValuePair<string, object>> failedGoal)
	{
		Debug.Log("Plan failed");
	}

	public void planFound(HashSet<KeyValuePair<string, object>> goal, Queue<GOAPAction> action)
	{
		Debug.Log("Plan found");
		
	}

	public void actionsFinished()
	{
		Debug.Log("Action completed");
	}

	public void planAborted(GOAPAction aborter)
	{
		Debug.Log("Plan aborted");
	}

	public virtual bool moveAgent(GOAPAction nextAction)
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
			Vector3 moveDirection = target.transform.position - transform.position;

			Vector3 newPosition = moveDirection;

			float fixedSpeed;

			if(moveDirection.x > 0)
            {
				fixedSpeed = speed;
			}
            else
            {
				fixedSpeed = -speed;
            }

			newPosition.y = transform.position.y; //dont move the y

			rbAI.velocity = new Vector2(fixedSpeed, rbAI.velocity.y);
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
