using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    // Singleton: เพื่อให้เรียกใช้ได้จากทุกที่ (ResourceManager.Instance)
    public static ResourceManager Instance;

    [Header("--- ทรัพย์สมบัติ ---")]
    public int currentGold = 0; // เงินที่มีปัจจุบัน

    [Header("--- UI แสดงผล ---")]
    public TextMeshProUGUI goldText; // ลาก Text UI มาใส่ตรงนี้เพื่อโชว์เงิน

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI(); // เริ่มเกมมาอัปเดตหน้าจอก่อน
    }

    // ฟังก์ชันเพิ่มเงิน (เรียกจาก StoryFlowController)
    public void AddGold(int amount)
    {
        currentGold += amount;
        Debug.Log($"💰 ได้รับเงิน: {amount} G | รวม: {currentGold}");
        UpdateUI();
    }

    // ฟังก์ชันอัปเดตตัวเลขบนหน้าจอ
    void UpdateUI()
    {
        if (goldText != null)
        {
            goldText.text = $"{currentGold:N0} G"; // :N0 คือใส่ลูกน้ำให้ด้วย (เช่น 1,000)
        }
    }
}