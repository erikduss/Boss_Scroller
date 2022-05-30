using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Abigail : Enemy, IDamageable {

	// Use this for initialization
	void Start () {
		health = 300;
		speed = 30;
		strength = 15;
		regenRate = 3f;
		stamina = 500f;
		maxStamina = 500f;

		minDist = 10f;

		target = GameObject.FindGameObjectWithTag("Player");
	}

	public override void passiveRegen(){
		stamina += regenRate;
	}

	public override HashSet<KeyValuePair<string, object>> createGoalState(){
		HashSet<KeyValuePair<string, object>> goal = new HashSet<KeyValuePair<string, object>> ();
		goal.Add (new KeyValuePair<string, object> ("damagePlayer", true));
		goal.Add (new KeyValuePair<string, object> ("stayAlive", true));
		return goal;
	}

	void OnCollisionEnter2D(Collision2D collision){
		
	}

    public void TakeDamage(float damage)
    {
		Debug.Log("Taken "+ damage +" damage");
    }
}