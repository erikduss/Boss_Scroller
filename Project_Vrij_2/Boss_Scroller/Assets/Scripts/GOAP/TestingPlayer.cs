using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingPlayer : MonoBehaviour, IDamageable
{
    private float playerSpeed = 2.5f;
    private Rigidbody2D rbPlayer;
    private Animator animPlayer;

    private Vector3 startPos;

    //implement stamina sytstem
    private float recoveryDelay = 2f;
    private float recoveryRate = 0.1f;
    private float maxStamina = 100;
    private float currentStamina = 100;
    private float dodgeCost = 25f;
    private float attackCost = 20f;

    private WaitTimer staminaRecoverTimer = new WaitTimer();

    private SpriteRenderer renderer;

    private BoxCollider2D playerCollider;

    private float attackCooldown = 0.75f;
    private float dodgeCooldown = 0.5f;

    private float attackDamage = 25f;

    private bool damaging = false;

    private float dodgeVelocity = 250f;

    private float dodgeImmuneTime = 0.4f;

    private WaitTimer playerActionTimer = new WaitTimer();
    private WaitTimer playerInvincibilityTimer = new WaitTimer();

    [SerializeField] private BoxCollider2D Attack_Trigger;

    private Vector2 defaultAttackTriggerOffset;

    private IDamageable objectToDamage;
    private bool alreadyDamaged = false;

    private float maxHealth = 100;
    private float currentHealth = 100;

    // Use this for initialization
    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        rbPlayer = this.GetComponent<Rigidbody2D>();
        animPlayer = this.GetComponent<Animator>();

        renderer = GetComponent<SpriteRenderer>();

        startPos = this.transform.position;

        defaultAttackTriggerOffset = Attack_Trigger.offset;
    }

    void Update()
    {
        if (damaging)
        {
            if (objectToDamage != null && !alreadyDamaged)
            {
                objectToDamage.TakeDamage(attackDamage);
                alreadyDamaged = true;
            }
        }

        CheckInputs();
        RecoverStamina();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //CheckInputs();
    }

    public void SetDefaultValues()
    {
        currentHealth = maxHealth;
        animPlayer.Play("Idle");
        transform.position = startPos;
        playerCollider.enabled = true;
        rbPlayer.velocity = Vector2.zero;
        renderer.flipX = false;
        objectToDamage = null;
    }

    private void RecoverStamina()
    {
        if (currentStamina < maxStamina && staminaRecoverTimer.TimerFinished())
        {
            currentStamina += recoveryRate;
            if (currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }
        }
    }

    private void CheckInputs()
    {
        if (!playerInvincibilityTimer.TimerFinished())
        {
            playerCollider.enabled = false;
        }
        else if (!playerCollider.enabled)
        {
            playerCollider.enabled = true;
        }

        if (!playerActionTimer.TimerFinished()) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (currentStamina >= attackCost)
            {
                AttackAction();
                currentStamina -= attackCost;
                staminaRecoverTimer.StartTimer(recoveryDelay);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentStamina >= dodgeCost)
            {
                DodgeAction();
                currentStamina -= dodgeCost;
                staminaRecoverTimer.StartTimer(recoveryDelay);
            }
        }
        else
        {
            MovePlayer();
        }
    }

    private void AttackAction()
    {
        rbPlayer.velocity = new Vector2(0, rbPlayer.velocity.y);
        animPlayer.Play("Dash_Attack");
        playerActionTimer.StartTimer(attackCooldown);
        StartCoroutine(DamageCollisionDuration(attackCooldown));
    }

    private void DodgeAction()
    {
        rbPlayer.velocity = new Vector2(0, rbPlayer.velocity.y);

        if (renderer.flipX)
        {
            rbPlayer.AddForce(new Vector2(-dodgeVelocity, 0));
        }
        else
        {
            rbPlayer.AddForce(new Vector2(dodgeVelocity, 0));
        }

        GrandPlayerInvincibility(dodgeImmuneTime);

        animPlayer.Play("Slide");
        playerActionTimer.StartTimer(dodgeCooldown);
    }

    public void GrandPlayerInvincibility(float immuneTime)
    {
        playerInvincibilityTimer.StartTimer(immuneTime);
    }

    private void MovePlayer()
    {
        float hor = Input.GetAxis("Horizontal");
        //float ver = Input.GetAxis("Vertical");

        rbPlayer.velocity = new Vector2(hor * playerSpeed, rbPlayer.velocity.y);

        animPlayer.SetFloat("HorizontalSpeed", Mathf.Abs(hor));
        //animPlayer.SetFloat("verticalSpeed", ver);


        if (hor == 0)
        {
            return;
        }

        if (hor < 0)
        {
            if (!renderer.flipX) //Character facing left after code
            {
                renderer.flipX = true;

                //playerCollider.offset = new Vector2(-defaultColliderOffset.x, defaultColliderOffset.y);
                Attack_Trigger.offset = new Vector2(-defaultAttackTriggerOffset.x, defaultAttackTriggerOffset.y);
            }
        }
        else
        {
            if (renderer.flipX) //Character facing right after code
            {
                renderer.flipX = false;

                //playerCollider.offset = defaultColliderOffset;
                Attack_Trigger.offset = defaultAttackTriggerOffset;
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (playerInvincibilityTimer.TimerFinished())
        {
            if (currentHealth >= 0)
            {
                currentHealth -= damage;

                if (currentHealth > 0)
                {
                    animPlayer.Play("Hurt");

                    playerInvincibilityTimer.StartTimer(.80f);

                    playerCollider.enabled = false;
                }
                else
                {
                    rbPlayer.velocity = Vector2.zero;
                    playerCollider.enabled = false;
                    animPlayer.Play("Death");
                }
            }
            else
            {
                rbPlayer.velocity = Vector2.zero;
                playerCollider.enabled = false;
                animPlayer.Play("Death");
            }
        }
    }

    private IEnumerator DamageCollisionDuration(float duration)
    {
        damaging = true;
        yield return new WaitForSeconds(duration);
        damaging = false;
        alreadyDamaged = false;
    }

    private void OnTriggerStay2D(Collider2D triggerCollision)
    {
        if (triggerCollision.gameObject.tag == "DamageAble" && objectToDamage == null)
        {
            objectToDamage = triggerCollision.gameObject.GetComponent<IDamageable>();
        }
        if (triggerCollision.gameObject.name == "BossTrigger")
        {
            triggerCollision.gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D triggerCollision)
    {
        if (triggerCollision.gameObject.tag == "DamageAble")
        {
            objectToDamage = null;
        }
    }
}
