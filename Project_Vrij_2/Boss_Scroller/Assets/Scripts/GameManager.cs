using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject bossRoomBorders;
    [SerializeField] public DeathBringer deathBringerEnemy;
    private SmoothCamera cam;
    private UIManager uiManager;

    [SerializeField] private AudioManager audioManager;

    private GameObject playerObject;
    public bool playerIsAlive = true;

    private Player player;

    [SerializeField] private BoxCollider2D bossFightTrigger;
    public bool gameStarted = false;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.GetComponent<SmoothCamera>();
        uiManager = GetComponent<UIManager>();
        playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.GetComponent<Player>();
        player.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        uiManager.SetUIButtonStates(false);
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
        gameStarted = true;
        uiManager.SetTutorialText(true);
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

    public IEnumerator DeathBringerDefeated()
    {
        uiManager.SetBossUI(false);
        audioManager.StopAllSoundEffects();
        yield return new WaitForSeconds(5f);
        uiManager.SetEndDemoPanelAlpha(1,2f);
        yield return new WaitForSeconds(2f);
        cam.target = playerObject.transform;
        yield return new WaitForSeconds(3f);
        bossRoomBorders.SetActive(false);
        player.SetDefaultValues();
        deathBringerEnemy.ResetToDefaults();
        StartCoroutine(uiManager.ResetAllUI());
        StartCoroutine(audioManager.FadeAndChangeMusic(1f, MusicType.MENU));
        yield return new WaitForSeconds(1f);
        player.enabled = false;
        playerIsAlive = true;
        bossFightTrigger.gameObject.SetActive(true);
    }

    public IEnumerator GameOver()
    {
        uiManager.SetDeathPanelAlpha(1,2f);
        yield return new WaitForSeconds(2f);
        cam.target = playerObject.transform;
        yield return new WaitForSeconds(3f);
        bossRoomBorders.SetActive(false);
        player.SetDefaultValues();
        deathBringerEnemy.ResetToDefaults();
        StartCoroutine(uiManager.ResetAllUI());
        StartCoroutine(audioManager.FadeAndChangeMusic(1f, MusicType.MENU));
        yield return new WaitForSeconds(1f);
        player.enabled = false;
        playerIsAlive = true;
        bossFightTrigger.gameObject.SetActive(true);
    }
}
