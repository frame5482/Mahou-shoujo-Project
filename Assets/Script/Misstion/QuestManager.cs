using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [Header("--- 🔮 จุดเชื่อมต่อเวทมนตร์ ---")]
    public StoryFlowController storyFlowController;

    [Header("--- 📜 สถานะภารกิจปัจจุบัน ---")]
    public int currentGlobalQuestIndex = 0; // ตอนนี้อยู่ภารกิจลำดับที่เท่าไหร่
    public TextMeshProUGUI questDescriptionText; // ช่องซ้ายสำหรับโชว์รายละเอียดภารกิจ

    [Header("--- 🎭 ระบบคัดเลือกตัวละคร ---")]
    public GameObject characterSelectionPanel; // หน้าต่างหรือ Panel ที่ครอบปุ่มเลือกตัวละครไว้
    public List<CharacterData> availableCharacters; // รายชื่อตัวละครที่มีในค่าย
    public Transform characterButtonContainer; // จุดที่ปุ่มจะไปเกิด (ใส่ Vertical/Horizontal Layout ไว้)
    public GameObject characterButtonPrefab; // Prefab ของปุ่ม (ต้องมี Image เพื่อใส่รูปหน้า)

    void Start()
    {
        // เริ่มเกมมา ให้เปิดหน้าต่างเลือกตัวละคร
        OpenQuestBoard();
    }

    public void OpenQuestBoard()
    {
        characterSelectionPanel.SetActive(true);

        // ดึงรายละเอียดเควส "แบบปกติ" (Default) มาโชว์เป็นน้ำจิ้มก่อนเลือกตัวละคร
        QuestData previewQuest = storyFlowController.GetDefaultQuest(currentGlobalQuestIndex);
        if (previewQuest != null)
        {
            questDescriptionText.text = previewQuest.questDescription;
        }
        print("GenerateCharacterButtons");
        // สร้างปุ่มเลือกตัวละคร
        GenerateCharacterButtons();
    }

    void GenerateCharacterButtons()
    {
        // เติมบรรทัดนี้ลงไปเพื่อเช็คว่าระบบเห็นทหารกี่นาย!
        Debug.Log($"[QuestManager] กำลังสร้างปุ่ม... พบรายชื่อทหารทั้งหมด: {availableCharacters.Count} นาย");

        foreach (Transform child in characterButtonContainer) Destroy(child.gameObject);

        foreach (var character in availableCharacters)
        {
            GameObject btnObj = Instantiate(characterButtonPrefab, characterButtonContainer);
            Button btn = btnObj.GetComponent<Button>();

            // 🖼️ เอารูป Portrait จาก CharacterData มาใส่ในปุ่ม
            Image btnImage = btnObj.GetComponent<Image>();
            if (character.portrait != null && btnImage != null)
            {
                btnImage.sprite = character.portrait;
            }

            // (Optional) ถ้าปุ่มมี Text ก็ใส่ชื่อกำกับไว้ด้วย
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = character.characterName;

            // ฝังคำสั่งเมื่อนายท่านกดปุ่มตัวละครนี้!
            btn.onClick.AddListener(() =>
            {
                OnCharacterSelected(character);
            });
        }
    }

    // 🌟 เมื่อตัดสินใจเลือกหมากตัวใดตัวหนึ่ง 🌟
    void OnCharacterSelected(CharacterData selectedChar)
    {
        Debug.Log($"[QuestManager] ตัดสินใจส่ง: {selectedChar.characterName} เข้าสู่สนามรบ!");

        // 1. ส่งตัวละครไปให้ StoryFlow ประมวลผลเส้นทาง (เช็คว่าเป็นเส้นทางลับหรือไม่)
        storyFlowController.SetCharacter(selectedChar);

        // 2. ดึงเควสที่ "จะทำงานจริงๆ" ออกมา (ถ้าเป็นตัวละครที่มีเส้นทางลับ มันจะดึงเนื้อเรื่องลับมา!)
        QuestData activeQuest = storyFlowController.GetActiveQuest(currentGlobalQuestIndex);

        if (activeQuest != null)
        {
            // 3. อัปเดตช่องรายละเอียดเควสด้านซ้าย ให้เป็นฉบับจริง!
            questDescriptionText.text = activeQuest.questDescription;
        }

        // 4. ปิดหน้าต่างเลือกตัวละคร
        characterSelectionPanel.SetActive(false);

        // 5. สั่งลุยภารกิจ!
        storyFlowController.StartQuest(currentGlobalQuestIndex);
    }

    // ฟังก์ชันนี้เรียกใช้เมื่อเควสจบ (เพื่อให้ขยับไปเควสถัดไป)
    public void OnQuestCompleted()
    {
        currentGlobalQuestIndex++; // ขยับเลขลำดับเควส
        OpenQuestBoard(); // เปิดหน้าต่างเลือกตัวละครสำหรับเควสต่อไป
    }
}