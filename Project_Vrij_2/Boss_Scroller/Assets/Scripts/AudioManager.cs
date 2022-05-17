using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource musicAudioSource;

    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip tutorialMusic;
    [SerializeField] private AudioClip bossMusic;

    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private AudioSource deathBringerAudioSource;
    [SerializeField] private AudioSource spellsAudioSource;
    [SerializeField] private AudioSource extraSpellsAudioSource;
    [SerializeField] private AudioSource playerImpactAudioSource;
    [SerializeField] private AudioSource enemyImpactAudioSource;

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

    [SerializeField] private List<AudioClip> enemyImpacts = new List<AudioClip>();
    [SerializeField] private AudioClip playerImpact;

    [SerializeField] private AudioClip defeatStinger;

    private float maxMusicVolume = 0.10f; //Chance this to settings later
    private float maxSFXVolume = 0.15f;

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

        musicAudioSource.clip = mainMenuMusic;
        musicAudioSource.loop = true;
        musicAudioSource.volume = maxMusicVolume;
        musicAudioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        spellsAudioSource.PlayOneShot(deathBringerExplotionSound);
    }

    public void PlaySlidePlayerSound()
    {
        playerAudioSource.PlayOneShot(playerSlideSound);
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
        spellsAudioSource.loop = true;
        spellsAudioSource.clip = deathBringerSpellCloud;
        spellsAudioSource.Play();
        yield return new WaitForSeconds(activationTime);
        extraSpellsAudioSource.PlayOneShot(deathBringerSpellActivation);
        yield return new WaitForSeconds(duration - activationTime);
        spellsAudioSource.loop = false;
        spellsAudioSource.Stop();
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
