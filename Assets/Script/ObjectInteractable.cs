using UnityEngine;
using TMPro;

public class ObjectInteractable : MonoBehaviour
{
    [Header("--- 🔮 พลังเวทที่หลับใหล ---")]
    public string interactMessage = "กด [F] เพื่อเปิดสภาบัญชาการ";
    public GameObject uiToOpen;
    public TextMeshProUGUI promptTextUI;

    // ฟังก์ชันนี้จะถูกเรียกโดย "ร่างอวตาร" (Player) เมื่อกดปุ่ม
    public void BreakTheSeal()
    {
        if (uiToOpen != null)
        {
            uiToOpen.SetActive(true);
            HidePrompt();

            // 🔓 ปลดผนึกวิญญาณเมาส์! ให้ปรากฏตัวและขยับได้อิสระ
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Debug.Log($"✅ [แท่นบูชา] เปิดหน้าต่าง UI และปลดปล่อยเมาส์สำเร็จ!");
        }
    }
    // ฟังก์ชันเรียกออร่าและข้อความ
    public void ShowPrompt()
    {
        if (promptTextUI != null)
        {
            promptTextUI.text = interactMessage;
            GlobalQuestState.ApplyLanguageFont(promptTextUI);
            promptTextUI.gameObject.SetActive(true);
        }
    }

    // ฟังก์ชันดับออร่า
    public void HidePrompt()
    {
        if (promptTextUI != null) promptTextUI.gameObject.SetActive(false);
    }
}