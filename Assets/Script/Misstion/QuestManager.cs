using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement; // ⚠️ ต้องมีบรรทัดนี้เพื่อเรียกใช้วิชาเปิดมิติ
public class QuestManager : MonoBehaviour
{
    [Header("--- 🔮 จุดเชื่อมต่อเวทมนตร์ ---")]
    public StoryFlowController storyFlowController;
    [Tooltip("ใช้เมื่อเควสเป็นประเภท Battle (ต่อสู้)")]
    public BattleQuestController battleQuestController;

    [Header("--- 📜 ความคืบหน้าเนื้อเรื่อง (เลข 1 = บทที่ 1) ---")]
    [Tooltip("ใส่ StoryProgressData ที่เก็บแบบ CharacterQuestRoute แล้วใช้เลขใน GlobalQuestState เลือกบท")]
    public StoryProgressData storyProgressData;

    [Header("--- 📜 สถานะภารกิจ (เมื่อไม่ใช้ storyProgressData) ---")]
    public int currentGlobalQuestIndex = 0;
    public TextMeshProUGUI questDescriptionText;

    [Header("--- 🎭 ระบบคัดเลือกตัวละคร ---")]
    public GameObject characterSelectionPanel; // หน้าต่างหรือ Panel ที่ครอบปุ่มเลือกตัวละครไว้
    public List<CharacterData> availableCharacters; // รายชื่อตัวละครที่มีในค่าย
    public Transform characterButtonContainer; // จุดที่ปุ่มจะไปเกิด (ใส่ Vertical/Horizontal Layout ไว้)
    public GameObject characterButtonPrefab; // Prefab ของปุ่ม (ต้องมี Image เพื่อใส่รูปหน้า)

    [Header("--- ✨ อนิเมชันปุ่มตัวละคร ---")]
    public QuestButtonAnimation buttonAnimation; // สคริปต์จัดการอนิเมชัน
    [Tooltip("ขนาดปุ่ม (ถ้าเป็น 0 จะใช้ขนาดจากรูปภาพ)")]
    public float buttonSize = 200f;

    void Start()
    {
        GlobalQuestState.LoadState(); // ดึงที่เซฟไว้มาบอกว่าตอนนี้อยู่บทไหน เควสที่เท่าไหร่
        OpenQuestBoard();
    }

    bool UseStoryProgress() => storyProgressData != null && storyProgressData.routes != null && storyProgressData.routes.Count > 0;

    /// <summary> ดึงรายการเควสของบทปัจจุบัน (บทที่ 1 = routes[0], ...) </summary>
    List<QuestData> GetCurrentChapterQuestList()
    {
        if (!UseStoryProgress()) return null;
        int ch = GlobalQuestState.CurrentChapter - 1;
        if (ch < 0 || ch >= storyProgressData.routes.Count) return null;
        var route = storyProgressData.routes[ch];
        if (route?.exclusiveQuestList == null) return null;
        return new List<QuestData>(route.exclusiveQuestList);
    }

    /// <summary> ดึงรายการเควสของตัวละครที่เลือก (จาก StoryProgressData.routes ที่ targetCharacter ตรง) </summary>
    List<QuestData> GetQuestListForCharacter(CharacterData character)
    {
        if (!UseStoryProgress() || character == null) return null;
        foreach (var route in storyProgressData.routes)
        {
            if (route != null && route.targetCharacter == character && route.exclusiveQuestList != null)
                return new List<QuestData>(route.exclusiveQuestList);
        }
        return null;
    }

    QuestData GetQuestAtCurrentPosition()
    {
        var list = GetCurrentChapterQuestList();
        int idx = GlobalQuestState.CurrentQuestIndex;
        if (list == null || idx < 0 || idx >= list.Count) return null;
        return list[idx];
    }

    int GetCurrentQuestIndex()
    {
        if (UseStoryProgress()) return GlobalQuestState.CurrentQuestIndex;
        return currentGlobalQuestIndex;
    }

