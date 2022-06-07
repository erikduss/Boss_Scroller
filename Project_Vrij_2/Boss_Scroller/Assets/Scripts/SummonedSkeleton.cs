using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonedSkeleton : MonoBehaviour, IDamageable
{
	private float maxHealth = 10;
	private float currentHealth = 10;

	private float strength = 5;
	private float currentStrength = 5;

	private float minDist = 1.25f;

	private float speed = 1f;

	private Animator animator;
	private Rigidbody2D rbAI;
	private BoxCollider2D boxCollider;
	private SpriteRenderer renderer;

	private GameObject playerObject;
	private AudioManager audioManager;

	private float attackFatigueTime = 1f;
	private WaitTimer attackFatigueTimer = new WaitTimer();

	private float walkFatigueTime = 2f;
	private WaitTimer walkFatigueTimer = new WaitTimer();

	[SerializeField] private BoxCollider2D meleeAttackTrigger;

	[SerializeField] private ParticleSystem consumeFire;

	private Necromancer necromancer;

	public bool isDead = false;

	private bool attacking = false;

	private bool canAttack = false;

	// Start is called before the first frame update
	void Start()
    {
        playerObject = GameObject.FindGameObjectWithTag("Player");

		animator = GetComponent<Animator>();
		rbAI = GetComponent<Rigidbody2D>();
		boxCollider = GetComponent<BoxCollider2D>();
		renderer = GetComponent<SpriteRenderer>();

		necromancer = FindObjectOfType<Necromancer>();

		audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();

		attackFatigueTimer.StartTimer(RandomFatigueTime(1.4f, 2.7f));

		StartCoroutine(reborn());
	}

    // Update is called once per frame
    void Update()
    {
		if (currentHealth <= 0 || isDead || attacking || !canAttack)
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

	public void ConsumeDeath()
    {
		isDead = true;
		StopAllCoroutines();
		consumeFire.Play();
		boxCollider.enabled = false;
		animator.Play("death");
		StartCoroutine(skeletonDied());
	}

	private void ChooseNewAttack()
    {
		//The enemy is fatigued and cannot execute another action
		if (!attackFatigueTimer.TimerFinished())
		{
			return;
		}

		animator.SetBool("Walking", false);

		if (playerDistance() <= minDist)
		{
			attacking = true;
			StartCoroutine(Melee(strength));
		}
	}

	private bool IsAllowedToMove()
	{
		return walkFatigueTimer.TimerFinished();
	}

	private float RandomFatigueTime(float min, float max)
	{
		return Random.Range(min, max);
	}

	private void FaceThePlayer()
	{
		Vector3 direction = playerObject.transform.position - transform.position;

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

	public void TakeDamage(float damage)
	{
		if (currentHealth >= 0)
		{
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
				animator.Play("death");
				StartCoroutine(skeletonDied());
			}
		}
		else
		{
			if (!isDead)
			{
				isDead = true;
				boxCollider.enabled = false;
				StopAllCoroutines();
				animator.Play("death");
				StartCoroutine(skeletonDied());
			}
		}
	}

	private IEnumerator skeletonDied()
	{
		yield return new WaitForSeconds(1);
		renderer.enabled = false;
		StartCoroutine(DestroyThis());
	}

	private IEnumerator DestroyThis()
    {
		yield return new WaitForSeconds(1);
		if (necromancer.summonedSkeletons.Contains(this))
        {
			necromancer.summonedSkeletons.Remove(this);
			necromancer.amountOfSkeletonsAlive--;
        }
		yield return new WaitForSeconds(1);
		Destroy(this.gameObject);
    }

	private float playerDistance()
	{
		var vec = transform.position - playerObject.transform.position;
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
			Vector3 moveDirection = playerObject.transform.position - transform.position;

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

	private IEnumerator reborn()
    {
		yield return new WaitForSeconds(1.4f);
		canAttack = true;
    } 

	private IEnumerator Melee(float str)
	{
		rbAI.velocity = Vector2.zero;

		FaceThePlayer();
		//audioManager.PlayDeathBringerAttackSound();

		int rand = Random.Range(0,2);

		if(rand == 0)
        {
			animator.Play("Attack_1");
		}
        else
        {
			animator.Play("Attack_2");
		}
		
		currentStrength = str;

		yield return new WaitForSeconds(0.35f);
		//audioManager.PlayDeathBringerSwingSound();
		meleeAttackTrigger.transform.gameObject.SetActive(true);
		if (!renderer.flipX)
		{
			meleeAttackTrigger.offset = new Vector2(0.1f, -0.16f);
		}
		else
		{
			meleeAttackTrigger.offset = new Vector2(-0.1f, -0.16f);
		}
		yield return new WaitForSeconds(0.2f);
		meleeAttackTrigger.transform.gameObject.SetActive(false);
		yield return new WaitForSeconds(.45f);
		attacking = false;
		attackFatigueTimer.StartTimer(attackFatigueTime);
		walkFatigueTimer.StartTimer(walkFatigueTime);
		//animator.Play("Idle"); not needed?
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
