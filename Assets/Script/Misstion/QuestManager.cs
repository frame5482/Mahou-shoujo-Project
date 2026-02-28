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

    [Header("--- ✨ อนิเมชันปุ่มตัวละคร ---")]
    public QuestButtonAnimation buttonAnimation; // สคริปต์จัดการอนิเมชัน
    [Tooltip("ขนาดปุ่ม (ถ้าเป็น 0 จะใช้ขนาดจากรูปภาพ)")]
    public float buttonSize = 200f;

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
            if (txt != null) txt.text = character.characterName;

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
       /// characterSelectionPanel.SetActive(false);

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