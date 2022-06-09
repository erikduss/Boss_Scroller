using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject bossRoomBorders;
    [SerializeField] private GameObject rightBossRoomBorder;
    [SerializeField] public DeathBringer deathBringerEnemy;
    [SerializeField] public Necromancer necromancerEnemy;
    private SmoothCamera cam;
    private UIManager uiManager;

    [SerializeField] private AudioManager audioManager;

    private GameObject playerObject;
    public bool playerIsAlive = true;

    private Player player;
    private WaitTimer correctionTimer = new WaitTimer();

    [SerializeField] private BoxCollider2D bossFightTrigger;
    public bool gameStarted = false;

    public bool activatedNecromancer = false;

    private GameState currentGameState;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.GetComponent<SmoothCamera>();
        uiManager = GetComponent<UIManager>();
        playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject.GetComponent<Player>();
        player.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        currentGameState = GameState.IN_MENU;
        correctionTimer.StartTimer(5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (correctionTimer.TimerFinished())
        {
            switch (currentGameState)
            {
                case GameState.IN_MENU:
                        if (!Cursor.visible || Cursor.lockState == CursorLockMode.Locked || gameStarted || playerObject.GetComponent<Player>().enabled || cam.target != playerObject.transform || cam.maxXValue != 0f || cam.minXValue != -30f || !playerIsAlive || !bossFightTrigger.enabled || bossRoomBorders.activeInHierarchy || activatedNecromancer)
                        {
                            StartCoroutine(RestoreIncorrectionsInMenu());
                            correctionTimer.StartTimer(15f);
                        }
                    break;
                case GameState.TUTORIAL:
                        if (Cursor.visible || Cursor.lockState != CursorLockMode.Locked || !gameStarted || !playerObject.GetComponent<Player>().enabled)
                        {
                            StartGame();
                            correctionTimer.StartTimer(7.5f);
                        }
                    break;
                case GameState.DEATHBRINGER:
                        if (!bossRoomBorders.activeInHierarchy || !deathBringerEnemy.combatEnabled)
                        {
                            LockDeathbringerBossRoom();
                            correctionTimer.StartTimer(6f);
                        }
                    break;
                case GameState.NECROMANCER:
                        if (cam.maxXValue != 90f || cam.minXValue != 0f || cam.target != playerObject.transform || !bossRoomBorders.activeInHierarchy || !activatedNecromancer)
                        {
                            StartCoroutine(DeathBringerDefeated());
                            correctionTimer.StartTimer(10f);
                        }
                    break;
            }
        }
    }

    public void StartGame()
    {
        currentGameState = GameState.TUTORIAL;
        correctionTimer.StartTimer(7.5f);
        uiManager.SetUIButtonStates(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

    public void LockDeathbringerBossRoom()
    {
        currentGameState = GameState.DEATHBRINGER;
        correctionTimer.StartTimer(6f);
        bossRoomBorders.SetActive(true);
        StartCoroutine(audioManager.FadeAndChangeMusic(2f, MusicType.BOSS));
        uiManager.SetDeathBringerUI(true);
        StartCoroutine(deathBringerEnemy.ActivateBoss());
        cam.SetStaticCamera(new Vector3(0,0,0));
    }

    public void LockNecromancerBossRoom()
    {
        bossRoomBorders.SetActive(true);
        StartCoroutine(audioManager.FadeAndChangeMusic(2f, MusicType.BOSS_NECROMANCER));
        StartCoroutine(necromancerEnemy.ActivateBoss());
        uiManager.SetNecromancerUI(true);
        activatedNecromancer = true;
    }

    private void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public IEnumerator DeathBringerDefeated()
    {
        currentGameState = GameState.NECROMANCER;
        correctionTimer.StartTimer(10f);
        uiManager.SetDeathBringerUI(false);
        audioManager.StopAllSoundEffects();
        cam.target = playerObject.transform;
        cam.maxXValue = 90f;
        cam.minXValue = 0f;
        rightBossRoomBorder.transform.position = new Vector3(100, rightBossRoomBorder.transform.position.y, rightBossRoomBorder.transform.position.z);
        yield return new WaitForSeconds(5f);
        player.RestoreHealth();
        //defeating the death bringer enables the necromancer
        cam.target = necromancerEnemy.transform;
        yield return new WaitForSeconds(0.25f);
        necromancerEnemy.animator.Play("rebornNecromancer");
        yield return new WaitForSeconds(0.75f);
        LockNecromancerBossRoom();
        yield return new WaitForSeconds(2.5f);
        cam.target = playerObject.transform;
    }

    public IEnumerator NecromancerDefeated()
    {
        currentGameState = GameState.IN_MENU;
        correctionTimer.StartTimer(15f);
        uiManager.SetNecromancerUI(false);
        audioManager.StopAllSoundEffects();
        cam.target = playerObject.transform;
        yield return new WaitForSeconds(1);
        StartCoroutine(EndTheGame());
    }

    //combine this function with the game over?
    public IEnumerator EndTheGame()
    {
        currentGameState = GameState.IN_MENU;
        correctionTimer.StartTimer(15f);
        yield return new WaitForSeconds(5f);
        uiManager.SetEndDemoPanelAlpha(1, 2f);
        yield return new WaitForSeconds(2f);
        cam.target = playerObject.transform;
        yield return new WaitForSeconds(3f);
        ResetToDefaults();
        StartCoroutine(audioManager.FadeAndChangeMusic(1f, MusicType.MENU));
        yield return new WaitForSeconds(1f);
        player.enabled = false;
        playerIsAlive = true;
        bossFightTrigger.gameObject.SetActive(true);
        cam.maxXValue = 0f;
        cam.minXValue = -30f;
        EnableCursor();
    }

    private void ResetToDefaults()
    {
        bossRoomBorders.SetActive(false);
        player.SetDefaultValues();
        deathBringerEnemy.ResetToDefaults();
        necromancerEnemy.ResetToDefaults();
        StartCoroutine(uiManager.ResetAllUI());
        activatedNecromancer = false;
        gameStarted = false;
    }

    private IEnumerator RestoreIncorrectionsInMenu()
    {
        currentGameState = GameState.IN_MENU;
        correctionTimer.StartTimer(5f);
        yield return new WaitForSeconds(1f);
        cam.target = playerObject.transform;
        yield return new WaitForSeconds(1f);
        ResetToDefaults();
        yield return new WaitForSeconds(1f);
        player.enabled = false;
        playerIsAlive = true;
        bossFightTrigger.gameObject.SetActive(true);
        cam.maxXValue = 0f;
        cam.minXValue = -30f;
        EnableCursor();
    }

    public IEnumerator GameOver()
    {
        currentGameState = GameState.IN_MENU;
        correctionTimer.StartTimer(15f);
        StartCoroutine(audioManager.PlayDefeatAudio());
        uiManager.SetDeathPanelAlpha(1,2f);
        yield return new WaitForSeconds(2f);
        cam.target = playerObject.transform;
        yield return new WaitForSeconds(3f);
        ResetToDefaults();
        StartCoroutine(audioManager.FadeAndChangeMusic(1f, MusicType.MENU));
        yield return new WaitForSeconds(1f);
        player.enabled = false;
        playerIsAlive = true;
        bossFightTrigger.gameObject.SetActive(true);
        cam.maxXValue = 0f;
        cam.minXValue = -30f;
        EnableCursor();
    }
}
