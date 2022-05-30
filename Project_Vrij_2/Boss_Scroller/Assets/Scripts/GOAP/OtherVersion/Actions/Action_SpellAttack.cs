using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_SpellAttack : Action_Base
{
    List<System.Type> SupportedGoals = new List<System.Type>(new System.Type[] { typeof(Goal_SpellAttack) });

    Goal_SpellAttack spellAttackGoal;
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
        spellAttackGoal = (Goal_SpellAttack)LinkedGoal;

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

		spellAttackGoal.deathbringer.attackFatigueTimer.StartTimer(spellAttackGoal.deathbringer.RandomFatigueTime(3f + spellAttackGoal.deathbringer.attackFatigueTime, 4f + spellAttackGoal.deathbringer.attackFatigueTime));
		spellAttackGoal.deathbringer.walkFatigueTimer.StartTimer(spellAttackGoal.deathbringer.walkFatigueTime);
		isRunning = true;
		StartCoroutine(CastingSpell());
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

	private IEnumerator CastingSpell()
	{
		spellAttackGoal.deathbringer.rbAI.velocity = Vector2.zero;
		//spellAttackGoal.audioManager.PlayDeathBringerSpellAttackSound();
		spellAttackGoal.deathbringer.animator.Play("Cast");
		yield return new WaitForSeconds(1);
		int rand = Random.Range(3, 7);
		CastSpell(rand);
		yield return new WaitForSeconds(1);
	}

	private void CastSpell(int amountOfSpells)
	{
		float minX = -10;
		float maxX = 10;

		float minimumCastRange = 3f;

		bool canCastLeft = false;
		bool canCastRight = false;

		if (transform.position.x > (minX + minimumCastRange)) canCastLeft = true;
		if (transform.position.x < (maxX - minimumCastRange)) canCastRight = true;

		Vector3 castLocation = new Vector3(transform.position.x, transform.position.y, 0);

		List<float> xLocationsSpawnedSpells = new List<float>();

		for (int i = 0; i < amountOfSpells; i++)
		{
			if (canCastRight && canCastLeft)
			{
				int rand = Random.Range(0, 2);

				if (rand == 0) //We're casting to the left Side
				{
					float randomX = Random.Range((transform.position.x - minimumCastRange), (minX + minimumCastRange));
					castLocation.x = randomX;
				}
				else
				{
					float randomX = Random.Range((transform.position.x + minimumCastRange), (maxX - minimumCastRange));
					castLocation.x = randomX;
				}
			}
			else if (canCastLeft)
			{
				float randomX = Random.Range((transform.position.x - minimumCastRange), (minX + minimumCastRange));
				castLocation.x = randomX;
			}
			else
			{
				float randomX = Random.Range((transform.position.x + minimumCastRange), (maxX - minimumCastRange));
				castLocation.x = randomX;
			}

			bool canCastOnThisLocation = true;

			if (xLocationsSpawnedSpells.Count > 0)
			{
				for (int t = 0; t < xLocationsSpawnedSpells.Count; t++)
				{
					if (Mathf.Abs(xLocationsSpawnedSpells[t] - castLocation.x) < 1.5f)
					{
						canCastOnThisLocation = false;
						break;
					}
				}
			}

			if (canCastOnThisLocation)
			{
				int randObject = Random.Range(0, 2);

				xLocationsSpawnedSpells.Add(castLocation.x);

				if (randObject == 0)
				{
					castLocation.y = spellAttackGoal.spellBelow.transform.position.y;
					GameObject.Instantiate(spellAttackGoal.spellBelow, castLocation, spellAttackGoal.spellBelow.transform.rotation);
				}
				else
				{
					castLocation.y = spellAttackGoal.spellAbove.transform.position.y;
					GameObject.Instantiate(spellAttackGoal.spellAbove, castLocation, spellAttackGoal.spellAbove.transform.rotation);
				}
			}
			else
			{
				if (amountOfSpells < 50)
				{
					amountOfSpells++;
				}
			}
		}

		//StartCoroutine(spellAttackGoal.audioManager.PlayDeathBringerSpellSound(3.65f, 2f));

		isRunning = false;
	}

}
