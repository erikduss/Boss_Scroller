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

    public void TransitionToGame()
    {
        foreach(Button but in mainMenuButtons)
        {
            but.interactable = false;
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