    public void OpenQuestBoard()
    {
        characterSelectionPanel.SetActive(true);

        QuestData previewQuest = UseStoryProgress() ? GetQuestAtCurrentPosition() : storyFlowController.GetDefaultQuest(currentGlobalQuestIndex);
        if (previewQuest != null)
            questDescriptionText.text = previewQuest.questDescription;
        GlobalQuestState.ApplyLanguageFont(questDescriptionText);
        print("GenerateCharacterButtons");
        GenerateCharacterButtons();
    }

    void GenerateCharacterButtons()
    {
        // เติมบรรทัดนี้ลงไปเพื่อเช็คว่าระบบเห็นทหารกี่นาย!
        Debug.Log($"[QuestManager] กำลังสร้างปุ่ม... พบรายชื่อทหารทั้งหมด: {availableCharacters.Count} นาย");

        foreach (Transform child in characterButtonContainer) Destroy(child.gameObject);

        int buttonIndex = 0;
        foreach (var character in availableCharacters)
        {
            GameObject btnObj = Instantiate(characterButtonPrefab, characterButtonContainer);
            Button btn = btnObj.GetComponent<Button>();
            RectTransform rectTransform = btnObj.GetComponent<RectTransform>();

            // 🔲 ตั้งขนาดปุ่มให้เป็นสัดส่วน (ไม่ให้ยืด)
            float targetSize = buttonSize;
            if (targetSize <= 0 && character.portrait != null)
            {
                // ถ้าไม่ได้ตั้งขนาด ให้ใช้ขนาดจากรูปภาพ
                targetSize = Mathf.Max(character.portrait.rect.width, character.portrait.rect.height);
            }
            if (targetSize > 0)
            {
                rectTransform.sizeDelta = new Vector2(targetSize, targetSize);
                
                // เพิ่ม LayoutElement เพื่อป้องกันการยืดจาก Layout Group
                UnityEngine.UI.LayoutElement layoutElement = btnObj.GetComponent<UnityEngine.UI.LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = btnObj.AddComponent<UnityEngine.UI.LayoutElement>();
                }
                layoutElement.preferredWidth = targetSize;
                layoutElement.preferredHeight = targetSize;
                layoutElement.flexibleWidth = 0;
                layoutElement.flexibleHeight = 0;
            }

            // 🖼️ เอารูป Portrait จาก CharacterData มาใส่ในปุ่ม (ไม่ให้ยืด)
            Image btnImage = btnObj.GetComponent<Image>();
            if (character.portrait != null && btnImage != null)
            {
                btnImage.sprite = character.portrait;
                btnImage.preserveAspect = true; // รักษาอัตราส่วน ไม่ให้รูปยืด
            }
            // ถ้ารูปอยู่ที่ child Image (เช่นใต้ปุ่ม) ให้ preserve aspect ด้วย
            Image childImage = btnObj.GetComponentInChildren<Image>(true);
            if (childImage != null && childImage != btnImage)
            {
                childImage.preserveAspect = true;
            }

            // (Optional) ถ้าปุ่มมี Text ก็ใส่ชื่อกำกับไว้ด้วย
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.text = character.characterName; GlobalQuestState.ApplyLanguageFont(txt); }

            // ฝังคำสั่งเมื่อนายท่านกดปุ่มตัวละครนี้!
            btn.onClick.AddListener(() =>
            {
                if (buttonAnimation != null)
                {
                    buttonAnimation.PlayCharacterSendAnimation(btnObj, character, OnCharacterSelected, characterSelectionPanel);
                }
                else
                {
                    // ถ้าไม่มี Animation component ให้เรียก OnCharacterSelected โดยตรง
                    OnCharacterSelected(character);
                }
            });

