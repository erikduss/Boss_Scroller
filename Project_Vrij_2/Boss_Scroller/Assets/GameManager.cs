using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject bossRoomBorders;
    [SerializeField] DeathBringer deathBringerEnemy;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LockBossRoom()
    {
        bossRoomBorders.SetActive(true);
        StartCoroutine(deathBringerEnemy.ActivateBoss());
    }
}
