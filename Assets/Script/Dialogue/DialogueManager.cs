using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{




    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI JPdialogueText;
    public TextMeshProUGUI ENGdialogueText;
    public TextMeshProUGUI ThaidialogueText;
    public Image characterImage;
    public Image characterImageSmall;

    public Image _BGImage;
    public Image _StoryImage;


    public GameObject JPGameObject;
    public GameObject ENGGameObject;
    public GameObject ThaiGameObject;

    public Textbase textbase;


    public enum Language { ENG, THAI, JP }
    /// <summary> ?????? GlobalQuestState.CurrentLanguage (0=ENG, 1=THAI, 2=JP) </summary>
    public Language currentLanguage = Language.THAI;

    public int currentLineIndex = 0;


    public string SceneName;

    public System.Action onDialogueFinished;


    void Start()
    {
        speakerNameText = GameObject.Find("Name")?.GetComponent<TextMeshProUGUI>();

        JPdialogueText = GameObject.Find("JP")?.GetComponent<TextMeshProUGUI>();
        ENGdialogueText = GameObject.Find("ENG")?.GetComponent<TextMeshProUGUI>();
        ThaidialogueText = GameObject.Find("TH")?.GetComponent<TextMeshProUGUI>();

        characterImage = GameObject.Find("Cha")?.GetComponent<Image>();
        characterImageSmall = GameObject.Find("EmtyCurrantTurn")?.GetComponent<Image>();
        _BGImage = GameObject.Find("BG")?.GetComponent<Image>();
        _StoryImage = GameObject.Find("Storypic")?.GetComponent<Image>();

    }

    public void Update()
    {

        Language_setting();
        JPGameObject = JPdialogueText.gameObject;
        ThaiGameObject = ThaidialogueText.gameObject;
        ENGGameObject = ENGdialogueText.gameObject;
    }
    // 2. ùù??ùù? OnNextButtonPressed ù?ùùù

    public void OnNextButtonPressed()
    {
        currentLineIndex++;

        if (currentLineIndex < textbase.TextData.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            // --- ùù?Áùùù ---
            // ùùùùùùùùùù??ù??ùùùùù (ùù ù?ù StoryFlowController)
            if (onDialogueFinished != null)
            {
                // ùùùù?ùùùùùùùù "ùùùùùù!"
                onDialogueFinished.Invoke();

                // ùù?ùù?ùù ùùùùùùùùùù?ùùùùùùùù?ùù?ùùùùùù
                onDialogueFinished = null;
                Debug.Log("chane of Dialogu");

            }
            else
            {
                // ùùùùùùùùù‰ùù (ùùù?ùùùù) ùùùùùùù? Scene ùùù?ùùùù?ùù?ùùùùù
                LoadScene();
                Debug.Log("End of Dialogue (Default behavior)");
            }
            // ------------------
        }
    }

   public void ShowCurrentLine()
    {
        ENGdialogueText.text = "";
        ThaidialogueText.text = "";
        JPdialogueText.text = "";
        StopAllCoroutines();
        StartCoroutine(TypeLine());

        var line = textbase.TextData[currentLineIndex];


        speakerNameText.text = line.speakerName;

        characterImage.sprite = line.speakerImage;
        characterImageSmall.sprite = line.SmallImage;

        _BGImage.sprite = line.BGImage;
        _StoryImage.sprite = line.StoryImage;




    }
    IEnumerator TypeLine()
    {
        string currentSentence = "";
        TextMeshProUGUI activeText = null;

        if (currentLanguage == Language.ENG) { currentSentence = textbase.TextData[currentLineIndex].ENGsentence; activeText = ENGdialogueText; }
        else if (currentLanguage == Language.THAI) { currentSentence = textbase.TextData[currentLineIndex].Thaisentence; activeText = ThaidialogueText; }
        else if (currentLanguage == Language.JP) { currentSentence = textbase.TextData[currentLineIndex].Jpsentence; activeText = JPdialogueText; }

        // Type each character 1 by 1
        foreach (char c in currentSentence.ToCharArray())
        {
            activeText.text += c;
            yield return new WaitForSeconds(0.01f);
        }
    }
    public void LoadScene()
    {
        SceneManager.LoadScene(SceneName);

    }

    /// <summary> ??????????? GlobalQuestState ????????? GameObject + ????? </summary>
    public void Language_setting()
    {
        int lang = GlobalQuestState.CurrentLanguage;
        if (lang == 0)
        {
            currentLanguage = Language.ENG;
            if (ThaiGameObject != null) ThaiGameObject.SetActive(false);
            if (ENGGameObject != null) ENGGameObject.SetActive(true);
            if (JPGameObject != null) JPGameObject.SetActive(false);
            GlobalQuestState.ApplyLanguageFont(ENGdialogueText);
        }
        else if (lang == 1)
        {
            currentLanguage = Language.THAI;
            if (ThaiGameObject != null) ThaiGameObject.SetActive(true);
            if (ENGGameObject != null) ENGGameObject.SetActive(false);
            if (JPGameObject != null) JPGameObject.SetActive(false);
            GlobalQuestState.ApplyLanguageFont(ThaidialogueText);
        }
        else if (lang == 2)
        {
            currentLanguage = Language.JP;
            if (ThaiGameObject != null) ThaiGameObject.SetActive(false);
            if (ENGGameObject != null) ENGGameObject.SetActive(false);
            if (JPGameObject != null) JPGameObject.SetActive(true);
            GlobalQuestState.ApplyLanguageFont(JPdialogueText);
        }
        GlobalQuestState.ApplyLanguageFont(speakerNameText);
    }

}