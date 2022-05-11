using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerGOAP : Enemy, IDamageable
{
	// Use this for initialization
	void Start()
	{
		health = 300;
		speed = 1;
		strength = 15;
		regenRate = 3f;
		stamina = 500f;
		maxStamina = 500f;

		minDist = 1.5f;

		setSpeed(speed);

		player = GameObject.FindGameObjectWithTag("Player");
	}

	public override void passiveRegen()
	{
		stamina += regenRate;
	}

	public override HashSet<KeyValuePair<string, object>> createGoalState()
	{
		HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>>();
		goal.Add(new KeyValuePair<string, object>("damagePlayer", true));
		goal.Add(new KeyValuePair<string, object>("stayAlive", true));
		return goal;
	}

	public void TakeDamage(float damage)
	{
		Debug.Log("Taken " + damage + " damage");
	}

	void OnCollisionEnter2D(Collision2D collision)
	{

	}
}
