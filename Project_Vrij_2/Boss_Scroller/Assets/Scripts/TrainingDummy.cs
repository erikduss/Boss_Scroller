using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();
    }

    public void TakeDamage(float damage)
    {
        audioManager.PlayEnemyImpact();
    }
}
