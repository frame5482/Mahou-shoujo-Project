using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StoryFlowController : MonoBehaviour
{
    // โครงสร้างข้อมูลสำหรับ "แต่ละขั้นตอน" ของเนื้อเรื่อง
    [System.Serializable]
    public class StoryStep
    {
        public string stepName; // ตั้งชื่อไว้อ่านเอง เช่น "Intro", "Choice Phase"
        public List<StoryOption> options; // รายการตัวเลือกในขั้นนี้
    }

    [System.Serializable]
    public class StoryOption
    {
        public string buttonText;     // ข้อความบนปุ่ม (เช่น "ไปทางซ้าย", "สู้เลย")
        public Textbase targetTextbase; // Textbase ที่จะเล่นเมื่อเลือกข้อนี้
    }

    [Header("--- การตั้งค่าบทละคร ---")]
    public List<StoryStep> storyFlow; // ลาก Textbase มาใส่เรียงลำดับที่นี่ (0, 1, 2...)

    [Header("--- การเชื่อมต่อ ---")]
    public DialogueManager dialogueManager;
    public GameObject choicePanel;         // Panel ที่เก็บปุ่มตัวเลือก
    public Transform buttonContainer;      // จุดที่จะให้ปุ่มไปเกิด (Content หรือ Panel)
    public GameObject buttonPrefab;        // Prefab ของปุ่ม (ต้องมี Button และ TextMeshProUGUI)

    private int currentStepIndex = 0; // ตัวนับว่าตอนนี้อยู่ขั้นตอนไหนแล้ว

    void Start()
    {
        // เริ่มต้นที่ขั้นตอนแรก (Step 0)
        currentStepIndex = 0;
        ProcessCurrentStep();
    }

    // ฟังก์ชันสมองกล: ตัดสินใจว่าจะทำอะไรในขั้นตอนนี้
    void ProcessCurrentStep()
    {
        // 1. เช็คว่าเนื้อเรื่องจบหมดหรือยัง
        if (currentStepIndex >= storyFlow.Count)
        {
            Debug.Log("--- จบบริบูรณ์ (End of Flow) ---");
            // ตรงนี้อาจจะสั่งให้ปิด UI หรือโหลด Scene ใหม่ก็ได้
            return;
        }

        // ดึงข้อมูลขั้นตอนปัจจุบันออกมา
        StoryStep currentStep = storyFlow[currentStepIndex];

        // 2. เช็คจำนวนตัวเลือก
        if (currentStep.options.Count == 0)
        {
            Debug.LogWarning($"ขั้นตอน {currentStep.stepName} ไม่มี Textbase ใส่ไว้เลย! ข้ามไปขั้นถัดไป");
            GoToNextStep();
        }
        else if (currentStep.options.Count == 1)
        {
            // === กรณีมี 1 อัน: เล่นอัตโนมัติ (Auto Play) ===
            choicePanel.SetActive(false); // ซ่อนปุ่ม
            PlaySpecificTextbase(currentStep.options[0].targetTextbase);
        }
        else
        {
            // === กรณีมีหลายอัน: แสดงปุ่มตัวเลือก (Choice Phase) ===
            ShowChoiceButtons(currentStep.options);
        }
    }

    // ฟังก์ชันสร้างปุ่ม
    void ShowChoiceButtons(List<StoryOption> options)
    {
        choicePanel.SetActive(true); // เปิดหน้าต่างเลือก

        // ล้างปุ่มเก่าทิ้งก่อน
        foreach (Transform child in buttonContainer) Destroy(child.gameObject);

        // สร้างปุ่มใหม่ตามจำนวนตัวเลือก
        foreach (var option in options)
        {
            Debug.Log("Instantiate(buttonPrefab");

            GameObject btn = Instantiate(buttonPrefab, buttonContainer);

            // ตั้งชื่อปุ่ม
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = option.buttonText;

            // ฝังคำสั่งเมื่อกดปุ่ม
            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                choicePanel.SetActive(false); // ปิดหน้าต่างเลือก
                PlaySpecificTextbase(option.targetTextbase); // เล่น Textbase ที่เลือก
            });
        }
    }

    // ฟังก์ชันสั่งให้ DialogueManager ทำงาน
    void PlaySpecificTextbase(Textbase textbaseToPlay)
    {
        if (textbaseToPlay == null)
        {
            Debug.LogError("Textbase เป็น Null! กรุณาลากใส่ใน Inspector");
            return;
        }

        // 1. ยัดข้อมูล Textbase ใหม่ใส่ DialogueManager
        dialogueManager.textbase = textbaseToPlay;
        dialogueManager.currentLineIndex = 0;
        Debug.LogError("Textbase เปล่ยนทำงาน");
        // 2. *** หัวใจสำคัญ *** สั่งว่า "ถ้าคุยจบ ให้กลับมาเรียกฟังก์ชัน GoToNextStep ของฉันนะ"
        dialogueManager.onDialogueFinished = GoToNextStep;

        // 3. เริ่มคุย
        dialogueManager.ShowCurrentLine();
    }

    // ฟังก์ชันข้ามไปขั้นถัดไป
    void GoToNextStep()
    {
        currentStepIndex++; // เพิ่มเลขขั้นตอน
        ProcessCurrentStep(); // วนลูปกลับไปตัดสินใจใหม่
    }
}