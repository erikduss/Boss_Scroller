using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject bossRoomBorders;
    [SerializeField] private DeathBringer deathBringerEnemy;
    private SmoothCamera cam;
    private UIManager uiManager;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.GetComponent<SmoothCamera>();
        uiManager = GetComponent<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LockBossRoom()
    {
        bossRoomBorders.SetActive(true);
        StartCoroutine(deathBringerEnemy.ActivateBoss());
        cam.SetStaticCamera(new Vector3(0,0,0));
        uiManager.EnableBossUI();
    }
}
