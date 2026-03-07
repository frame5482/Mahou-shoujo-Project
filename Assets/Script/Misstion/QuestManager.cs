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

    [Header("--- 📜 Questpaper (แผ่นเควส) ---")]
    [Tooltip("Prefab ของแผ่นเควส ต้องมี QuestPaperItem (รูป ชื่อ รายละเอียด)")]
    public GameObject questPaperPrefab;
    [Tooltip("จุดที่เสก Questpaper (เช่น Content ของ Scroll View)")]
    public Transform questPaperContainer;
    [Tooltip("Panel รายละเอียดเควสเมื่อกดเลือก (ชื่อ รายละเอียด รูป + ปุ่มส่งตัวละคร)")]
    public GameObject questDetailPanel;
    public TextMeshProUGUI detailQuestNameText;
    public TextMeshProUGUI detailQuestDescriptionText;
    public Image detailQuestImage;

    int _selectedQuestIndexForDetail = -1;
    QuestData _selectedQuestDataForDetail;

    void Start()
    {
        GlobalQuestState.ResetAllQuestProgress(storyProgressData);
        if (UseStoryProgress() && storyProgressData != null)
        {
            GlobalQuestState.StoryProgressData = storyProgressData;
            GlobalQuestState.LoadState();
            // โหลดรายชื่อเควสใหม่เฉพาะเมื่อ list ว่าง (ขึ้นบทหรือเพิ่งเริ่ม) — ไม่รีเซ็ตทุกครั้งที่เข้าซีน ไม่งั้นเควสที่ทำแล้วจะกลับมา
            if (GlobalQuestState.ListQuestCount == 0)
                GlobalQuestState.ReloadQuestListFromStoryProgressData();
        }
        else
            GlobalQuestState.LoadState();
        OpenQuestBoard();
    }

    /// <summary> เรียกเมื่อกด "เริ่มเกมใหม่" — รีเซ็ตเควสทั้งหมดแล้วไปบทแรก (ให้ปุ่มเมนูหลักเรียก) </summary>
    public void StartNewGame()
    {
        if (!UseStoryProgress() || storyProgressData == null) return;
        GlobalQuestState.StoryProgressData = storyProgressData;
        GlobalQuestState.ResetAllQuestProgress(storyProgressData);
        GlobalQuestState.SetToFirstChapter(storyProgressData);
        GlobalQuestState.ReloadQuestListFromStoryProgressData();
        OpenQuestBoard();
    }

    bool UseStoryProgress() => storyProgressData != null && storyProgressData.routes != null && storyProgressData.routes.Count > 0;

    /// <summary> หา route ที่เลขบทตรงกับ GlobalQuestState.CurrentChapter ถ้าไม่มีใช้ route แรกที่มีเควส (ไม่ลบรายชื่อเควส) </summary>
    StoryChapterRoute GetCurrentChapterRoute()
    {
        if (!UseStoryProgress()) return null;
        int currentCh = GlobalQuestState.CurrentChapter;
        StoryChapterRoute fallback = null;
        foreach (var route in storyProgressData.routes)
        {
            if (route == null || route.exclusiveQuestList == null || route.exclusiveQuestList.Count == 0)
                continue;
            if (route.chapterNumber == currentCh)
                return route;
            if (fallback == null)
                fallback = route;
        }
        return fallback;
    }

    /// <summary> รายการเควสที่ยังไม่ทำ — ดึงจาก GlobalQuestState (ที่เก็บจาก StoryProgressData) </summary>
    List<(int index, QuestData data)> GetAvailableQuestList()
    {
        if (UseStoryProgress())
            return GlobalQuestState.GetAvailableQuestList();
        var full = GetCurrentChapterQuestListLegacy();
        if (full == null || full.Count == 0) return new List<(int, QuestData)>();
        var completed = GlobalQuestState.GetCompletedQuestIndicesForChapter(GlobalQuestState.CurrentChapter);
        var result = new List<(int, QuestData)>();
        for (int i = 0; i < full.Count; i++)
        {
            if (!completed.Contains(i)) result.Add((i, full[i]));
        }
        return result;
    }

    List<QuestData> GetCurrentChapterQuestListLegacy()
    {
        var route = GetCurrentChapterRoute();
        if (route?.exclusiveQuestList == null) return null;
        return new List<QuestData>(route.exclusiveQuestList);
    }

    /// <summary> รายการเควสของบทปัจจุบัน — ให้ StoryFlowController ใช้ (ดึงจาก GlobalQuestState) </summary>
    List<QuestData> GetQuestListForCharacter(CharacterData character)
    {
        if (UseStoryProgress())
            return GlobalQuestState.GetCurrentChapterQuestDataList();
        return GetCurrentChapterQuestListLegacy();
    }

    QuestData GetQuestAtCurrentPosition()
    {
        if (UseStoryProgress())
            return GlobalQuestState.GetCurrentSelectedQuest();
        var list = GetCurrentChapterQuestListLegacy();
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
        _selectedQuestDataForDetail = null;
        _selectedQuestIndexForDetail = -1;
        if (questDetailPanel != null) questDetailPanel.SetActive(false);
        // ใช้ listQuest จาก GlobalQuestState โดยตรง — เรียกรายชื่อใหม่เฉพาะเมื่อขึ้นบทหรือเควสเหลือ 0

        // เสก Questpaper ถ้ามี Prefab + Container
        if (questPaperPrefab != null && questPaperContainer != null && UseStoryProgress())
        {
            // ลบเฉพาะ UI เก่า (แผ่นเควสที่เคยเสก) ไม่ได้ลบรายชื่อเควสจากข้อมูล — รายชื่อมาจาก GetAvailableQuestList() ด้านล่าง
            foreach (Transform child in questPaperContainer) Destroy(child.gameObject);
            var available = GetAvailableQuestList();
            foreach (var (index, data) in available)
            {
                var go = Instantiate(questPaperPrefab, questPaperContainer);
                var paper = go.GetComponent<QuestPaperItem>();
                if (paper != null) paper.Setup(this, data, index);
            }
            // แสดงเฉพาะรายการเควสก่อน ไม่เปิด panel เลือกตัวละครจนกว่าจะกดเลือกเควส
            if (characterSelectionPanel != null) characterSelectionPanel.SetActive(false);
        }
        else
        {
            if (characterSelectionPanel != null) characterSelectionPanel.SetActive(true);
        }

        QuestData previewQuest = UseStoryProgress() ? GetQuestAtCurrentPosition() : storyFlowController.GetDefaultQuest(currentGlobalQuestIndex);
        if (questDescriptionText != null && previewQuest != null)
        {
            questDescriptionText.text = previewQuest.questDescription;
            GlobalQuestState.ApplyLanguageFont(questDescriptionText);
        }
        GenerateCharacterButtons();
    }

    /// <summary> เปิดรายละเอียดเควสที่กดเลือก (แสดงชื่อ รายละเอียด รูป + ปุ่มส่งตัวละคร) </summary>
    public void OpenQuestDetail(QuestData questData, int questIndex)
    {
        _selectedQuestDataForDetail = questData;
        _selectedQuestIndexForDetail = questIndex;

        if (detailQuestNameText != null && questData != null)
        {
            detailQuestNameText.text = questData.questName;
            GlobalQuestState.ApplyLanguageFont(detailQuestNameText);
        }
        if (detailQuestDescriptionText != null && questData != null)
        {
            detailQuestDescriptionText.text = questData.questDescription;
            GlobalQuestState.ApplyLanguageFont(detailQuestDescriptionText);
        }
        if (detailQuestImage != null)
        {
            if (questData != null && questData.questImage != null)
            {
                detailQuestImage.sprite = questData.questImage;
                detailQuestImage.gameObject.SetActive(true);
            }
            else
                detailQuestImage.gameObject.SetActive(false);
        }

        if (questDetailPanel != null) questDetailPanel.SetActive(true);
        if (characterSelectionPanel != null) characterSelectionPanel.SetActive(true);
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
    void OnCharacterSelected(CharacterData selectedChar)
    {
        Debug.Log($"[QuestManager] ตัดสินใจส่ง: {selectedChar.characterName} เข้าสู่สนามรบ!");

        int index;
        QuestData activeQuest;

        if (_selectedQuestDataForDetail != null && _selectedQuestIndexForDetail >= 0)
        {
            index = _selectedQuestIndexForDetail;
            activeQuest = _selectedQuestDataForDetail;
            GlobalQuestState.CurrentQuestIndex = index;
        }
        else
        {
            index = GetCurrentQuestIndex();
            activeQuest = UseStoryProgress() ? GetQuestAtCurrentPosition() : storyFlowController.GetActiveQuest(index);
        }

        var questList = UseStoryProgress() ? GetQuestListForCharacter(selectedChar) : null;
        if (questList == null)
            questList = UseStoryProgress() ? GlobalQuestState.GetCurrentChapterQuestDataList() : GetCurrentChapterQuestListLegacy();
        storyFlowController.SetCharacter(selectedChar, questList);

        if (questDescriptionText != null && activeQuest != null)
        {
            questDescriptionText.text = activeQuest.questDescription;
            GlobalQuestState.ApplyLanguageFont(questDescriptionText);
        }

        if (activeQuest != null && activeQuest.questType == QuestType.Battle)
        {
            GlobalQuestState.ActiveQuest = activeQuest;
            GlobalQuestState.SelectedCharacter = selectedChar;
            string sceneName = activeQuest.battleConfig != null ? activeQuest.battleConfig.battleSceneName : "";
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"[QuestManager] 🌌 กำลังเปิดประตูมิติ ส่งกองทัพไปยังสมรภูมิ: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
                Debug.LogError("❌ [QuestManager] นายท่านลืมใส่ชื่อ Scene ใน Battle Config หรือเปล่าขอรับ!?");
        }
        else
        {
            if (activeQuest != null && !string.IsNullOrEmpty(activeQuest.sceneToLoad))
            {
                GlobalQuestState.ActiveQuest = activeQuest;
                GlobalQuestState.SelectedCharacter = selectedChar;
                Debug.Log($"[QuestManager] 🌌 โหลดซีนคุย: {activeQuest.sceneToLoad}");
                SceneManager.LoadScene(activeQuest.sceneToLoad);
            }
            else
                storyFlowController.StartQuest(index);
        }
    }

    public void OnQuestCompleted()
    {
        if (UseStoryProgress())
        {
            if (GlobalQuestState.GetAvailableQuestList().Count == 0)
            {
                var routes = storyProgressData.routes;
                if (routes != null && routes.Count > 0)
                {
                    int idx = -1;
                    for (int i = 0; i < routes.Count; i++)
                    {
                        if (routes[i] != null && routes[i].chapterNumber == GlobalQuestState.CurrentChapter)
                        { idx = i; break; }
                    }
                    if (idx >= 0 && idx + 1 < routes.Count && routes[idx + 1] != null)
                    {
                        GlobalQuestState.CurrentChapter = routes[idx + 1].chapterNumber;
                        GlobalQuestState.CurrentQuestIndex = 0;
                        GlobalQuestState.ReloadQuestListFromStoryProgressData();
                        GlobalQuestState.SaveState();
                    }
                }
            }
        }
        else
            currentGlobalQuestIndex++;

        OpenQuestBoard();
    }
}