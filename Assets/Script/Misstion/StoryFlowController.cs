using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StoryFlowController : MonoBehaviour
{
    [Header("--- 🎭 ตัวละครที่กำลังทำภารกิจ ---")]
    public CharacterData participatingCharacter;

    [Header("--- 📜 คัมภีร์ภารกิจ (Quest List) ---")]
    public List<QuestData> questList; // ใส่เควสที่ต้องการให้รันเป็นคิวที่นี่
    public int startQuestIndex = 0;   // เลือกว่าจะเริ่มรันจากคิวที่เท่าไหร่ (ค่าเริ่มต้นคือ 0 = อันแรกสุด)

    [Header("--- References ---")]
    public DialogueManager dialogueManager;
    public GameObject choicePanel;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    // ตัวแปรติดตามสถานะปัจจุบัน
    private int currentQuestIndex = 0;
    private int currentStepIndex = 0;
    private QuestData currentQuest;

    void Start()
    {
        if (participatingCharacter == null)
            Debug.LogWarning("⚠️ ยังไม่มีตัวละครทำภารกิจ!");

        if (questList.Count == 0)
        {
            Debug.LogError("⚠️ คัมภีร์ภารกิจว่างเปล่า! กรุณาใส่ QuestData ลงใน Quest List อย่างน้อย 1 อัน");
            return;
        }

        // เริ่มต้นเควสตาม Index ที่นายท่านกำหนด
        StartQuest(startQuestIndex);
    }

    // ฟังก์ชันสั่งเริ่มเควส
    public void StartQuest(int questIndex)
    {
        if (questIndex >= questList.Count)
        {
            Debug.Log("🎉 จบทุกภารกิจในคลังคัมภีร์แล้ว!");
            return;
        }

        currentQuestIndex = questIndex;
        currentQuest = questList[currentQuestIndex];
        currentStepIndex = 0; // รีเซ็ตลำดับเหตุการณ์ให้เริ่มจาก 0 ใหม่

        Debug.Log($"⚔️ เริ่มภารกิจ: {currentQuest.questName}");
        ProcessCurrentStep();
    }

    void ProcessCurrentStep()
    {
        // 1. เช็คว่าจบ "เควสปัจจุบัน" หรือยัง?
        if (currentStepIndex >= currentQuest.storyFlow.Count)
        {
            Debug.Log($"✅ จบภารกิจ: {currentQuest.questName}");
            // ขยับไปทำเควสลำดับถัดไปใน List ทันที
            StartQuest(currentQuestIndex + 1);
            return;
        }

        StoryStep currentStep = currentQuest.storyFlow[currentStepIndex];

        // 2. ดำเนินการตามตัวเลือก
        if (currentStep.options.Count == 0)
        {
            GoToNextStep();
        }
        else if (currentStep.options.Count == 1)
        {
            choicePanel.SetActive(false);
            GiveReward(currentStep.options[0].rewardGold);
            PlaySpecificTextbase(currentStep.options[0].targetTextbase);
        }
        else
        {
            ShowChoiceButtons(currentStep.options);
        }
    }

    void ShowChoiceButtons(List<StoryOption> options)
    {
        choicePanel.SetActive(true);
        foreach (Transform child in buttonContainer) Destroy(child.gameObject);

        foreach (var option in options)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            Image btnImage = btnObj.GetComponent<Image>();

            if (txt != null) txt.text = option.buttonText;

            // --- ตรวจสอบเงื่อนไขตัวละคร ---
            bool passCondition = true;
            string reason = "";

            if (participatingCharacter != null)
            {
                if (participatingCharacter.HP < option.reqHP) { passCondition = false; reason += "(HP ไม่พอ) "; }
                if (participatingCharacter.VIT < option.reqVIT) { passCondition = false; reason += "(VIT ไม่พอ) "; }
                if (participatingCharacter.STR < option.reqSTR) { passCondition = false; reason += "(STR ไม่พอ) "; }
                if (participatingCharacter.DEX < option.reqDEX) { passCondition = false; reason += "(DEX ไม่พอ) "; }
                if (participatingCharacter.INT < option.reqINT) { passCondition = false; reason += "(INT ไม่พอ) "; }
                if (participatingCharacter.CHA < option.reqCHA) { passCondition = false; reason += "(CHA ไม่พอ) "; }
                if (participatingCharacter.LUCK < option.reqLUCK) { passCondition = false; reason += "(LUCK ไม่พอ) "; }

                if (!string.IsNullOrEmpty(option.skillNeed) && !participatingCharacter.HasSkill(option.skillNeed))
                {
                    passCondition = false;
                    reason += $"(ขาดสกิล {option.skillNeed})";
                }
            }
            else
            {
                passCondition = false;
                reason = "(ไม่พบตัวละคร)";
            }

            // --- แสดงปุ่ม ---
            if (passCondition)
            {
                btn.interactable = true;
                btnImage.color = Color.white;
                btn.onClick.AddListener(() =>
                {
                    choicePanel.SetActive(false);
                    GiveReward(option.rewardGold);
                    PlaySpecificTextbase(option.targetTextbase);
                });
            }
            else
            {
                btn.interactable = false;
                btnImage.color = Color.red;
                if (txt != null) txt.text += $" <size=80%><color=yellow>{reason}</color></size>";
            }
        }
    }

    void GiveReward(int amount)
    {
        if (amount > 0 && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddGold(amount);
        }
    }

    void PlaySpecificTextbase(Textbase textbaseToPlay)
    {
        if (textbaseToPlay == null) return;
        dialogueManager.textbase = textbaseToPlay;
        dialogueManager.currentLineIndex = 0;
        dialogueManager.onDialogueFinished = GoToNextStep; // บอกว่าถ้าจบ Text ให้เรียกก้าวต่อไป
        dialogueManager.ShowCurrentLine();
    }

    // ก้าวไปยังเหตุการณ์ต่อไป "ในเควสปัจจุบัน"
    void GoToNextStep()
    {
        currentStepIndex++;
        ProcessCurrentStep();
    }
}