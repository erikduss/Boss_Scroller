using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringer : MonoBehaviour, IDamageable
{
	[HideInInspector] public float maxHealth = 1000; //default 1000
	[HideInInspector] public float currentHealth = 1000;
	
	private float strength = 15;
	private float currentStrength = 15;

	private float regenRate = 3f;
	private float stamina = 500f;
	private float maxStamina = 500f;

	private float minDist = 5f;

	private float speed = 1;
	private float terminalSpeed;
	private float initialSpeed;
	private float acceleration;

	private GameObject player;
	private Animator animator;
	private Rigidbody2D rbAI;
	private BoxCollider2D boxCollider;

	private Vector3 startPos;

	private SpriteRenderer renderer;

	[SerializeField] private GameObject spellBelow;
	[SerializeField] private GameObject spellAbove;
	[SerializeField] private GameObject spellBelowBig;
	[SerializeField] private GameObject healingStatue;

	public float currentAmountOfHealingStatues = 0;
	private float maxHealingStatues = 2;

	private float attackFatigueTime = 1f;
	private WaitTimer attackFatigueTimer = new WaitTimer();

	private float walkFatigueTime = 2f;
	private WaitTimer walkFatigueTimer = new WaitTimer();

	private WaitTimer selfCorrectionTimer = new WaitTimer();

	[SerializeField] private GameObject explosion_Indicator;

	[SerializeField] private BoxCollider2D meleeAttackTrigger;

	private bool combatEnabled = false;
	private Vector3 startPosition = new Vector3(4.27f,-1.8f,0);

	private AudioManager audioManager;
	private GameManager gameManager;

	private HealthBar healthBar;

	[SerializeField] private ParticleSystem healingFire;
	private List<GameObject> spawnedHealingStatues = new List<GameObject>();

	public bool isDead = false;

	private bool attacking = false;

	public bool isSummoned = false;
	public Vector3 summonLocation;

	private float minAreaX = -10f;
	private float maxAreaX = 10f;

	// Use this for initialization
	void Start()
	{
		animator = GetComponent<Animator>();
		rbAI = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		renderer = GetComponent<SpriteRenderer>();

		audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();
		gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

		healthBar = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();

		setSpeed(speed);

		startPos = transform.position;

		player = GameObject.FindGameObjectWithTag("Player");

		if(PlayerPrefs.GetInt("ExperimentalEnabled") == 1)
        {
			currentHealth = 100;
			maxHealth = 100;
		}
        else
        {
			maxHealth = 1000;
			currentHealth = maxHealth;
		}
		
		if (healthBar != null)
		{
			healthBar.SetUpHealthBar(maxHealth);
		}

		attackFatigueTimer.StartTimer(RandomFatigueTime(1.4f, 2.7f));
	}

	void Update()
    {
		if (!combatEnabled || currentHealth <= 0 || !gameManager.playerIsAlive || isDead || attacking)
		{
            if (isDead)
            {
                //check if something went wrong
                if (selfCorrectionTimer.TimerFinished())
                {
					if (!gameManager.activatedNecromancer)
					{
						audioManager.spellsAudioSource.Stop();
						StartCoroutine(gameManager.DeathBringerDefeated());
						selfCorrectionTimer.StartTimer(8.5f);
					}
				}
            }

			rbAI.velocity = Vector2.zero;
			return;
		}

        ChooseNewAttack();

        if (IsAllowedToMove())
        {
            MoveAgent();
        }
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

	public void SetHealthCorrectly()
    {
		if (PlayerPrefs.GetInt("ExperimentalEnabled") == 1)
		{
			currentHealth = 100;
			maxHealth = 100;
		}
		else
		{
			maxHealth = 1000;
			currentHealth = maxHealth;
		}

		if (healthBar == null)
		{
			healthBar = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();
		}
		healthBar.SetUpHealthBar(maxHealth);
	}

	public IEnumerator ActivateBoss()
    {
		//renderer.enabled = false;
		SetHealthCorrectly();
		StartCoroutine(Teleport(startPosition));
		yield return new WaitForSeconds(1f);
		if (healthBar == null)
        {
			healthBar = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();
			healthBar.SetUpHealthBar(maxHealth);
		}
		yield return new WaitForSeconds(2f);
		combatEnabled = true;
	}

	public IEnumerator ActivateSummonedBoss()
	{
		//renderer.enabled = false;
		StartCoroutine(Teleport(summonLocation));
		yield return new WaitForSeconds(1f);
		if (healthBar == null)
		{
			healthBar = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();
			healthBar.SetUpHealthBarWithDecreasedHealth(maxHealth, currentHealth);
		}
		yield return new WaitForSeconds(1f);
		combatEnabled = true;
	}

	public void SetSummonedData(Vector3 summonLoc, float hlt)
    {
		isSummoned = true;
		summonLocation = summonLoc;
		summonLocation.y = -1.8f;
		currentHealth = hlt;

		maxHealth = 1000;
		currentStrength = strength;
		boxCollider.enabled = true;
		animator.Play("Idle");
		transform.position = summonLocation;
		combatEnabled = false;
		rbAI.velocity = Vector2.zero;
		renderer.flipX = false;
		renderer.enabled = true;
		isDead = false;
		currentAmountOfHealingStatues = 0;
		spawnedHealingStatues.Clear();

		minAreaX = -35f;
		maxAreaX = 95f;

		if (healthBar == null)
		{
			healthBar = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();
			healthBar.SetUpHealthBarWithDecreasedHealth(maxHealth, currentHealth);
		}
        else
        {
			healthBar.SetUpHealthBarWithDecreasedHealth(maxHealth, currentHealth);
		}

		if (currentHealth > 0)
		{
			if (currentHealth >= maxHealth)
			{
				currentHealth = maxHealth;
				healthBar.SetHealth(maxHealth);
			}
			else
			{
				healthBar.SetHealth(currentHealth);
			}
		}

		StartCoroutine(ActivateSummonedBoss());
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

			if(currentAmountOfHealingStatues < maxHealingStatues)
            {
				if (randNumber >= 70)
				{
					SpellAttack();
				}
				else if (randNumber < 70 && randNumber > 50)
				{
					StartCoroutine(AmbushAttack());
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
					StartCoroutine(AmbushAttack());
				}
				else
				{
					//prevent the enemy from attemting another attack for some time.
					attackFatigueTimer.StartTimer(2f);
				}
			}
		}
        else if(playerDistance() <= minDist)
        {
			FaceThePlayer();

			if(currentAmountOfHealingStatues < maxHealingStatues)
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
						//attackFatigueTimer.StartTimer(RandomFatigueTime(3.65f, 4.65f));
						//walkFatigueTimer.StartTimer(walkFatigueTime);
						attacking = true;
						StartCoroutine(CastBigSpellBelow());
					}
					else if (randNumber >= 40 && randNumber < 60)
					{
						StartCoroutine(AmbushAttack());
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
						StartCoroutine(AmbushAttack());
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
						//attackFatigueTimer.StartTimer(RandomFatigueTime(3.65f, 4.65f));
						//walkFatigueTimer.StartTimer(walkFatigueTime);

						attacking = true;

						StartCoroutine(CastBigSpellBelow());
					}
					else if (randNumber >= 40 && randNumber < 60)
					{
						StartCoroutine(AmbushAttack());
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
						StartCoroutine(AmbushAttack());
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
		attacking = true;

		//attackFatigueTimer.StartTimer(RandomFatigueTime(8.75f + attackFatigueTime, 9.75f + attackFatigueTime));
		//walkFatigueTimer.StartTimer(walkFatigueTime * 2.5f);

		StartCoroutine(Explode());
	}

	//Duration: 2 seconds -> new value 3
	private void SpellAttack()
    {
		attacking = true;

		//attackFatigueTimer.StartTimer(RandomFatigueTime(3f + attackFatigueTime, 4f + attackFatigueTime));
		//walkFatigueTimer.StartTimer(walkFatigueTime);
		StartCoroutine(CastingSpell());
	}

	//Duration: 2 seconds -> new value 3
	private void HealingStatueAttack()
	{
		attacking = true;

		//attackFatigueTimer.StartTimer(RandomFatigueTime(2f + attackFatigueTime, 3f + attackFatigueTime));
		//walkFatigueTimer.StartTimer(walkFatigueTime);
		StartCoroutine(SummoningingHealingStatue());
	}

	//Duration: 3 seconds
	private void TeleportAttack()
    {
		attacking = true;

		//attackFatigueTimer.StartTimer(RandomFatigueTime(3f + attackFatigueTime, 4f + attackFatigueTime));
		//walkFatigueTimer.StartTimer(walkFatigueTime);

		StartCoroutine(Teleport(PickRandomTeleportLocation()));
	}

	//Duration: 3.05 seconds -> new length = 2.35 (0.7 faster, 0.2 in animation and 0.5 in end recovery)
	private void MeleeAttack()
    {
		attacking = true;

		//attackFatigueTimer.StartTimer(RandomFatigueTime(1.35f + attackFatigueTime, 3.05f + attackFatigueTime));
		//walkFatigueTimer.StartTimer(walkFatigueTime);
		StartCoroutine(Melee());
	}

	//Duration: 6.05 seconds
	private IEnumerator AmbushAttack()
	{
		attacking = true;

		//attackFatigueTimer.StartTimer(RandomFatigueTime(5.05f + attackFatigueTime, 6.05f + attackFatigueTime));
		//walkFatigueTimer.StartTimer(walkFatigueTime);

		//teleport location
		Vector3 teleportLocation = new Vector3(transform.position.x, transform.position.y, 0);

		float minX = -10;
		float maxX = 10;

		float minimumTeleportLength = 4f;

		bool canTeleportLeft = false;
		bool canTeleportRight = false;

		if (player.transform.position.x > (minX + minimumTeleportLength)) canTeleportLeft = true;
		if (player.transform.position.x < (maxX - minimumTeleportLength)) canTeleportRight = true;

		if (canTeleportRight && canTeleportLeft)
		{
			int rand = Random.Range(0, 2);

			if (rand == 0) //We're teleporting to the left Side
			{
				float randomX = Random.Range((player.transform.position.x - minimumTeleportLength), (player.transform.position.x - (minimumTeleportLength + 1)));
				teleportLocation.x = randomX;
			}
			else
			{
				float randomX = Random.Range((player.transform.position.x + minimumTeleportLength), (player.transform.position.x + (minimumTeleportLength + 1)));
				teleportLocation.x = randomX;
			}
		}
		else if (canTeleportLeft)
		{
			float randomX = Random.Range((player.transform.position.x - minimumTeleportLength), (player.transform.position.x - (minimumTeleportLength + 1)));
			teleportLocation.x = randomX;
		}
		else
		{
			float randomX = Random.Range((player.transform.position.x + minimumTeleportLength), (player.transform.position.x + (minimumTeleportLength + 1)));
			teleportLocation.x = randomX;
		}

		yield return StartCoroutine(Teleport(teleportLocation));

		FaceThePlayer();
		attacking = true;

		yield return StartCoroutine(Melee());
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

	private float RandomFatigueTime(float min, float max)
    {
		return Random.Range(min,max);
    }

	private void FaceThePlayer()
    {
		Vector3 direction = player.transform.position - transform.position;

		float dir = direction.x;

		//This enemy is different than the player, the original sprites face different ways.
		if (dir < 0)
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
	}

	private void SummonBigSpell()
    {
		Vector3 castLocation = new Vector3(transform.position.x, transform.position.y, 0);

		castLocation.y = spellBelow.transform.position.y;
		GameObject.Instantiate(spellBelowBig, castLocation, spellBelow.transform.rotation);

		StartCoroutine(audioManager.PlayDeathBringerSpellSound(3.15f, 1.5f));
	}

	private void CastSpell(int amountOfSpells)
    {
		float minX = minAreaX;
		float maxX = maxAreaX;

		float minimumCastRange = 3f;

		bool canCastLeft = false;
		bool canCastRight = false;

		if (transform.position.x > (minX + minimumCastRange)) canCastLeft = true;
		if (transform.position.x < (maxX - minimumCastRange)) canCastRight = true;

		Vector3 castLocation = new Vector3(transform.position.x, transform.position.y, 0);

		List<float> xLocationsSpawnedSpells = new List<float>();

		for (int i = 0; i< amountOfSpells; i++)
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
					castLocation.y = spellBelow.transform.position.y;
					GameObject.Instantiate(spellBelow, castLocation, spellBelow.transform.rotation);
				}
				else
				{
					castLocation.y = spellAbove.transform.position.y;
					GameObject.Instantiate(spellAbove, castLocation, spellAbove.transform.rotation);
				}
			}
            else
            {
				if(amountOfSpells < 50)
                {
					amountOfSpells++;
				}
            }
		}

		StartCoroutine(audioManager.PlayDeathBringerSpellSound(3.65f, 2f));
	}


	private void SummonHealingStatue()
    {
		float minX = minAreaX;
		float maxX = maxAreaX;

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
		float minX = minAreaX;
		float maxX = maxAreaX;

		float minimumTeleportLength = 3f;

		bool canTeleportLeft = false;
		bool canTeleportRight = false;

		if (transform.position.x > (minX + minimumTeleportLength)) canTeleportLeft = true;
		if (transform.position.x < (maxX - minimumTeleportLength)) canTeleportRight = true;

		Vector3 teleportLocation = new Vector3(transform.position.x, transform.position.y, 0);

		if(canTeleportRight && canTeleportLeft)
        {
			int rand = Random.Range(0, 2);

			if(rand == 0) //We're teleporting to the left Side
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

			if(currentHealth > 0)
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
				selfCorrectionTimer.StartTimer(8.5f);
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
				selfCorrectionTimer.StartTimer(8.5f);
				StartCoroutine(deathBringerDied());
			}
		}
	}

	private IEnumerator deathBringerDied()
    {
		yield return new WaitForSeconds(1);
		renderer.enabled = false;
		explosion_Indicator.SetActive(false);
		if (!isSummoned)
        {
			StartCoroutine(gameManager.DeathBringerDefeated());
		}
		yield return new WaitForSeconds(2);
	}

	public void setSpeed(float val)
	{
		terminalSpeed = val / 10;
		initialSpeed = (val / 10) / 2;
		acceleration = (val / 10) / 4;
		return;
	}

	private float playerDistance()
    {
		var vec = transform.position - player.transform.position;
		vec.y = 0;
		float dist = vec.magnitude;

		return dist;
	}

	private void MoveAgent()
	{
		if (playerDistance() <= minDist)
		{
			rbAI.velocity = Vector2.zero;
			animator.SetBool("Walking", false);
		}
		else
		{
			setSpeed(speed);

			if (initialSpeed < terminalSpeed)
			{
				initialSpeed += acceleration;
			}

			Vector3 moveDirection = player.transform.position - transform.position;

			Vector3 newPosition = moveDirection * initialSpeed;

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
		}
	}

	private IEnumerator CastBigSpellBelow()
    {
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerSpellAttackSound();
		animator.Play("Close-Range-Spell");
		yield return new WaitForSeconds(1.75f);
		SummonBigSpell();
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(1);
	}

	private IEnumerator CastingSpell()
    {
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerSpellAttackSound();
		animator.Play("Cast");
		yield return new WaitForSeconds(1);
		int rand = Random.Range(3,7);
		CastSpell(rand);
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(1);
	}

	private IEnumerator SummoningingHealingStatue()
	{
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerSpellAttackSound();
		animator.Play("summonHealingStatue");
		yield return new WaitForSeconds(1.2f);
		SummonHealingStatue();
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(1);
	}

	private IEnumerator Melee()
    {
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerAttackSound();
		animator.Play("Attack");
		currentStrength = strength;

		yield return new WaitForSeconds(0.6f);
		audioManager.PlayDeathBringerSwingSound();
		meleeAttackTrigger.transform.gameObject.SetActive(true);
        if (!renderer.flipX)
        {
			meleeAttackTrigger.offset = new Vector2(-0.54f, -0.078f);
		}
        else
        {
			meleeAttackTrigger.offset = new Vector2(0.54f, -0.078f);
		}
		yield return new WaitForSeconds(0.1f);
		if (!renderer.flipX)
		{
			meleeAttackTrigger.offset = new Vector2(-0.34f, -0.078f);
		}
		else
		{
			meleeAttackTrigger.offset = new Vector2(0.34f, -0.078f);
		}
		yield return new WaitForSeconds(0.1f);
		if (!renderer.flipX)
		{
			meleeAttackTrigger.offset = new Vector2(-0.24f, -0.01f);
		}
		else
		{
			meleeAttackTrigger.offset = new Vector2(0.24f, -0.01f);
		}
		yield return new WaitForSeconds(0.1f);
		meleeAttackTrigger.transform.gameObject.SetActive(false);
		yield return new WaitForSeconds(.45f);
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		//animator.Play("Idle"); not needed?
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator Teleport(Vector3 teleportLocation)
    {
		rbAI.velocity = Vector2.zero;
		audioManager.PlayDeathBringerTeleportSound();
		animator.Play("Teleport Start");
		yield return new WaitForSeconds(0.5f);
		boxCollider.enabled = false;
		yield return new WaitForSeconds(0.5f);
		transform.position = teleportLocation;
		renderer.enabled = false;
		yield return new WaitForSeconds(1);
		renderer.enabled = true;
		animator.Play("Teleport End");
		audioManager.PlayDeathBringerTeleportSound();
		yield return new WaitForSeconds(0.5f);
		boxCollider.enabled = true;
        attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(0.5f);
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

		if(ChooseRandomAttack() > 65f)
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
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
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
