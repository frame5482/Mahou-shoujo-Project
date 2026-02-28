using UnityEngine;

// =========================================================
// ใส่ใน Scene หนึ่งตัว แล้วลาก LanguageFontSettings มาใส่
// จะเซ็ต GlobalQuestState.FontSettings ให้ระบบฟอนต์ตามภาษาใช้ได้
// =========================================================
public class LanguageFontBootstrap : MonoBehaviour
{
    public LanguageFontSettings fontSettings;

    void Awake()
    {
        if (fontSettings != null)
            GlobalQuestState.FontSettings = fontSettings;
    }
}
