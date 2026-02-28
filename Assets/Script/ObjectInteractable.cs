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
        Debug.Log("⚡ [แท่นบูชา] ได้รับคำสั่งปลดผนึกจากร่างอวตารแล้ว!");

        if (uiToOpen != null)
        {
            uiToOpen.SetActive(true);
            HidePrompt();
            Debug.Log($"✅ [แท่นบูชา] เปิดหน้าต่าง UI: {uiToOpen.name} สำเร็จ!");
        }
        else
        {
            Debug.LogError("❌ [ข้อผิดพลาดร้ายแรง] นายท่านลืมใส่หน้าต่าง UI! (ช่อง uiToOpen เป็น None) รีบไปลาก Panel มาใส่ใน Inspector ด่วนเลยขอรับ!");
        }
    }

    // ฟังก์ชันเรียกออร่าและข้อความ
    public void ShowPrompt()
    {
        if (promptTextUI != null)
        {
            promptTextUI.text = interactMessage;
            promptTextUI.gameObject.SetActive(true);
        }
    }

    // ฟังก์ชันดับออร่า
    public void HidePrompt()
    {
        if (promptTextUI != null) promptTextUI.gameObject.SetActive(false);
    }
}