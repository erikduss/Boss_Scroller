using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject BossUI;

    // Start is called before the first frame update
    void Start()
    {
        PlayerUI.SetActive(false);
        BossUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnablePlayerUI()
    {
        PlayerUI.SetActive(true);
    }

    public void EnableBossUI()
    {
        BossUI.SetActive(true);
    }
}
