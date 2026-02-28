using UnityEngine;
using TMPro;

// =========================================================
// 🕊️ เก็บค่าตัวเลขว่าอยู่บทไหน เควสที่เท่าไหร่ + ภาษาปัจจุบัน
// ตัวการปรับภาษาอิงจากที่นี่; ฟอนต์ใช้ตามภาษาจาก LanguageFontSettings (ใส่ฟอนต์เอง)
// =========================================================
public static class GlobalQuestState
{
    // --- สถานะ Battle (ส่งข้าม Scene) ---
    public static QuestData ActiveQuest;
    public static CharacterData SelectedCharacter;

    // --- บทที่ + เควสที่ (เซฟได้) ---
    public static int CurrentChapter = 1;
    public static int CurrentQuestIndex = 0;

    // --- ภาษา (อิง key เดิม PlayerPrefsSetLanguage) ---
    /// <summary> 0 = ENG, 1 = THAI, 2 = JP </summary>
    public static int CurrentLanguage = 1;
    public static readonly string SetLanguage = "PlayerPrefsSetLanguage";

    /// <summary> ฟอนต์ต่อภาษา (ให้ใส่ใน Inspector ผ่าน component ที่เซ็ต FontSettings) </summary>
    public static LanguageFontSettings FontSettings;

    const string KeyChapter = "QuestState_Chapter";
    const string KeyQuestIndex = "QuestState_QuestIndex";

    /// <summary> ใส่ฟอนต์ให้ข้อความตามภาษาปัจจุบัน </summary>
    public static void ApplyLanguageFont(TextMeshProUGUI text)
    {
        if (text == null) return;
        if (FontSettings != null)
            FontSettings.ApplyTo(text);
    }

    /// <summary> เปลี่ยนภาษาแล้วเซฟ (0=ENG, 1=THAI, 2=JP) เรียกจากเมนูตั้งค่า </summary>
    public static void SetLanguageAndSave(int language)
    {
        CurrentLanguage = Mathf.Clamp(language, 0, 2);
        PlayerPrefs.SetInt(SetLanguage, CurrentLanguage);
        PlayerPrefs.Save();
    }

    /// <summary> โหลดค่าจากเซฟ (เรียกตอนเริ่มเกม) </summary>
    public static void LoadState()
    {
        if (PlayerPrefs.HasKey(KeyChapter))
            CurrentChapter = PlayerPrefs.GetInt(KeyChapter, 1);
        if (PlayerPrefs.HasKey(KeyQuestIndex))
            CurrentQuestIndex = PlayerPrefs.GetInt(KeyQuestIndex, 0);
        if (PlayerPrefs.HasKey(SetLanguage))
            CurrentLanguage = PlayerPrefs.GetInt(SetLanguage, 1);
    }

    /// <summary> เซฟค่าลง (เรียกตอนเซฟเกม หรือเมื่อขยับบท/เควส) </summary>
    public static void SaveState()
    {
        PlayerPrefs.SetInt(KeyChapter, CurrentChapter);
        PlayerPrefs.SetInt(KeyQuestIndex, CurrentQuestIndex);
        PlayerPrefs.SetInt(SetLanguage, CurrentLanguage);
        PlayerPrefs.Save();
    }
}
