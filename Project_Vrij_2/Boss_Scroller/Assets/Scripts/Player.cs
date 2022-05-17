using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    private float playerSpeed = 2.5f;
    private Rigidbody2D rbPlayer;
    private Animator animPlayer;

    private Vector3 startPos;

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

    [SerializeField] private HealthBar healthBar;
    private float maxHealth = 100;
    private float currentHealth = 100;

    private GameManager gameManager;
    private AudioManager audioManager;

    // Use this for initialization
    void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
        rbPlayer = this.GetComponent<Rigidbody2D>();
        animPlayer = this.GetComponent<Animator>();

        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();

        renderer = GetComponent<SpriteRenderer>();

        startPos = this.transform.position;

        defaultAttackTriggerOffset = Attack_Trigger.offset;

        if(healthBar != null)
        {
            healthBar.SetUpHealthBar(maxHealth);
        }
    }

    void Update()
    {
        if (!gameManager.playerIsAlive) return;

        if (damaging)
        {
            if (objectToDamage != null && !alreadyDamaged)
            {
                objectToDamage.TakeDamage(attackDamage);
                alreadyDamaged = true;
            }
        }

        CheckInputs();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //CheckInputs();
    }

    public void SetDefaultValues()
    {
        currentHealth = maxHealth;
        healthBar.SetUpHealthBar(maxHealth);
        animPlayer.Play("Idle");
        transform.position = startPos;
        playerCollider.enabled = true;
        rbPlayer.velocity = Vector2.zero;
        renderer.flipX = false;
        objectToDamage = null;
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
            AttackAction();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            DodgeAction();
        }
        else
        {
            MovePlayer();
        }
    }

    private void AttackAction()
    {
        rbPlayer.velocity = new Vector2(0, rbPlayer.velocity.y);
        audioManager.PlayPlayerAttackSound();
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

        audioManager.PlaySlidePlayerSound();
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

        if (Mathf.Abs(hor) > 0) audioManager.PlayMovementPlayerSound();

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
            if(currentHealth >= 0)
            {
                healthBar.DecreaseHealth(damage);
                currentHealth -= damage;

                if(currentHealth > 0)
                {
                    animPlayer.Play("Hurt");
                    audioManager.PlayPlayerImpact();

                    playerInvincibilityTimer.StartTimer(.80f);

                    playerCollider.enabled = false;
                }
                else
                {
                    gameManager.playerIsAlive = false;
                    rbPlayer.velocity = Vector2.zero;
                    playerCollider.enabled = false;
                    animPlayer.Play("Death");
                    StartCoroutine(audioManager.PlayDefeatAudio());
                    StartCoroutine(gameManager.GameOver());
                }
            }
            else
            {
                gameManager.playerIsAlive = false;
                rbPlayer.velocity = Vector2.zero;
                playerCollider.enabled = false;
                animPlayer.Play("Death");
                StartCoroutine(audioManager.PlayDefeatAudio());
                StartCoroutine(gameManager.GameOver());
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
        if(triggerCollision.gameObject.name == "BossTrigger")
        {
            gameManager.LockBossRoom();
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
