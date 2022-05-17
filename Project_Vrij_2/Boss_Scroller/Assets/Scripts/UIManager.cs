using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerUI;
    [SerializeField] private GameObject BossUI;

    [SerializeField] private List<Button> mainMenuButtons;
    [SerializeField] private List<Image> mainMenuUIImages;
    [SerializeField] private List<TextMeshProUGUI> mainMenuUIText;

    [SerializeField] private GameObject menuUIPanel;

    [SerializeField] private GameObject deathPanel;
    private Image deathPanelImage;
    [SerializeField] private TextMeshProUGUI deathPanelText;

    [SerializeField] private List<TextMeshProUGUI> tutorialText;

    // Start is called before the first frame update
    void Start()
    {
        PlayerUI.SetActive(false);
        BossUI.SetActive(false);
        deathPanelImage = deathPanel.GetComponent<Image>();

        SetTutorialText(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ResetAllUI()
    {
        SetAllMainMenuUIImagesAlpha(1,0.1f);
        SetDeathPanelAlpha(0,1f);
        SetPlayerUI(true);
        SetBossUI(false);
        SetMenuUI(true);
        SetUIButtonStates(true);
        yield return new WaitForSeconds(1);
        deathPanel.SetActive(false);
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

    public void SetBossUI(bool state)
    {
        BossUI.SetActive(state);
    }
    public void SetMenuUI(bool state)
    {
        PlayerUI.SetActive(state);
    }
}
