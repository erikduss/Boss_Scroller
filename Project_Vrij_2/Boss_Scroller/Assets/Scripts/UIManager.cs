using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject DeathBringerUI;
    [SerializeField] private GameObject NecromancerUI;

    [SerializeField] private List<Button> mainMenuButtons;
    [SerializeField] private List<Image> mainMenuUIImages;
    [SerializeField] private List<TextMeshProUGUI> mainMenuUIText;

    [SerializeField] private GameObject menuUIPanel;

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsMenuPanel;

    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider generalVolumeSlider;

    private float lastKnownMusicVolume;
    private float lastKnownGeneralVolume;

    [SerializeField] private TextMeshProUGUI musicVolumePercentage;
    [SerializeField] private TextMeshProUGUI generalVolumePercentage;

    [SerializeField] private Toggle ExperimentalToggle;

    [SerializeField] private GameObject deathPanel;
    private Image deathPanelImage;
    [SerializeField] private TextMeshProUGUI deathPanelText;

    [SerializeField] private GameObject endDemoPanel;
    private Image endDemoImage;
    [SerializeField] private TextMeshProUGUI endDemoText;

    [SerializeField] private List<TextMeshProUGUI> tutorialText;

    [SerializeField] private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        PlayerUI.SetActive(false);
        SetDeathBringerUI(false);
        NecromancerUI.SetActive(false);
        deathPanelImage = deathPanel.GetComponent<Image>();
        endDemoImage = endDemoPanel.GetComponent<Image>();

        audioManager = GameObject.FindGameObjectWithTag("AudioController").GetComponent<AudioManager>();

        SetTutorialText(false);

        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SliderValueChanged()
    {
        if (musicVolumeSlider.value != lastKnownMusicVolume || generalVolumeSlider.value != lastKnownGeneralVolume)
        {
            audioManager.TestOutNewValues(musicVolumeSlider.value, generalVolumeSlider.value);
            lastKnownMusicVolume = musicVolumeSlider.value;
            lastKnownGeneralVolume = generalVolumeSlider.value;

            musicVolumePercentage.text = Mathf.Floor(musicVolumeSlider.value * 100) + "%";
            generalVolumePercentage.text = Mathf.Floor(generalVolumeSlider.value * 100) + "%";
        }
    }

    public void OpenOptions()
    {
        optionsMenuPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

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

        if (!PlayerPrefs.HasKey("ExperimentalEnabled"))
        {
            PlayerPrefs.SetInt("ExperimentalEnabled", 0);
            PlayerPrefs.Save();
        }

        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        generalVolumeSlider.value = PlayerPrefs.GetFloat("GeneralVolume");

        int experimentalValue = PlayerPrefs.GetInt("ExperimentalEnabled");
        if (experimentalValue == 0)
        {
            ExperimentalToggle.isOn = false;
        }
        else
        {
            ExperimentalToggle.isOn = true;
        }

        lastKnownMusicVolume = musicVolumeSlider.value;
        lastKnownGeneralVolume = generalVolumeSlider.value;

        musicVolumePercentage.text = Mathf.Floor(musicVolumeSlider.value * 100) + "%";
        generalVolumePercentage.text = Mathf.Floor(generalVolumeSlider.value * 100) + "%";
    }

    public void SaveOptions()
    {
        float valueMusic = musicVolumeSlider.value;
        float valueGeneral = generalVolumeSlider.value;

        if (ExperimentalToggle.isOn)
        {
            PlayerPrefs.SetInt("ExperimentalEnabled", 1);
        }
        else
        {
            PlayerPrefs.SetInt("ExperimentalEnabled", 0);
        }

        PlayerPrefs.SetFloat("GeneralVolume", valueGeneral);
        PlayerPrefs.SetFloat("MusicVolume", valueMusic);
        PlayerPrefs.Save();

        audioManager.UpdateAudioVolumes();
    }

    public void BackToMenu()
    {
        audioManager.UpdateAudioVolumes();

        optionsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void TestOutCurrentGeneralVolume()
    {
        audioManager.TestOutSFXAudio();
    }

    public IEnumerator ResetAllUI()
    {
        SetAllMainMenuUIImagesAlpha(1,0.1f);
        SetDeathPanelAlpha(0,1f);
        SetEndDemoPanelAlpha(0,1f);
        SetPlayerUI(true);
        SetDeathBringerUI(false);
        SetNecromancerUI(false);
        SetMenuUI(true);
        SetUIButtonStates(true);
        yield return new WaitForSeconds(1);
        deathPanel.SetActive(false);
        endDemoPanel.SetActive(false);
    }

    public void SetUIButtonStates(bool state)
    {
        foreach(Button but in mainMenuButtons)
        {
            but.interactable = state;
        }
    }

    public void SetTutorialText(bool state)
    {
        foreach (TextMeshProUGUI txt in tutorialText)
        {
            txt.enabled = state;
        }
    }

    public void SetAllMainMenuUIImagesAlpha(float targetAlpha, float time)
    {
        foreach(Image img in mainMenuUIImages)
        {
            StartCoroutine(FadeTo(img, targetAlpha, time));
        }
        foreach(TextMeshProUGUI txt in mainMenuUIText)
        {
            StartCoroutine(TextFadeTo(txt, targetAlpha, time));
        }
    }

    public void SetEndDemoPanelAlpha(float targetAlpha, float time)
    {
        endDemoPanel.SetActive(true);
        StartCoroutine(FadeTo(endDemoImage, targetAlpha, time));
        StartCoroutine(TextFadeTo(endDemoText, targetAlpha, time));
    }

    public void SetDeathPanelAlpha(float targetAlpha, float time)
    {
        deathPanel.SetActive(true);
        StartCoroutine(FadeTo(deathPanelImage, targetAlpha, time));
        StartCoroutine(TextFadeTo(deathPanelText, targetAlpha, time));
    }

    private IEnumerator FadeTo(Image img, float aValue, float aTime)
    {
        Color initialColor = img.color;
        Color targetColor = img.color;

        targetColor.a = aValue;

        float elapsedTime = 0;

        while (elapsedTime < aTime)
        {
            elapsedTime += Time.deltaTime;
            img.color = Color.Lerp(initialColor, targetColor, elapsedTime / aTime);
            yield return null;
        }
    }

    private IEnumerator TextFadeTo(TextMeshProUGUI txt, float aValue, float aTime)
    {
        Color initialColor = txt.color;
        Color targetColor = txt.color;

        targetColor.a = aValue;

        float elapsedTime = 0;

        while (elapsedTime < aTime)
        {
            elapsedTime += Time.deltaTime;
            txt.color = Color.Lerp(initialColor, targetColor, elapsedTime / aTime);
            yield return null;
        }
    }

    public void SetPlayerUI(bool state)
    {
        PlayerUI.SetActive(state);
    }

    public void SetDeathBringerUI(bool state)
    {
        for(int i=0; i< DeathBringerUI.transform.childCount; i++)
        {
            DeathBringerUI.transform.GetChild(i).gameObject.SetActive(state);
        }
    }
    public void SetNecromancerUI(bool state)
    {
        NecromancerUI.SetActive(state);
    }

    public void SetMenuUI(bool state)
    {
        PlayerUI.SetActive(state);
    }
}
