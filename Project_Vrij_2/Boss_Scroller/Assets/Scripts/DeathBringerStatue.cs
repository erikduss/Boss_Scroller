using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerStatue : MonoBehaviour, IDamageable
{
    private AudioManager audioManager;
    private GameManager gameManager;

    private float maxHealth = 75;
    private float currentHealth = 75;

    private float minHealing = 5f;
    private float maxHealing = 20f;

    private float minHealingCooldown = 10f;
    private float maxHealingCooldown = 25f;

    private Animator anim;

    private WaitTimer buffTimer = new WaitTimer();

    private DeathBringer deathBringer;

    private bool isAlive = true;

    [SerializeField] private ParticleSystem healingFire;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        deathBringer = gameManager.deathBringerEnemy;
        anim = GetComponent<Animator>();

        float rand = Random.Range(minHealingCooldown, maxHealingCooldown);
        buffTimer.StartTimer(rand);
    }

    // Update is called once per frame
    void Update()
    {
        if (deathBringer.isDead || !gameManager.playerIsAlive)
        {
            isAlive = false;
            StartCoroutine(DestroyObject());
        }

        if (buffTimer.TimerFinished() && isAlive)
        {
            if(!healingFire.gameObject.activeInHierarchy) healingFire.gameObject.SetActive(true);
            healingFire.Play();
            audioManager.PlayHealingFireSound();
            float healAmount = Random.Range(minHealing, maxHealing);
            StartCoroutine(deathBringer.GainHealing(healAmount));

            float rand = Random.Range(minHealingCooldown, maxHealingCooldown);
            buffTimer.StartTimer(rand);
        }
    }

    private IEnumerator DestroyObject()
    {
        deathBringer.currentAmountOfHealingStatues--;
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth >= 0)
        {
            currentHealth -= damage;
            
            if (currentHealth > 0)
            {
                anim.Play("StatueDamaged");
                audioManager.PlayEnemyImpact();
            }
            else
            {
                isAlive = false;
                anim.Play("StatueDestroyed");
                StartCoroutine(DestroyObject());
            }
        }
        else
        {
            if (isAlive)
            {
                isAlive = false;
                anim.Play("StatueDestroyed");
                StartCoroutine(DestroyObject());
            }
        }
    }
}
