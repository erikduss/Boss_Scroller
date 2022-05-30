using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerGOAP : MonoBehaviour, IDamageable
{
	public float maxHealth = 1000;
	public float currentHealth = 1000;

	public float strength = 15;
	public float currentStrength = 15;

	public float regenRate = 3f;
	public float stamina = 500f;
	public float maxStamina = 500f;

	public float minDist = 5f;

	public float speed = 1;

	public GameObject player;
	public Animator animator;
	public Rigidbody2D rbAI;
	public BoxCollider2D boxCollider;

	public Vector3 startPos;

	public SpriteRenderer renderer;

	[SerializeField] public GameObject spellBelowBig;
	[SerializeField] public GameObject healingStatue;

	public float currentAmountOfHealingStatues = 0;
	public float maxHealingStatues = 2;

	public float attackFatigueTime = 0.5f;
	public WaitTimer attackFatigueTimer = new WaitTimer();

	public float walkFatigueTime = 2f;
	public WaitTimer walkFatigueTimer = new WaitTimer();

	[SerializeField] public GameObject explosion_Indicator;

	[SerializeField] public BoxCollider2D meleeAttackTrigger;

	public bool combatEnabled = false;
	public Vector3 startPosition = new Vector3(4.27f, -1.8f, 0);

	public AudioManager audioManager;
	public GameManager gameManager;

	[SerializeField] public HealthBar healthBar;

	[SerializeField] public ParticleSystem healingFire;
	public List<GameObject> spawnedHealingStatues = new List<GameObject>();

	public bool isDead = false;

	void Start()
	{
		animator = GetComponent<Animator>();
		rbAI = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		renderer = GetComponent<SpriteRenderer>();

		//audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();
		//gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

		startPos = transform.position;

		player = GameObject.FindGameObjectWithTag("Player");

		//if (healthBar != null)
		//{
		//	healthBar.SetUpHealthBar(maxHealth);
		//}

		attackFatigueTimer.StartTimer(RandomFatigueTime(1.4f, 4.7f));
	}

	void Update()
	{
		//if (!combatEnabled || currentHealth <= 0 || !gameManager.playerIsAlive || isDead) return;

		//ChooseNewAttack();

		//if (IsAllowedToMove())
		//{
			
		//}
	}

	public IEnumerator GainHealing(float healingAmount)
	{
		yield return new WaitForSeconds(3f);
		healingFire.Play();
		audioManager.PlayHealingFireReceiveSound();

		if (currentHealth > 0 && currentHealth < maxHealth)
		{
			currentHealth += healingAmount;

			if (currentHealth > maxHealth)
			{
				currentHealth = maxHealth;
				healthBar.SetHealth(maxHealth);
			}
			else
			{
				healthBar.IncreaseHealth(healingAmount);
			}
		}
	}

	public void ResetToDefaults()
	{
		currentHealth = maxHealth;
		currentStrength = strength;
		boxCollider.enabled = true;
		healthBar.SetUpHealthBar(maxHealth);
		animator.Play("Idle");
		transform.position = startPos;
		combatEnabled = false;
		rbAI.velocity = Vector2.zero;
		renderer.flipX = false;
		renderer.enabled = true;
		isDead = false;
		currentAmountOfHealingStatues = 0;
		spawnedHealingStatues.Clear();
	}

	public IEnumerator ActivateBoss()
	{
		//renderer.enabled = false;
		//StartCoroutine(Teleport(startPosition));
		yield return new WaitForSeconds(3f);
		combatEnabled = true;
	}

	private void ChooseNewAttack()
	{
		//The enemy is fatigued and cannot execute another action
		if (!attackFatigueTimer.TimerFinished())
		{
			return;
		}

		animator.SetBool("Walking", false);

		//The player is out of attack range
		if (playerDistance() > (minDist * 1.5f))
		{
			float randNumber = ChooseRandomAttack();

			if (currentAmountOfHealingStatues < maxHealingStatues)
			{
				if (randNumber >= 70)
				{
					SpellAttack();
				}
				else if (randNumber < 70 && randNumber > 50)
				{
				}
				else if (randNumber < 50 && randNumber > 30)
				{
					HealingStatueAttack();
				}
				else
				{
					//prevent the enemy from attemting another attack for some time.
					attackFatigueTimer.StartTimer(2f);
				}
			}
			else
			{
				if (randNumber >= 65)
				{
					SpellAttack();
				}
				else if (randNumber < 65 && randNumber > 40)
				{
				}
				else
				{
					//prevent the enemy from attemting another attack for some time.
					attackFatigueTimer.StartTimer(2f);
				}
			}
		}
		else if (playerDistance() <= minDist)
		{

			if (currentAmountOfHealingStatues < maxHealingStatues)
			{
				//cant hit the player with melee attack anymore
				if (playerDistance() <= 2f)
				{
					float randNumber = ChooseRandomAttack();

					if (randNumber < 20)
					{
						ExplodeAttack();
					}
					else if (randNumber >= 20 && randNumber < 40)
					{
						//duration is 3.15 seconds.
						attackFatigueTimer.StartTimer(RandomFatigueTime(3.65f, 4.65f));
						walkFatigueTimer.StartTimer(walkFatigueTime);
						StartCoroutine(CastBigSpellBelow());
					}
					else if (randNumber >= 40 && randNumber < 60)
					{
					}
					else if (randNumber >= 60 && randNumber < 75)
					{
						SpellAttack();
					}
					else if (randNumber >= 75 && randNumber < 90)
					{
						HealingStatueAttack();
					}
					else
					{
						TeleportAttack();
					}
				}
				else
				{
					float randNumber = ChooseRandomAttack();

					if (randNumber < 10)
					{
						ExplodeAttack();
					}
					else if (randNumber >= 10 && randNumber < 45)
					{
						MeleeAttack();
					}
					else if (randNumber >= 45 && randNumber < 65)
					{
					}
					else if (randNumber >= 65 && randNumber < 80)
					{
						SpellAttack();
					}
					else if (randNumber >= 80 && randNumber < 92)
					{
						HealingStatueAttack();
					}
					else
					{
						TeleportAttack();
					}
				}
			}
			else
			{
				//cant hit the player with melee attack anymore
				if (playerDistance() <= 2f)
				{
					float randNumber = ChooseRandomAttack();

					if (randNumber < 20)
					{
						ExplodeAttack();
					}
					else if (randNumber >= 20 && randNumber < 40)
					{
						//duration is 3.15 seconds.
						attackFatigueTimer.StartTimer(RandomFatigueTime(3.65f, 4.65f));
						walkFatigueTimer.StartTimer(walkFatigueTime);
						StartCoroutine(CastBigSpellBelow());
					}
					else if (randNumber >= 40 && randNumber < 60)
					{
					}
					else if (randNumber >= 60 && randNumber < 80)
					{
						SpellAttack();
					}
					else
					{
						TeleportAttack();
					}
				}
				else
				{
					float randNumber = ChooseRandomAttack();

					if (randNumber < 10)
					{
						ExplodeAttack();
					}
					else if (randNumber >= 10 && randNumber < 50)
					{
						MeleeAttack();
					}
					else if (randNumber >= 50 && randNumber < 70)
					{
					}
					else if (randNumber >= 70 && randNumber < 90)
					{
						SpellAttack();
					}
					else
					{
						TeleportAttack();
					}
				}
			}
		}
	}

	//Duration: 8.75 seconds
	private void ExplodeAttack()
	{
		attackFatigueTimer.StartTimer(RandomFatigueTime(8.75f + attackFatigueTime, 9.75f + attackFatigueTime));
		walkFatigueTimer.StartTimer(walkFatigueTime * 2.5f);

		StartCoroutine(Explode());
	}

	//Duration: 2 seconds -> new value 3
	private void SpellAttack()
	{
		
	}

	//Duration: 2 seconds -> new value 3
	private void HealingStatueAttack()
	{
		attackFatigueTimer.StartTimer(RandomFatigueTime(3f + attackFatigueTime, 4f + attackFatigueTime));
		walkFatigueTimer.StartTimer(walkFatigueTime);
		StartCoroutine(SummoningingHealingStatue());
	}

	//Duration: 3 seconds
	private void TeleportAttack()
	{
		attackFatigueTimer.StartTimer(RandomFatigueTime(3f + attackFatigueTime, 4f + attackFatigueTime));
		walkFatigueTimer.StartTimer(walkFatigueTime);

		//StartCoroutine(Teleport(PickRandomTeleportLocation()));
	}

	//Duration: 3.05 seconds -> new length = 2.35 (0.7 faster, 0.2 in animation and 0.5 in end recovery)
	private void MeleeAttack()
	{
		attackFatigueTimer.StartTimer(RandomFatigueTime(2.35f + attackFatigueTime, 4.05f + attackFatigueTime));
		walkFatigueTimer.StartTimer(walkFatigueTime);
		//StartCoroutine(Melee());
	}

	

	private bool IsAllowedToMove()
	{
		return walkFatigueTimer.TimerFinished();
	}

	private float ChooseRandomAttack()
	{
		float rand = Random.Range(0, 100);

		return rand;
	}

	public float RandomFatigueTime(float min, float max)
	{
		return Random.Range(min, max);
	}

	

	private void SummonBigSpell()
	{
		//Vector3 castLocation = new Vector3(transform.position.x, transform.position.y, 0);

		//castLocation.y = spellBelow.transform.position.y;
		//GameObject.Instantiate(spellBelowBig, castLocation, spellBelow.transform.rotation);

		//StartCoroutine(audioManager.PlayDeathBringerSpellSound(3.15f, 1.5f));
	}

	private void SummonHealingStatue()
	{
		float minX = -10;
		float maxX = 10;

		float minimumCastRange = 3f;

		bool canCastLeft = false;
		bool canCastRight = false;

		if (transform.position.x > (minX + minimumCastRange)) canCastLeft = true;
		if (transform.position.x < (maxX - minimumCastRange)) canCastRight = true;

		Vector3 castLocation = new Vector3(transform.position.x, transform.position.y, 0);

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

		castLocation.y = -4.1f;
		GameObject statueToSpawn = healingStatue;
		statueToSpawn.name = "Healing Statue";
		spawnedHealingStatues.Add(statueToSpawn);
		GameObject.Instantiate(statueToSpawn, castLocation, statueToSpawn.transform.rotation);
		currentAmountOfHealingStatues++;
	}

	private Vector3 PickRandomTeleportLocation()
	{
		float minX = -10;
		float maxX = 10;

		float minimumTeleportLength = 3f;

		bool canTeleportLeft = false;
		bool canTeleportRight = false;

		if (transform.position.x > (minX + minimumTeleportLength)) canTeleportLeft = true;
		if (transform.position.x < (maxX - minimumTeleportLength)) canTeleportRight = true;

		Vector3 teleportLocation = new Vector3(transform.position.x, transform.position.y, 0);

		if (canTeleportRight && canTeleportLeft)
		{
			int rand = Random.Range(0, 2);

			if (rand == 0) //We're teleporting to the left Side
			{
				float randomX = Random.Range((transform.position.x - minimumTeleportLength), (minX + minimumTeleportLength));
				teleportLocation.x = randomX;
			}
			else
			{
				float randomX = Random.Range((transform.position.x + minimumTeleportLength), (maxX - minimumTeleportLength));
				teleportLocation.x = randomX;
			}
		}
		else if (canTeleportLeft)
		{
			float randomX = Random.Range((transform.position.x - minimumTeleportLength), (minX + minimumTeleportLength));
			teleportLocation.x = randomX;
		}
		else
		{
			float randomX = Random.Range((transform.position.x + minimumTeleportLength), (maxX - minimumTeleportLength));
			teleportLocation.x = randomX;
		}

		return teleportLocation;
	}

	public void passiveRegen()
	{
		stamina += regenRate;
	}

	public void TakeDamage(float damage)
	{
		if (currentHealth >= 0)
		{
			healthBar.DecreaseHealth(damage);
			currentHealth -= damage;

			if (currentHealth > 0)
			{
				audioManager.PlayEnemyImpact();
			}
			else
			{
				isDead = true;
				boxCollider.enabled = false;
				StopAllCoroutines();
				audioManager.PlayDeathBringerDeathSound();
				animator.Play("Death");
				StartCoroutine(deathBringerDied());
			}
		}
		else
		{
			if (!isDead)
			{
				isDead = true;
				boxCollider.enabled = false;
				StopAllCoroutines();
				audioManager.PlayDeathBringerDeathSound();
				animator.Play("Death");
				StartCoroutine(deathBringerDied());
			}
		}
	}

	private IEnumerator deathBringerDied()
	{
		yield return new WaitForSeconds(1);
		renderer.enabled = false;
		StartCoroutine(gameManager.DeathBringerDefeated());
		yield return new WaitForSeconds(2);
	}

	private float playerDistance()
	{
		var vec = transform.position - player.transform.position;
		vec.y = 0;
		float dist = vec.magnitude;

		return dist;
	}

	private IEnumerator CastBigSpellBelow()
	{
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerSpellAttackSound();
		animator.Play("Close-Range-Spell");
		yield return new WaitForSeconds(1.75f);
		SummonBigSpell();
		yield return new WaitForSeconds(1);
	}

	private IEnumerator SummoningingHealingStatue()
	{
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerSpellAttackSound();
		animator.Play("summonHealingStatue");
		yield return new WaitForSeconds(1.2f);
		SummonHealingStatue();
		yield return new WaitForSeconds(1);
	}

	

	private IEnumerator Explode()
	{
		rbAI.velocity = Vector2.zero;

		animator.Play("AOE-Explode");
		audioManager.PlayDeathBringerExplotionChannelSound();

		currentStrength = strength;

		float elapsedTime = 0f;

		Vector3 explosionSize = explosion_Indicator.transform.localScale;
		explosion_Indicator.SetActive(true);

		while (elapsedTime < 0.75f)
		{
			elapsedTime += Time.deltaTime;
			explosion_Indicator.transform.localScale = new Vector3(Mathf.Lerp(0, 1.5f, elapsedTime / 0.75f), Mathf.Lerp(0, 1.5f, elapsedTime / 0.75f), 1);
			yield return null;
		}

		elapsedTime = 0;

		while (elapsedTime < 0.75f)
		{
			elapsedTime += Time.deltaTime;
			explosion_Indicator.transform.localScale = new Vector3(Mathf.Lerp(1.5f, 0, elapsedTime / 0.75f), Mathf.Lerp(1.5f, 0, elapsedTime / 0.75f), 1);
			yield return null;
		}

		elapsedTime = 0;
		audioManager.PlayDeathBringerExplotionChannelSound();

		currentStrength = strength * 2;

		while (elapsedTime < 0.75f)
		{
			elapsedTime += Time.deltaTime;
			explosion_Indicator.transform.localScale = new Vector3(Mathf.Lerp(0, 2.5f, elapsedTime / 0.75f), Mathf.Lerp(0, 2.5f, elapsedTime / 0.75f), 1);
			yield return null;
		}

		elapsedTime = 0;

		while (elapsedTime < 0.75f)
		{
			elapsedTime += Time.deltaTime;
			explosion_Indicator.transform.localScale = new Vector3(Mathf.Lerp(2.5f, 0, elapsedTime / 0.75f), Mathf.Lerp(2.5f, 0, elapsedTime / 0.75f), 1);
			yield return null;
		}

		elapsedTime = 0;

		explosion_Indicator.GetComponent<BoxCollider2D>().enabled = true;

		currentStrength = strength * 3;

		audioManager.PlayDeathBringerExplotionSound();
		animator.Play("AOE-Explode-End");

		while (elapsedTime < 0.75f)
		{
			elapsedTime += Time.deltaTime;
			explosion_Indicator.transform.localScale = new Vector3(Mathf.Lerp(0, 5f, elapsedTime / 0.75f), Mathf.Lerp(0, 5f, elapsedTime / 0.75f), 1);
			yield return null;
		}

		elapsedTime = 0;
		float shrinkTime = 0.35f; //was normally 0.75f

		while (elapsedTime < shrinkTime)
		{
			elapsedTime += Time.deltaTime;
			Color colorToLerpTo = Color.red;
			colorToLerpTo.a = 0;
			explosion_Indicator.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.red, colorToLerpTo, elapsedTime / shrinkTime);
			renderer.color = Color.Lerp(Color.red, colorToLerpTo, elapsedTime / shrinkTime);
			yield return null;
		}

		renderer.enabled = false;
		boxCollider.enabled = false;
		explosion_Indicator.GetComponent<BoxCollider2D>().enabled = false;

		yield return new WaitForSeconds(1);
		//reset everything to normal
		explosion_Indicator.SetActive(false);
		explosion_Indicator.GetComponent<SpriteRenderer>().color = Color.red;

		Vector3 locationToAppearAt = transform.position;

		if (ChooseRandomAttack() > 65f)
		{
			locationToAppearAt = PickRandomTeleportLocation();
		}

		yield return new WaitForSeconds(0.5f);
		transform.position = locationToAppearAt;
		yield return new WaitForSeconds(1);
		renderer.enabled = true;
		animator.Play("Teleport End");
		yield return new WaitForSeconds(0.5f);
		boxCollider.enabled = true;
		yield return new WaitForSeconds(0.5f);
	}

	private void OnTriggerStay2D(Collider2D triggerCollision)
	{
		if (triggerCollision.gameObject.tag == "Player")
		{
			triggerCollision.gameObject.GetComponent<IDamageable>().TakeDamage(currentStrength);
		}
	}
}
