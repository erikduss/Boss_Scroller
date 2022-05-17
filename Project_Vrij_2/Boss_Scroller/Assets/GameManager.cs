using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject bossRoomBorders;
    [SerializeField] private DeathBringer deathBringerEnemy;
    private SmoothCamera cam;
    private UIManager uiManager;

    [SerializeField] private AudioManager audioManager;

    private GameObject playerObject;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.GetComponent<SmoothCamera>();
        uiManager = GetComponent<UIManager>();
        playerObject = GameObject.FindGameObjectWithTag("Player");
        playerObject.GetComponent<Player>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        uiManager.TransitionToGame();
        StartCoroutine(GameStart());
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private IEnumerator GameStart()
    {
        uiManager.SetAllMainMenuUIImagesAlpha(0f,2.5f);
        StartCoroutine(audioManager.FadeMusic(0f, 2.5f));
        yield return new WaitForSeconds(3f);
        uiManager.SetMenuUI(false);
        uiManager.SetPlayerUI(true);
        audioManager.SetTutorialMusic();
        StartCoroutine(audioManager.FadeMusic(1f, 2.5f));
        playerObject.GetComponent<Player>().enabled = true;
    }

    public void LockBossRoom()
    {
        bossRoomBorders.SetActive(true);
        StartCoroutine(audioManager.FadeAndChangeMusic(2f, MusicType.BOSS));
        StartCoroutine(deathBringerEnemy.ActivateBoss());
        cam.SetStaticCamera(new Vector3(0,0,0));
        uiManager.SetBossUI(true);
    }
}
