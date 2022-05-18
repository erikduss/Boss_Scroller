using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingApple : MonoBehaviour
{
    private Rigidbody2D rbApple;
    private Vector2 startPos;

    private bool falling = false;

    private WaitTimer fallTimer = new WaitTimer();

    private float minWaitTime = 5f;
    private float maxWaitTime = 15f;

    private GameManager gameManager;

    private bool initiatedTimer = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        rbApple = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameStarted) return;

        if (!initiatedTimer)
        {
            StartNewTimer();
            initiatedTimer = true;
        }

        if (fallTimer.TimerFinished() && transform.position.y > -25f)
        {
            rbApple.gravityScale = 1f;
            falling = true;
        }
        else
        {
            rbApple.velocity = Vector2.zero;
            rbApple.gravityScale = 0;
            transform.position = startPos;
            falling = false;

            if (fallTimer.TimerFinished())
            {
                StartNewTimer();
            }
        }
    }

    private void StartNewTimer()
    {
        float rand = Random.Range(minWaitTime, maxWaitTime);
        fallTimer.StartTimer(rand);
    }

    private void OnTriggerStay2D(Collider2D triggerCollision)
    {
        if (triggerCollision.gameObject.tag == "Player")
        {
            triggerCollision.gameObject.GetComponent<IDamageable>().TakeDamage(0);
        }
    }
}
