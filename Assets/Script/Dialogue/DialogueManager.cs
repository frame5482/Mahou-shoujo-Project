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


    private static readonly string SetLanguage = "PlayerPrefsSetLanguage";
    public int SetintLanguage;
    public enum Language { ENG, THAI, JP }
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
    // 2. แก้ไขฟังก์ชัน OnNextButtonPressed ดังนี้

    public void OnNextButtonPressed()
    {
        currentLineIndex++;

        if (currentLineIndex < textbase.TextData.Count)
        {
            ShowCurrentLine();
        }
        else
        {
            // --- แก้ไขตรงนี้ ---
            // เช็คว่ามีใครรอฟังผลตอนจบไหม (เช่น ระบบ StoryFlowController)
            if (onDialogueFinished != null)
            {
                // แจ้งเตือนคนคุมว่า "จบแล้ว!"
                onDialogueFinished.Invoke();

                // ล้างค่าทิ้ง เพื่อไม่ให้จำค่าเดิมไปใช้กับบทสนทนาอื่น
                onDialogueFinished = null;
                Debug.Log("chane of Dialogu");

            }
            else
            {
                // ถ้าไม่มีใครคุม (เล่นแบบปกติ) ก็ให้โหลด Scene หรือทำอย่างอื่นตามเดิม
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

    public void Language_setting()
    {
        SetintLanguage = PlayerPrefs.GetInt(SetLanguage);
        if (SetintLanguage == 0)
        {
            currentLanguage = Language.ENG;
            ThaiGameObject.SetActive(false);
            ENGGameObject.SetActive(true);
            JPGameObject.SetActive(false);

        }
        else if (SetintLanguage == 1)
        {
            currentLanguage = Language.THAI;
            ThaiGameObject.SetActive(true);
            ENGGameObject.SetActive(false);
            JPGameObject.SetActive(false);

        }
        if (SetintLanguage == 2)
        {
            currentLanguage = Language.JP;
            ThaiGameObject.SetActive(false);
            ENGGameObject.SetActive(false);
            JPGameObject.SetActive(true);
        }

    }

}