using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer renderer;
    private Rigidbody2D rb;

    private float strength = 10f;

    private float speed = 2f;

    private bool goingRight = false;

    private bool isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        //altering speed makes the attack really RNG based to dodge
        //speed = Random.Range(2f, 4f);

        goingRight = !FindObjectOfType<Necromancer>().GetComponent<SpriteRenderer>().flipX; //make sure the projectile faces the correct way
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < 0 || transform.position.x > 100)
        {
            StartCoroutine(DestroyThis());
        }

        if (isAlive)
        {
           Move();
        }
    }

    private void Move()
    {
        float fixedSpeed;

        if (goingRight)
        {
            fixedSpeed = speed;
            renderer.flipX = false;
        }
        else
        {
            fixedSpeed = -speed;
            renderer.flipX = true;
        }

        rb.velocity = new Vector2(fixedSpeed, rb.velocity.y);
    }

    private IEnumerator DestroyThis()
    {
        isAlive = false;
        rb.velocity = Vector2.zero;
        anim.Play("darkSpellProjectileHit");
        yield return new WaitForSeconds(1);
        renderer.enabled = false;
        yield return new WaitForSeconds(1);
        Destroy(this.gameObject);
    }

    private void OnTriggerStay2D(Collider2D triggerCollision)
    {
        if (triggerCollision.gameObject.tag == "Player" && isAlive)
        {
            triggerCollision.gameObject.GetComponent<IDamageable>().TakeDamage(strength);
            StartCoroutine(DestroyThis());
        }
    }
}
