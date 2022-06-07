using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource musicAudioSource;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip tutorialMusic;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip necromancerBossMusic;

    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource deathBringerAudioSource;
    [SerializeField] private AudioSource necromancerAudioSource;
    [SerializeField] private AudioSource spellsAudioSource;
    [SerializeField] private AudioSource extraSpellsAudioSource;
    [SerializeField] private AudioSource playerImpactAudioSource;
    [SerializeField] private AudioSource enemyImpactAudioSource;
    [SerializeField] private AudioSource statueSpells;

    [SerializeField] private List<AudioClip> playerRunSounds = new List<AudioClip>();
    [SerializeField] private AudioClip playerSlideSound;
    [SerializeField] private AudioClip playerAttackSound;

    [SerializeField] private List<AudioClip> deathBringerSpellAttackSounds = new List<AudioClip>();
    [SerializeField] private AudioClip deathBringerSpellCloud;
    [SerializeField] private AudioClip deathBringerSpellActivation;

    [SerializeField] private List<AudioClip> deathBringerTeleportSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> deathBringerAttackSounds = new List<AudioClip>();
    [SerializeField] private AudioClip deathBringerSwingSound;
    [SerializeField] private AudioClip deathBringerExplotionChanneling;
    [SerializeField] private AudioClip deathBringerExplotionSound;

    [SerializeField] private AudioClip deathBringerDeathSound;

    [SerializeField] private List<AudioClip> necromancerJumpSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> necromancerEffortSounds = new List<AudioClip>();

    [SerializeField] private AudioClip necromancerDeathSound;
    [SerializeField] private AudioClip necromancerUltimateSound;
    [SerializeField] private AudioClip necromancerIntroSound;

    [SerializeField] private AudioClip healingFireSound;
    [SerializeField] private AudioClip healingFireReceiveSound;

    [SerializeField] private List<AudioClip> enemyImpacts = new List<AudioClip>();
    [SerializeField] private AudioClip playerImpact;

    [SerializeField] private AudioClip defeatStinger;

    private float maxMusicVolume;
    private float maxSFXVolume;

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", 0.10f);
            PlayerPrefs.Save();
        }

        if (!PlayerPrefs.HasKey("GeneralVolume"))
        {
            PlayerPrefs.SetFloat("GeneralVolume", 0.15f);
            PlayerPrefs.Save();
        }

        maxMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        maxSFXVolume = PlayerPrefs.GetFloat("GeneralVolume");
    }

    // Start is called before the first frame update
    void Start()
    {
        musicAudioSource = GetComponent<AudioSource>();

        playerAudioSource.volume = maxSFXVolume;
        deathBringerAudioSource.volume = maxSFXVolume;
        spellsAudioSource.volume = maxSFXVolume;
        extraSpellsAudioSource.volume = maxSFXVolume;
        playerImpactAudioSource.volume = maxSFXVolume;
        enemyImpactAudioSource.volume = maxSFXVolume;
        statueSpells.volume = maxSFXVolume;
        necromancerAudioSource.volume = maxSFXVolume;

        musicAudioSource.clip = mainMenuMusic;
        musicAudioSource.loop = true;
        musicAudioSource.volume = maxMusicVolume;
        musicAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestOutSFXAudio()
    {
        int rand = Random.Range(0,12);

        switch (rand)
        {
            case 0:
                PlayMovementPlayerSound();
                break;
            case 1:
                PlayEnemyImpact();
                break;
            case 2:
                PlayPlayerImpact();
                break;
            case 3:
                PlayDeathBringerDeathSound();
                break;
            case 4:
                PlayDeathBringerTeleportSound();
                break;
            case 5:
                PlayDeathBringerAttackSound();
                break;
            case 6:
                PlayDeathBringerSpellAttackSound();
                break;
            case 7:
                PlayDeathBringerSwingSound();
                break;
            case 8:
                PlayDeathBringerExplotionChannelSound();
                break;
            case 9:
                PlayDeathBringerExplotionSound();
                break;
            case 10:
                PlaySlidePlayerSound();
                break;
            case 11:
                PlayPlayerAttackSound();
                break;
        }
    }

    public void UpdateAudioVolumes()
    {
        maxMusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        maxSFXVolume = PlayerPrefs.GetFloat("GeneralVolume");

        playerAudioSource.volume = maxSFXVolume;
        deathBringerAudioSource.volume = maxSFXVolume;
        spellsAudioSource.volume = maxSFXVolume;
        extraSpellsAudioSource.volume = maxSFXVolume;
        playerImpactAudioSource.volume = maxSFXVolume;
        enemyImpactAudioSource.volume = maxSFXVolume;
        statueSpells.volume = maxSFXVolume;
        necromancerAudioSource.volume = maxSFXVolume;

        musicAudioSource.volume = maxMusicVolume;
    }

    public void TestOutNewValues(float music, float sfx)
    {
        maxMusicVolume = music;
        maxSFXVolume = sfx;

        playerAudioSource.volume = maxSFXVolume;
        deathBringerAudioSource.volume = maxSFXVolume;
        spellsAudioSource.volume = maxSFXVolume;
        extraSpellsAudioSource.volume = maxSFXVolume;
        playerImpactAudioSource.volume = maxSFXVolume;
        enemyImpactAudioSource.volume = maxSFXVolume;
        statueSpells.volume = maxSFXVolume;
        necromancerAudioSource.volume = maxSFXVolume;

        musicAudioSource.volume = maxMusicVolume;
    }

    public void StopAllSoundEffects()
    {
        playerAudioSource.Stop();
        deathBringerAudioSource.Stop();
        spellsAudioSource.Stop();
        spellsAudioSource.loop = false;
        extraSpellsAudioSource.Stop();
        playerImpactAudioSource.Stop();
        enemyImpactAudioSource.Stop();
        statueSpells.Stop();
    }

    public void PlayMovementPlayerSound()
    {
        if (!playerAudioSource.isPlaying)
        {
            int rand = Random.Range(0,playerRunSounds.Count);

            playerAudioSource.PlayOneShot(playerRunSounds[rand]);
        }
    }

    public void PlayEnemyImpact()
    {
        int rand = Random.Range(0, enemyImpacts.Count);
        enemyImpactAudioSource.PlayOneShot(enemyImpacts[rand]);
    }
    public void PlayPlayerImpact()
    {
        enemyImpactAudioSource.PlayOneShot(playerImpact);
    }

    public void PlayDeathBringerDeathSound()
    {
        deathBringerAudioSource.PlayOneShot(deathBringerDeathSound);
    }

    public void PlayDeathBringerTeleportSound()
    {
        int rand = Random.Range(0, deathBringerTeleportSounds.Count);
        deathBringerAudioSource.PlayOneShot(deathBringerTeleportSounds[rand]);
    }
    public void PlayDeathBringerAttackSound()
    {
        int rand = Random.Range(0, deathBringerAttackSounds.Count);
        deathBringerAudioSource.PlayOneShot(deathBringerAttackSounds[rand]);
    }
    public void PlayDeathBringerSpellAttackSound()
    {
        int rand = Random.Range(0, deathBringerSpellAttackSounds.Count);
        deathBringerAudioSource.PlayOneShot(deathBringerSpellAttackSounds[rand]);
    }

    public void PlayDeathBringerSwingSound()
    {
        deathBringerAudioSource.PlayOneShot(deathBringerSwingSound);
    }
    public void PlayDeathBringerExplotionChannelSound()
    {
        deathBringerAudioSource.PlayOneShot(deathBringerExplotionChanneling);
    }

    public void PlayDeathBringerExplotionSound()
    {
        spellsAudioSource.Stop();
        spellsAudioSource.PlayOneShot(deathBringerExplotionSound);
    }

    public void PlaySlidePlayerSound()
    {
        playerAudioSource.PlayOneShot(playerSlideSound);
    }

    public void PlayHealingFireSound()
    {
        statueSpells.PlayOneShot(healingFireSound);
    }
    public void PlayHealingFireReceiveSound()
    {
        statueSpells.PlayOneShot(healingFireReceiveSound);
    }

    public IEnumerator PlayDefeatAudio()
    {
        StartCoroutine(FadeMusic(0, 2f));
        yield return new WaitForSeconds(2f);
        musicAudioSource.Stop();
        musicAudioSource.volume = maxMusicVolume;
        musicAudioSource.loop = false;
        musicAudioSource.PlayOneShot(defeatStinger);
    }

    public IEnumerator PlayDeathBringerSpellSound(float duration, float activationTime)
    {
        spellsAudioSource.volume = 0;
        spellsAudioSource.loop = true;
        spellsAudioSource.clip = deathBringerSpellCloud;
        spellsAudioSource.Play();
        StartCoroutine(FadeSpell(maxSFXVolume, 0.75f));
        yield return new WaitForSeconds(activationTime);
        extraSpellsAudioSource.PlayOneShot(deathBringerSpellActivation);
        yield return new WaitForSeconds((duration - activationTime)-0.75f);
        StartCoroutine(FadeSpell(0, 0.75f));
        yield return new WaitForSeconds(0.75f);
        spellsAudioSource.loop = false;
        spellsAudioSource.Stop();
        spellsAudioSource.volume = maxSFXVolume;
    }

    //implement necromancer audio
    public void PlayNecromancerJumpSound()
    {
        int rand = Random.Range(0, necromancerJumpSounds.Count);
        necromancerAudioSource.PlayOneShot(necromancerJumpSounds[rand]);
    }

    public void PlayNecromancerAttackSound()
    {
        int rand = Random.Range(0, necromancerEffortSounds.Count);
        necromancerAudioSource.PlayOneShot(necromancerEffortSounds[rand]);
    }

    public void PlayNecromancerBigSummonSound()
    {
        necromancerAudioSource.PlayOneShot(necromancerUltimateSound);
    }

    public void PlayNecromancerDeathSound()
    {
        necromancerAudioSource.PlayOneShot(necromancerUltimateSound);
    }

    public void PlayNecromancerIntroSound()
    {
        necromancerAudioSource.PlayOneShot(necromancerIntroSound);
    }

    public void PlayPlayerAttackSound()
    {
        playerAudioSource.PlayOneShot(playerAttackSound);
    }

    public void SetTutorialMusic()
    {
        musicAudioSource.clip = tutorialMusic;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void SetBossMusic()
    {
        musicAudioSource.clip = bossMusic;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void SetNecromancerBossMusic()
    {
        musicAudioSource.clip = necromancerBossMusic;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void SetMenuMusic()
    {
        musicAudioSource.clip = mainMenuMusic;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public IEnumerator FadeMusic(float volume, float time)
    {
        float initialVolume = musicAudioSource.volume;
        if(volume > maxMusicVolume) volume = maxMusicVolume;

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(initialVolume, volume, elapsedTime / time);
            yield return null;
        }
    }

    public IEnumerator FadeSpell(float volume, float time)
    {
        float initialVolume = spellsAudioSource.volume;
        if (volume > maxSFXVolume) volume = maxSFXVolume;

        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            spellsAudioSource.volume = Mathf.Lerp(initialVolume, volume, elapsedTime / time);
            yield return null;
        }
    }

    public IEnumerator FadeAndChangeMusic(float time, MusicType type)
    {
        float initialVolume = musicAudioSource.volume;

        float elapsedTime = 0;
        float fixedTime = time / 2;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(initialVolume, 0, elapsedTime / fixedTime);
            yield return null;
        }

        switch (type)
        {
            case MusicType.MENU:
                    SetMenuMusic();
                break;
            case MusicType.TUTORIAL:
                    SetTutorialMusic();
                break;
            case MusicType.BOSS:
                    SetBossMusic();
                break;
            case MusicType.BOSS_NECROMANCER:
                SetNecromancerBossMusic();
                break;
        }

        elapsedTime = 0;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            musicAudioSource.volume = Mathf.Lerp(0, initialVolume, elapsedTime / fixedTime);
            yield return null;
        }
    }
}