            // ✨ เริ่มอนิเมชันเมื่อปุ่มปรากฏ (แต่ละปุ่มมี delay เล็กน้อยเพื่อให้ดูสวยงาม)
            if (buttonAnimation != null)
            {
                buttonAnimation.PlayButtonAppearAnimation(btnObj, buttonIndex * 0.05f);
            }
            buttonIndex++;
        }
    }

    // 🌟 เมื่อตัดสินใจเลือกหมากตัวใดตัวหนึ่ง 🌟
    // 🌟 เมื่อตัดสินใจเลือกหมากตัวใดตัวหนึ่ง 🌟
    void OnCharacterSelected(CharacterData selectedChar)
    {
        Debug.Log($"[QuestManager] ตัดสินใจส่ง: {selectedChar.characterName} เข้าสู่สนามรบ!");

        // ส่งรายการเควสของตัวละครที่เลือก (จาก StoryProgressData) เพื่อให้ข้อความเควสเปลี่ยนตามตัวละคร
        var questList = UseStoryProgress() ? GetQuestListForCharacter(selectedChar) : null;
        if (questList == null && UseStoryProgress())
            questList = GetCurrentChapterQuestList(); // ถ้าไม่มี route ตรงตัวละคร ใช้บทปัจจุบัน
        storyFlowController.SetCharacter(selectedChar, questList);

        int index = GetCurrentQuestIndex();
        QuestData activeQuest = storyFlowController.GetActiveQuest(index);

        // อัปเดตข้อความเควสจากรายการที่ส่งไป (หรือจาก activeQuest) ให้เปลี่ยนเมื่อเลือกตัวละคร
        if (questDescriptionText != null)
        {
            if (questList != null && index >= 0 && index < questList.Count)
                questDescriptionText.text = questList[index].questDescription;
            else if (activeQuest != null)
                questDescriptionText.text = activeQuest.questDescription;
            GlobalQuestState.ApplyLanguageFont(questDescriptionText);
        }

        // 5. เลือกประเภทเควส: เรื่องราว หรือ ต่อสู้ (ฉบับข้ามมิติ!)
        if (activeQuest != null && activeQuest.questType == QuestType.Battle)
        {
            // 🕊️ ฝากความทรงจำไว้ที่ผู้ส่งสาร ก่อนมิติจะพังทลาย!
            GlobalQuestState.ActiveQuest = activeQuest;
            GlobalQuestState.SelectedCharacter = selectedChar;

            // 🌌 ร่ายเวทเปิดประตูมิติ! (ดึงชื่อ Scene จากคัมภีร์ QuestData)
            string sceneName = activeQuest.battleConfig.battleSceneName;

            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"[QuestManager] 🌌 กำลังเปิดประตูมิติ ส่งกองทัพไปยังสมรภูมิ: {sceneName}");
                SceneManager.LoadScene(sceneName); // โหลดฉากใหม่ทันที!
            }
            else
            {
                Debug.LogError("❌ [QuestManager] นายท่านลืมใส่ชื่อ Scene ใน Battle Config หรือเปล่าขอรับ!?");
            }
        }
        else
        {
            storyFlowController.StartQuest(index);
        }
    }

    public void OnQuestCompleted()
    {
        if (UseStoryProgress())
        {
            var list = GetCurrentChapterQuestList();
            if (list != null && GlobalQuestState.CurrentQuestIndex < list.Count - 1)
            {
                GlobalQuestState.CurrentQuestIndex++;
            }
            else
            {
                GlobalQuestState.CurrentChapter++;
                GlobalQuestState.CurrentQuestIndex = 0;
                int maxCh = storyProgressData.routes != null ? storyProgressData.routes.Count : 1;
                GlobalQuestState.CurrentChapter = Mathf.Clamp(GlobalQuestState.CurrentChapter, 1, maxCh);
            }
            GlobalQuestState.SaveState();
        }
        else
        {
            currentGlobalQuestIndex++;
        }
        OpenQuestBoard();
    }
}