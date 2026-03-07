using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    private List<QuestData> _injectedQuestList; // รายการเควสที่ส่งมาจาก QuestManager (ตามบทใน GlobalQuestState)
    private int currentQuestIndex = 0;
    private int currentStepIndex = 0;
    private QuestData currentQuest;
    private List<StoryStep> _activeStoryFlow; // บทสนทนาตามตัวละครที่เลือก (จาก QuestData.characterStoryFlows)

    void Start()
    {
        // ถ้ามี activeQuest ใน GlobalQuestState ให้เริ่มเล่นบทสนทนาจากนั้นเลย (ไม่ต้องรอ QuestManager)
        if (GlobalQuestState.ActiveQuest != null)
            StartFromGlobalState();
    }

    /// <summary> ดึงเควสที่ต้องพูดมาจาก GlobalQuestState.ActiveQuest + SelectedCharacter แล้วเริ่มเล่น — ใช้ได้โดยไม่ต้องมี QuestManager ในซีน </summary>
    public void StartFromGlobalState()
    {
        QuestData quest = GlobalQuestState.ActiveQuest;
        CharacterData character = GlobalQuestState.SelectedCharacter;
        if (quest == null)
        {
            Debug.LogWarning("⚠️ [StoryFlow] GlobalQuestState.ActiveQuest เป็น null — ไม่มีเควสที่จะเล่น");
            return;
        }
        participatingCharacter = character;
        currentQuest = quest;
        currentStepIndex = 0;
        _activeStoryFlow = currentQuest.GetStoryFlowForCharacter(participatingCharacter);
        if (_activeStoryFlow == null || _activeStoryFlow.Count == 0)
        {
            Debug.LogWarning($"⚠️ [StoryFlow] ไม่มี storyFlow สำหรับตัวละคร {participatingCharacter?.characterName} ในเควส {currentQuest.questName}");
            NotifyQuestCompleted();
            return;
        }
        Debug.Log($"⚔️ [StoryFlow] เริ่มภารกิจจาก GlobalQuestState: {currentQuest.questName}");
        ProcessCurrentStep();
    }

    /// <summary> เมื่อจบเควส — ลบเควสนั้นออกจาก list แล้วโหลดซีนหน้าเควสหลัก (หรือ refresh ถ้าอยู่ซีนเดียวกับ QuestManager) </summary>
    void NotifyQuestCompleted()
    {
        GlobalQuestState.RemoveQuestCompleted(GlobalQuestState.CurrentQuestIndex);

        var qm = FindAnyObjectByType<QuestManager>();
        if (qm != null)
            qm.OnQuestCompleted();
        else if (!string.IsNullOrEmpty(GlobalQuestState.MainQuestSceneName))
            SceneManager.LoadScene(GlobalQuestState.MainQuestSceneName);
    }

    /// <summary> ใส่ defaultQuestListForChapter = รายการเควสของบทปัจจุบัน (จาก StoryProgressData + GlobalQuestState) — ใช้เมื่อมี QuestManager ในซีน </summary>
    public void SetCharacter(CharacterData newChar, List<QuestData> defaultQuestListForChapter = null)
    {
        _injectedQuestList = defaultQuestListForChapter;
        participatingCharacter = newChar;
        PrepareQuestRoute();
    }

    // 🌟 ระบบลำดับความสำคัญ: หาว่าต้องใช้คัมภีร์ชุดไหน 🌟
    private void PrepareQuestRoute()
    {
        bool isSecretRouteFound = false;

        // 1. ค้นหาเส้นทางลับก่อนเสมอ
        if (participatingCharacter != null && characterRoutes != null && characterRoutes.Count > 0)
        {
            foreach (var route in characterRoutes)
            {
                if (route.targetCharacter == participatingCharacter && route.exclusiveQuestList != null && route.exclusiveQuestList.Count > 0)
                {
                    activeQuestList = new List<QuestData>(route.exclusiveQuestList);
                    isSecretRouteFound = true;
                    Debug.Log($"✨ [StoryFlow] ประตูมิติเปิดออก! โหลดเส้นทางลับของ: {participatingCharacter.characterName}");
                    break;
                }
            }
        }

        // 2. ถ้าไม่มีเส้นทางลับ ใช้รายการที่ส่งมา (บทปัจจุบัน) หรือ defaultQuestList
        if (!isSecretRouteFound)
        {
            if (_injectedQuestList != null && _injectedQuestList.Count > 0)
            {
                activeQuestList = new List<QuestData>(_injectedQuestList);
                Debug.Log("🚶 [StoryFlow] โหลดเส้นทางตามบทใน GlobalQuestState");
            }
            else
            {
                activeQuestList = defaultQuestList != null ? new List<QuestData>(defaultQuestList) : new List<QuestData>();
                Debug.Log("🚶 [StoryFlow] โหลดเส้นทางปกติ (Default Quest List)");
            }
        }
    }

    // QuestManager จะเรียกฟังก์ชันนี้ — ดึงเควสปัจจุบันที่เลือกจาก GlobalQuestState ไปเล่น
    public void StartQuest(int questIndex)
    {
        QuestData questToPlay = GlobalQuestState.GetCurrentSelectedQuest();
        if (questToPlay == null && activeQuestList != null && questIndex >= 0 && questIndex < activeQuestList.Count)
            questToPlay = activeQuestList[questIndex];

        if (questToPlay == null)
        {
            Debug.LogError("⚠️ [StoryFlow] ไม่มีเควสที่เลือก! ดึงจาก GlobalQuestState หรือ activeQuestList ไม่ได้");
            return;
        }

        if (activeQuestList != null && questIndex >= 0 && questIndex < activeQuestList.Count)
            currentQuestIndex = questIndex;
        currentQuest = questToPlay;
        currentStepIndex = 0;
        _activeStoryFlow = currentQuest.GetStoryFlowForCharacter(participatingCharacter);

        Debug.Log($"⚔️ [StoryFlow] เริ่มภารกิจ: {currentQuest.questName}");
        ProcessCurrentStep();
    }

    void ProcessCurrentStep()
    {
        if (_activeStoryFlow == null || _activeStoryFlow.Count == 0)
        {
            Debug.LogWarning($"⚠️ [StoryFlow] ไม่มี storyFlow สำหรับตัวละคร {participatingCharacter?.characterName} ในเควส {currentQuest?.questName} — ข้ามจบเควส");
            NotifyQuestCompleted();
            return;
        }
        if (currentStepIndex >= _activeStoryFlow.Count)
        {
            Debug.Log($"✅ [StoryFlow] ภารกิจ {currentQuest.questName} เสร็จสิ้นลงแล้ว!");
            NotifyQuestCompleted();
            return;
        }

        StoryStep currentStep = _activeStoryFlow[currentStepIndex];

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

            if (txt != null) { txt.text = option.buttonText; GlobalQuestState.ApplyLanguageFont(txt); }

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
                if (txt != null) { txt.text += $" <size=80%><color=yellow>{reason}</color></size>"; GlobalQuestState.ApplyLanguageFont(txt); }
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
        if (_injectedQuestList != null && index >= 0 && index < _injectedQuestList.Count)
            return _injectedQuestList[index];
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