using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using TMPro;

public class StoryFlowController : MonoBehaviour
{
    [Header("--- 🎭 ตัวละครที่กำลังทำภารกิจ ---")]
    public CharacterData participatingCharacter;

    [System.Serializable]
    public class CharacterQuestRoute
    {
        public CharacterData targetCharacter; // ใช้ Object ตรวจสอบ ป้องกันการพิมพ์ผิด 100%
        public List<QuestData> exclusiveQuestList;
    }

    [Header("--- 🔀 1. เส้นทางลับเฉพาะตัวละคร (Priority 1) ---")]
    public List<CharacterQuestRoute> characterRoutes;

    [Header("--- 📜 2. เส้นทางปกติ (Priority 2) ---")]
    public List<QuestData> defaultQuestList;

    [Header("--- References ---")]
    public DialogueManager dialogueManager;
    public GameObject choicePanel;
    public Transform buttonContainer;
    public GameObject buttonPrefab;

    // ตัวแปรติดตามสถานะปัจจุบัน
    private List<QuestData> activeQuestList;
    private int currentQuestIndex = 0;
    private int currentStepIndex = 0;
    private QuestData currentQuest;

    void Start()
    {
        // 🛑 ปลดชนวน! ไม่ให้มันเริ่มทำงานเองใน Start อีกต่อไป
        // มันจะหลับใหลจนกว่า QuestManager จะเป็นคนปลุกขึ้นมาสั่งการ!
    }

    // QuestManager จะเรียกฟังก์ชันนี้เมื่อผู้เล่นจิ้มเลือกตัวละคร
    public void SetCharacter(CharacterData newChar)
    {
        participatingCharacter = newChar;
        PrepareQuestRoute(); // คำนวณหาเส้นทางเควสทันทีที่ได้ตัวละครมา
    }

    // 🌟 ระบบลำดับความสำคัญ: หาว่าต้องใช้คัมภีร์ชุดไหน 🌟
    private void PrepareQuestRoute()
    {
        bool isSecretRouteFound = false;

        // 1. ค้นหาเส้นทางลับก่อนเสมอ
        if (participatingCharacter != null && characterRoutes.Count > 0)
        {
            foreach (var route in characterRoutes)
            {
                if (route.targetCharacter == participatingCharacter)
                {
                    activeQuestList = route.exclusiveQuestList;
                    isSecretRouteFound = true;
                    Debug.Log($"✨ [StoryFlow] ประตูมิติเปิดออก! โหลดเส้นทางลับของ: {participatingCharacter.characterName}");
                    break;
                }
            }
        }

        // 2. ถ้าไม่มีเส้นทางลับ ให้เดินตามเส้นทางปกติของสามัญชน
        if (!isSecretRouteFound)
        {
            activeQuestList = defaultQuestList;
            Debug.Log("🚶 [StoryFlow] ไม่พบเงื่อนไขตัวละครลับ โหลดเส้นทางปกติ (Default Route)");
        }
    }

    // QuestManager จะเรียกฟังก์ชันนี้เพื่อเข้าสู่สนามรบ
    public void StartQuest(int questIndex)
    {
        if (activeQuestList == null || activeQuestList.Count == 0)
        {
            Debug.LogError("⚠️ [StoryFlow] คัมภีร์ว่างเปล่า! ท่านลืมใส่ QuestData ใน Inspector หรือเปล่า?");
            return;
        }

        if (questIndex >= activeQuestList.Count)
        {
            Debug.Log("🎉 [StoryFlow] จบทุกภารกิจในคลังคัมภีร์แล้ว!");
            return;
        }

        currentQuestIndex = questIndex;
        currentQuest = activeQuestList[currentQuestIndex];
        currentStepIndex = 0;

        Debug.Log($"⚔️ [StoryFlow] เริ่มภารกิจ: {currentQuest.questName}");
        ProcessCurrentStep();
    }

    void ProcessCurrentStep()
    {
        // เช็คว่าจบทุกเหตุการณ์ (Step) ในเควสปัจจุบันหรือยัง?
        if (currentStepIndex >= currentQuest.storyFlow.Count)
        {
            Debug.Log($"✅ [StoryFlow] ภารกิจ {currentQuest.questName} เสร็จสิ้นลงแล้ว!");

            // 🔄 แจ้ง QuestManager ว่าจบแล้ว เพื่อให้เปิดหน้าต่างเลือกตัวละครสำหรับเควสต่อไป
            QuestManager questManager = FindAnyObjectByType<QuestManager>();

            if (questManager != null)
            {
                questManager.OnQuestCompleted();
            }
            return;
        }

        StoryStep currentStep = currentQuest.storyFlow[currentStepIndex];

        if (currentStep.options.Count == 0)
        {
            GoToNextStep();
        }
        else if (currentStep.options.Count == 1)
        {
            // ถ้ามี 1 ทางเลือก = Auto Play
            choicePanel.SetActive(false);
            GiveReward(currentStep.options[0].rewardGold);
            PlaySpecificTextbase(currentStep.options[0].targetTextbase);
        }
        else
        {
            // ถ้ามีหลายทางเลือก = โชว์ปุ่ม
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

            bool passCondition = true;
            string reason = "";

            // ตรวจสอบพลังและทักษะ
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

            // จัดการปุ่มตามเงื่อนไขที่เช็คมา
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
        if (textbaseToPlay == null)
        {
            GoToNextStep(); // ถ้าลืมใส่คัมภีร์ให้ข้ามไปเลย จะได้ไม่ค้าง
            return;
        }
        dialogueManager.textbase = textbaseToPlay;
        dialogueManager.currentLineIndex = 0;
        dialogueManager.onDialogueFinished = GoToNextStep; // สั่งว่าคุยจบให้เดินก้าวต่อไป
        dialogueManager.ShowCurrentLine();
    }

    void GoToNextStep()
    {
        currentStepIndex++;
        ProcessCurrentStep();
    }

    // -----------------------------------------------------------------
    // 🔮 ฟังก์ชันช่วยเหลือ ให้ QuestManager มาขอดึงข้อมูลไปโชว์
    // -----------------------------------------------------------------

    public QuestData GetDefaultQuest(int index)
    {
        if (defaultQuestList != null && index < defaultQuestList.Count)
            return defaultQuestList[index];
        return null;
    }

    public QuestData GetActiveQuest(int index)
    {
        if (activeQuestList != null && index < activeQuestList.Count)
            return activeQuestList[index];
        return null;
    }

    // ========== สำหรับ GameSaveManager: อ่าน/เขียนสถานะความคืบหน้าเนื้อเรื่อง ==========
    public string GetParticipatingCharacterName()
    {
        return participatingCharacter != null ? participatingCharacter.characterName : "";
    }

    public int GetCurrentQuestIndex() => currentQuestIndex;
    public int GetCurrentStepIndex() => currentStepIndex;
}