using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necromancer : MonoBehaviour, IDamageable
{
	private float maxHealth = 750;
	private float currentHealth = 750;

	private float strength = 10;
	private float currentStrength = 10;

	private float regenRate = 3f;
	private float stamina = 500f;
	private float maxStamina = 500f;

	private float minDist = 2.5f;

	private float speed = 2.5f;

	private GameObject player;
	public Animator animator;
	private Rigidbody2D rbAI;
	private BoxCollider2D boxCollider;

	private Vector3 startPos;

	private SpriteRenderer renderer;

	private float attackFatigueTime = 1f;
	private WaitTimer attackFatigueTimer = new WaitTimer();

	private float walkFatigueTime = 2f;
	private WaitTimer walkFatigueTimer = new WaitTimer();

	private float consumeDelayTime = 10f;
	private WaitTimer consumeDelayTimer = new WaitTimer();

	[SerializeField] private BoxCollider2D meleeAttackTrigger;

	private bool combatEnabled = false;
	private Vector3 startPosition = new Vector3(4.27f, -1.8f, 0);

	private AudioManager audioManager;
	private GameManager gameManager;

	[SerializeField] private HealthBar healthBar;
	[SerializeField] private ParticleSystem groundFire;
	[SerializeField] private GameObject summonSkeleton;
	[SerializeField] private GameObject summonDeathBringer;
	[SerializeField] private GameObject darkProjectile;

	[SerializeField] private DeathBringer deathBringer;
	[SerializeField] private Animator lifeLeachAnim;

	public List<SummonedSkeleton> summonedSkeletons = new List<SummonedSkeleton>();
	public int amountOfSkeletonsAlive = 0;

	public bool isDead = false;

	private bool attacking = false;

	private float nextDeathbringerSummonHealth = 100;
	private bool channelingEnrageSummon = false;
	private bool spawnedDeathBringer = false;
	private UIManager uiManager;

	private float enrage = 0;
	private float enrageSpeedBuffMultiplier = 1;

	private float fightDurationMultiplier = 1;
	private WaitTimer enrageIncreaseTimer = new WaitTimer();

	[SerializeField] private ParticleSystem enrageFire;

	// Use this for initialization
	void Start()
	{
		animator = GetComponent<Animator>();
		rbAI = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		renderer = GetComponent<SpriteRenderer>();

		audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();
		gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
		uiManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<UIManager>();

		startPos = transform.position;

		player = GameObject.FindGameObjectWithTag("Player");

		if (healthBar != null)
		{
			healthBar.SetUpHealthBar(maxHealth);
		}

		attackFatigueTimer.StartTimer(RandomFatigueTime(1.4f, 2.7f));
	}

	void Update()
	{
        if (combatEnabled && enrageIncreaseTimer.TimerFinished())
        {
			enrage += 0.1f;
			fightDurationMultiplier += 0.1f;
			EnrageUpdate();
			enrageIncreaseTimer.StartTimer(15f);
        }

        if (!combatEnabled || currentHealth <= 0 || !gameManager.playerIsAlive || isDead || attacking)
        {
            rbAI.velocity = Vector2.zero;
            return;
        }

        ChooseNewAttack();

        if (IsAllowedToMove())
        {
            MoveAgent();
        }
    }

	private void EnrageUpdate()
    {
		ParticleSystem.EmissionModule em = enrageFire.emission;
		em.rateOverTime = (enrage * 1000); //1000 is the max for the particles. Enrage scales between 0 and 1
	}

	public void ResetToDefaults()
	{
		currentHealth = maxHealth;
		currentStrength = strength;
		boxCollider.enabled = true;
		healthBar.SetUpHealthBar(maxHealth);
		animator.Play("Necromancer_Idle");
		transform.position = startPos;
		combatEnabled = false;
		rbAI.velocity = Vector2.zero;
		renderer.flipX = true;
		renderer.enabled = true;
		isDead = false;
	}

	public IEnumerator ActivateBoss()
	{
		//renderer.enabled = false;
		audioManager.PlayNecromancerIntroSound();
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

		//Attacks to add:
		//- Dash attack (run some distance (faster) and attack right after)
		//- Spellcast attack (summon? buffs for the cost of debuff? buff for health?)
		//- Enraged attacks (Faster dash attacks + jump attacks and chained together.)
		if (deathBringer.currentHealth <= 0 && spawnedDeathBringer)
		{
			spawnedDeathBringer = false;
			enrage -= 25f;
			EnrageUpdate();
		}
		animator.SetBool("Walking", false);

		if(enrage >= 1 && !spawnedDeathBringer)
        {
			//Summoning deathbringer costs 1 enrage (full 100%)
			//Enraged attacks cost 0.25 enrage (25%)
			//Buffs cost 0.10 enrage (10%)

			attacking = true;
			enrage -= 0.5f;
			EnrageUpdate();
			StartCoroutine(EnragedSummon());
		}
		else if(playerDistance() <= minDist)
        {
			float rand = ChooseRandomAttack();

			if (rand >= 0 && rand < 30)
			{
				attacking = true;
				StartCoroutine(JumpAttack());
			}
			else if(rand >= 30 && rand < 50 && enrage > 0.25f)
            {
				attacking = true;
				StartCoroutine(projectileBarrage());
				enrage -= 0.10f;
				EnrageUpdate();
			}
			else if (rand >= 50 && rand < 75)
            {
				attacking = true;
				StartCoroutine(dodgeBack());
            }
            else
            {
				attacking = true;
				StartCoroutine(Melee(strength, enrageSpeedBuffMultiplier));
			}
        }
        else
        {
			float rand = ChooseRandomAttack();

			if(rand >= 0 && rand < 40)
            {
				attacking = true;
				StartCoroutine(JumpAttack());
			}
			else if (rand >= 40 && rand < 60)
            {
				if(amountOfSkeletonsAlive > 0 && consumeDelayTimer.TimerFinished())
                {
					attacking = true;
					StartCoroutine(SiphonAttack());
                }
                else
                {
					attacking = true;
					StartCoroutine(summonSkeletonsAttack());
				}
            }
            else
            {
				float r = ChooseRandomAttack();
				if(r > 30f || enrage < 0.25f)
                {
					attacking = true;
					StartCoroutine(DashAttack(enrageSpeedBuffMultiplier));
				}
                else
                {
					attacking = true;

					StartCoroutine(DashAttack(enrageSpeedBuffMultiplier));

					enrage -= 0.1f;
					EnrageUpdate();
				}
			}
        }
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
		return Random.Range(min, max);
	}

	private void FaceThePlayer()
	{
		Vector3 direction = player.transform.position - transform.position;

		float dir = direction.x;

		if (dir < 0)
		{
			if (!renderer.flipX)
			{
				renderer.flipX = true;
			}
		}
		else
		{
			if (renderer.flipX)
			{
				renderer.flipX = false;
			}
		}
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
			enrage += 0.1f;
			EnrageUpdate();

			if (channelingEnrageSummon)
            {
				nextDeathbringerSummonHealth += 50f;
				HealthBar dbHealth = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();
				dbHealth.IncreaseHealth(50f);
				lifeLeachAnim.Play("darkSpellActivation");
			}

			if (currentHealth > 0)
			{
				audioManager.PlayEnemyImpact();
			}
			else
			{
				isDead = true;
				boxCollider.enabled = false;
				StopAllCoroutines();
				audioManager.PlayNecromancerDeathSound();
				animator.Play("Necromancer_Death");
				StartCoroutine(necromancerDied());
			}
		}
		else
		{
			if (!isDead)
			{
				isDead = true;
				boxCollider.enabled = false;
				StopAllCoroutines();
				audioManager.PlayNecromancerDeathSound();
				animator.Play("Necromancer_Death");
				StartCoroutine(necromancerDied());
			}
		}
	}

	private void GainHealing(float healingAmount)
    {
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

	private void SiphonFromSkeletons()
    {
		foreach(SummonedSkeleton skelly in summonedSkeletons)
        {
			skelly.ConsumeDeath();
			GainHealing(25f);
		}

		amountOfSkeletonsAlive = 0;
		summonedSkeletons.Clear();
    }

	private IEnumerator EnragedSummon()
    {
		rbAI.velocity = Vector2.zero;
		audioManager.PlayNecromancerBigSummonSound();
		animator.Play("enrageSummon");

		nextDeathbringerSummonHealth = 100; //starting health
		uiManager.SetDeathBringerUI(true);
		HealthBar dbHealth = GameObject.FindGameObjectWithTag("DeathBringerHealthbar").GetComponentInChildren<HealthBar>();
		dbHealth.SetUpHealthBarWithDecreasedHealth(1000, nextDeathbringerSummonHealth);

		yield return new WaitForSeconds(0.4f);
		//variable to make sure that if the boss is hit during this time, it will buff the summon health;
		channelingEnrageSummon = true;
		yield return new WaitForSeconds(2.8f);
		channelingEnrageSummon = false;

		//spawnedDeathBringer = GameObject.Instantiate(summonDeathBringer, transform.position, summonDeathBringer.transform.rotation);

		Vector3 SpawnLoc = new Vector3(transform.position.x, -1.8f, transform.position.z);
		deathBringer.SetSummonedData(SpawnLoc, nextDeathbringerSummonHealth);
		spawnedDeathBringer = true;
		//spawnedDeathBringer.GetComponent<DeathBringer>().SetSummonedData(SpawnLoc, nextDeathbringerSummonHealth);
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(1);
	}

	private IEnumerator projectileBarrage()
	{
		Vector3 jumpLocation;
		float distanceToAttackFrom = Random.Range(5f, 25f);

		if(player.transform.position.x < -35f) //cant go left
        {
			float newX = player.transform.position.x + distanceToAttackFrom;
			if (newX > 95f) newX = 95f;

			jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
        }
		else if (player.transform.position.x > 95f) //cant go right
        {
			float newX = player.transform.position.x - distanceToAttackFrom;
			if (newX < -35f) newX = -35f;

			jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
		}
        else
        {
			float rand = ChooseRandomAttack();

			if(rand < 50)
            {
				float newX = player.transform.position.x + distanceToAttackFrom;
				if (newX > 95f) newX = 95f;

				jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
			}
            else
            {
				float newX = player.transform.position.x - distanceToAttackFrom;
				if (newX < -35f) newX = -35f;

				jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
			}
        }

		groundFire.Play();

		rbAI.velocity = Vector2.zero;
		currentStrength = strength;

		FaceThePlayer();

		animator.Play("Necromancer_Jump");
		audioManager.PlayNecromancerJumpSound();

		float elapsedTime = 0f;

		Vector2 jumpPosition;

		Vector3 direction = player.transform.position - transform.position;

		float dir = direction.x;

		if (dir > 0)
		{
			jumpPosition = new Vector2(transform.position.x + 1.5f, transform.position.y + 5f);
		}
		else
		{
			jumpPosition = new Vector2(transform.position.x - 1.5f, transform.position.y + 5f);
		}

		while (elapsedTime < 0.6f)
		{
			elapsedTime += Time.deltaTime;
			Vector2 targ = new Vector2(Mathf.Lerp(transform.position.x, jumpPosition.x, elapsedTime / 0.6f), Mathf.Lerp(transform.position.y, jumpPosition.y, elapsedTime / 0.6f));

			rbAI.MovePosition(targ);
			yield return null;
		}

		elapsedTime = 0f;

		FaceThePlayer();

		animator.Play("Necromancer_JumpToFall");
		yield return new WaitForSeconds(0.3f);
		animator.Play("Necromancer_Fall");
		audioManager.PlayNecromancerJumpSound();

		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;

			Vector2 targ = new Vector2(Mathf.Lerp(transform.position.x, jumpLocation.x, elapsedTime / 0.3f), Mathf.Lerp(transform.position.y, (jumpLocation.y), elapsedTime / 0.3f));

			rbAI.MovePosition(targ);
			yield return null;
		}

		rbAI.velocity = Vector2.zero;

		FaceThePlayer();

		bool cancelAttack = false;

		int amountOfProjectilesCast = 0;

        while (playerDistance() > minDist && !cancelAttack)
        {
            animator.Play("Necromancer_Spellcast");
			audioManager.PlayNecromancerAttackSound();
            yield return new WaitForSeconds(0.4f);

			float xSpawn;

            if (renderer.flipX)
            {
				xSpawn = transform.position.x - 1f;
            }
            else
            {
				xSpawn = transform.position.x + 1f;
			}

            Vector3 SpawnLoc = new Vector3(xSpawn, -3f, transform.position.z);

            GameObject.Instantiate(darkProjectile, SpawnLoc, darkProjectile.transform.rotation);
			amountOfProjectilesCast++;

			if (amountOfProjectilesCast > 25f) cancelAttack = true;

			float randWaitTime = Random.Range(0.75f, 2f);

			yield return new WaitForSeconds(randWaitTime);
		}

        attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(1);
	}

	private IEnumerator dodgeBack()
	{
		Vector3 jumpLocation;
		float distanceToJump = Random.Range(5f, 10f);

		if (player.transform.position.x < -35f) //cant go left
		{
			float newX = player.transform.position.x + distanceToJump;
			if (newX > 95f) newX = 95f;

			jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
		}
		else if (player.transform.position.x > 95f) //cant go right
		{
			float newX = player.transform.position.x - distanceToJump;
			if (newX < -35f) newX = -35f;

			jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
		}
		else
		{
			float rand = ChooseRandomAttack();

			if (rand < 50)
			{
				float newX = player.transform.position.x + distanceToJump;
				if (newX > 95f) newX = 95f;

				jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
			}
			else
			{
				float newX = player.transform.position.x - distanceToJump;
				if (newX < -35f) newX = -35f;

				jumpLocation = new Vector3(newX, transform.position.y, transform.position.z);
			}
		}
		rbAI.velocity = Vector2.zero;
		currentStrength = strength;

		FaceThePlayer();

		float elapsedTime = 0f;

		audioManager.PlayNecromancerJumpSound();
		animator.Play("Necromancer_JumpToFall");
		yield return new WaitForSeconds(0.3f);
		animator.Play("Necromancer_Fall");
		audioManager.PlayNecromancerJumpSound();
		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;

			Vector2 targ = new Vector2(Mathf.Lerp(transform.position.x, jumpLocation.x, elapsedTime / 0.3f), Mathf.Lerp(transform.position.y, (jumpLocation.y), elapsedTime / 0.3f));

			rbAI.MovePosition(targ);
			yield return null;
		}

		attacking = false;
	}

	private IEnumerator SiphonAttack()
    {
		animator.Play("Necromancer_Spellcast");
		yield return new WaitForSeconds(1);
		SiphonFromSkeletons();
		consumeDelayTimer.StartTimer(consumeDelayTime);
		yield return new WaitForSeconds(1);
		attacking = false;
	}

	private void SummonSkeletons(int amountSkeletons)
	{
		float minX = -35;
		float maxX = 95f;

		float minimumSummonRange = 3f;

		bool canSummonLeft = false;
		bool canSummonRight = false;

		if (transform.position.x > (minX + minimumSummonRange)) canSummonLeft = true;
		if (transform.position.x < (maxX - minimumSummonRange)) canSummonRight = true;

		Vector3 summonLocation = new Vector3(transform.position.x, transform.position.y, 0);

		List<float> xLocationsSpawnedSkeletons = new List<float>();

		for (int i = 0; i < amountSkeletons; i++)
		{
			if (canSummonRight && canSummonLeft)
			{
				int rand = Random.Range(0, 2);

				if (rand == 0)
				{
					float randomX = Random.Range((transform.position.x - minimumSummonRange), (minX + minimumSummonRange));
					summonLocation.x = randomX;
				}
				else
				{
					float randomX = Random.Range((transform.position.x + minimumSummonRange), (maxX - minimumSummonRange));
					summonLocation.x = randomX;
				}
			}
			else if (canSummonLeft)
			{
				float randomX = Random.Range((transform.position.x - minimumSummonRange), (minX + minimumSummonRange));
				summonLocation.x = randomX;
			}
			else
			{
				float randomX = Random.Range((transform.position.x + minimumSummonRange), (maxX - minimumSummonRange));
				summonLocation.x = randomX;
			}

			bool canSummonOnThisLocation = true;

			if (xLocationsSpawnedSkeletons.Count > 0)
			{
				for (int t = 0; t < xLocationsSpawnedSkeletons.Count; t++)
				{
					if (Mathf.Abs(xLocationsSpawnedSkeletons[t] - summonLocation.x) < 1.5f)
					{
						canSummonOnThisLocation = false;
						break;
					}
				}
			}

			if (canSummonOnThisLocation)
			{
				xLocationsSpawnedSkeletons.Add(summonLocation.x);

				summonLocation.y = summonSkeleton.transform.position.y;
				GameObject summonedSkeleton = GameObject.Instantiate(summonSkeleton, summonLocation, summonSkeleton.transform.rotation);
				summonedSkeletons.Add(summonedSkeleton.GetComponent<SummonedSkeleton>());
				amountOfSkeletonsAlive++;
			}
			else
			{
				if (amountSkeletons < 50)
				{
					amountSkeletons++;
				}
			}
		}
	}

	private IEnumerator summonSkeletonsAttack()
    {
		rbAI.velocity = Vector2.zero;
		audioManager.PlayNecromancerAttackSound();
		animator.Play("Necromancer_Summon");
		yield return new WaitForSeconds(1.2f);
		int rand = (int)Mathf.Round(Random.Range((1f * fightDurationMultiplier), (3f * fightDurationMultiplier)));
		SummonSkeletons(rand);
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		yield return new WaitForSeconds(1);
	}

	private IEnumerator necromancerDied()
	{
		enrage = 0;
		EnrageUpdate();
		if (spawnedDeathBringer) deathBringer.TakeDamage(1000);
		yield return new WaitForSeconds(1);
		renderer.enabled = false;
        foreach (SummonedSkeleton skelly in summonedSkeletons)
        {
			skelly.ConsumeDeath();
        }
		amountOfSkeletonsAlive = 0;
		StartCoroutine(gameManager.NecromancerDefeated());
		yield return new WaitForSeconds(2);
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
			Vector3 moveDirection = player.transform.position - transform.position;

			float fixedSpeed;

			if (moveDirection.x > 0)
			{
				fixedSpeed = speed;
			}
			else
			{
				fixedSpeed = -speed;
			}

			moveDirection.y = transform.position.y; //dont move the y

			rbAI.velocity = new Vector2(fixedSpeed, rbAI.velocity.y);
			animator.SetBool("Walking", true);

			if (rbAI.velocity.x < 0)
			{
				if (!renderer.flipX)
				{
					renderer.flipX = true;
				}
			}
			else
			{
				if (renderer.flipX)
				{
					renderer.flipX = false;
				}
			}
		}
	}

	private IEnumerator JumpAttack()
    {
		groundFire.Play();

		rbAI.velocity = Vector2.zero;
		currentStrength = strength;

		Vector3 targetPosition = player.transform.position;
		targetPosition.y = transform.position.y;

		FaceThePlayer();

		animator.Play("Necromancer_Jump");
		audioManager.PlayNecromancerJumpSound();

		float elapsedTime = 0f;

		Vector2 jumpPosition;

		Vector3 direction = player.transform.position - transform.position;

		float dir = direction.x;

		if (dir > 0)
        {
			jumpPosition = new Vector2(transform.position.x + 1.5f, transform.position.y + 5f);
        }
		else
        {
			jumpPosition = new Vector2(transform.position.x - 1.5f, transform.position.y + 5f);
		}

		while (elapsedTime < 0.6f)
		{
			elapsedTime += Time.deltaTime;
			Vector2 targ = new Vector2(Mathf.Lerp(transform.position.x, jumpPosition.x, elapsedTime / 0.6f), Mathf.Lerp(transform.position.y, jumpPosition.y, elapsedTime / 0.6f));

			rbAI.MovePosition(targ);
			yield return null;
		}

		elapsedTime = 0f;

		FaceThePlayer();

		animator.Play("Necromancer_JumpToFall");
		yield return new WaitForSeconds(0.3f);
		animator.Play("Necromancer_Fall");
		audioManager.PlayNecromancerJumpSound();

		while (elapsedTime < 0.3f)
		{
			elapsedTime += Time.deltaTime;

			Vector2 targ = new Vector2(Mathf.Lerp(transform.position.x, targetPosition.x, elapsedTime / 0.3f), Mathf.Lerp(transform.position.y, (targetPosition.y), elapsedTime / 0.3f));

			rbAI.MovePosition(targ);
			yield return null;
		}
		StartCoroutine(Melee(strength * 2, 2f));

		yield return new WaitForSeconds(1f);
	}

	private IEnumerator DashAttack(float additionalSpeedMultiplier)
	{
		rbAI.velocity = Vector2.zero;
		currentStrength = strength;

		Vector3 targetPosition = player.transform.position;
		targetPosition.y = transform.position.y;

		float speedMultiplier = 3f * additionalSpeedMultiplier;

		FaceThePlayer();

		animator.SetBool("Walking",true);
		animator.speed = 1.5f;

		bool inMeleeRange = false;

		var vec = transform.position - targetPosition;
		vec.y = 0;
		float dist = vec.magnitude;

		while (!inMeleeRange)
        {
			Vector3 moveDirection = targetPosition - transform.position;

			float fixedSpeed;

			if (moveDirection.x > 0)
			{
				fixedSpeed = (speed * speedMultiplier);
			}
			else
			{
				fixedSpeed = ((-speed) * speedMultiplier);
			}

			moveDirection.y = transform.position.y; //dont move the y

			rbAI.velocity = new Vector2(fixedSpeed, rbAI.velocity.y);
			animator.SetBool("Walking", true);

			if (rbAI.velocity.x < 0)
			{
				if (!renderer.flipX)
				{
					renderer.flipX = true;
				}
			}
			else
			{
				if (renderer.flipX)
				{
					renderer.flipX = false;
				}
			}

			vec = transform.position - targetPosition;
			vec.y = 0;
			dist = vec.magnitude;

			if (dist <= minDist) inMeleeRange = true;

			yield return null;
		}

		rbAI.velocity = Vector2.zero;
		animator.SetBool("Walking", false);
		animator.speed = 1;

		StartCoroutine(Melee(strength * 2, 2f));

		yield return new WaitForSeconds(1f);
	}

	private IEnumerator Melee(float str, float attSpeed)
	{
		rbAI.velocity = Vector2.zero;
		audioManager.PlayNecromancerAttackSound();
		animator.speed = attSpeed;
		animator.Play("Necromancer_Attack");
		currentStrength = str;

		yield return new WaitForSeconds(0.6f / attSpeed);
		audioManager.PlayDeathBringerSwingSound();
		meleeAttackTrigger.transform.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.3f / attSpeed);
		meleeAttackTrigger.transform.gameObject.SetActive(false);
        yield return new WaitForSeconds(.45f / attSpeed);
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		animator.speed = 1f;
		yield return new WaitForSeconds(0.5f / attSpeed);
	}

	private void OnTriggerStay2D(Collider2D triggerCollision)
	{
		if (triggerCollision.gameObject.tag == "Player")
		{
			triggerCollision.gameObject.GetComponent<IDamageable>().TakeDamage(currentStrength);
		}
	}
}
