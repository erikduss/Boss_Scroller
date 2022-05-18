using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBringerSpell : MonoBehaviour
{
    private float spellDamage = 15;
    private SpriteRenderer renderer;
    private Animator anim;

    [SerializeField] private bool isFastSpell = false;

    private BoxCollider2D spellCollider;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        spellCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        spellCollider.enabled = false;

        StartCoroutine(spellBehaviour());
    }

    private IEnumerator spellBehaviour()
    {
        //spell starts
        spellCollider.enabled = false;
        StartCoroutine(FadeTo(1,1));
        yield return new WaitForSeconds(1f);
        spellCollider.enabled = true;

        float waitTime = 1f;

        if (isFastSpell)
        {
            spellDamage = spellDamage * 2;
            anim.speed = 2;
            waitTime = 0.5f;
        }

        yield return new WaitForSeconds(waitTime);
        //spell damages
        spellCollider.offset = new Vector2(-0.018f, -0.173f);
        spellCollider.size = new Vector2(0.323f, 0.5642f);

        yield return new WaitForSeconds(.65f);

        spellCollider.offset = new Vector2(-0.018f, -0.03f);
        spellCollider.size = new Vector2(0.323f, 0.157f);

        StartCoroutine(FadeTo(0, 1));
        yield return new WaitForSeconds(0.5f);
        spellCollider.enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
        //spell ends
    }

    IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = renderer.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            renderer.color = newColor;
            yield return null;
        }
    }

    private void OnTriggerStay2D(Collider2D spellCollision)
    {
        if(spellCollision.gameObject.tag == "Player")
        {
            spellCollision.gameObject.GetComponent<IDamageable>().TakeDamage(spellDamage);
        }
    }
}
