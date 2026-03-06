using UnityEngine;
using TMPro;
using System.Collections.Generic;

// =========================================================
// 🕊️ เก็บค่าตัวเลขว่าอยู่บทไหน เควสที่เท่าไหร่ + ภาษาปัจจุบัน
// ใส่ใน Hierarchy ได้ และจะไม่ถูกทำลายเมื่อโหลดซีน (DontDestroyOnLoad)
// ฟอนต์ใช้ตามภาษาจาก LanguageFontSettings (ใส่ฟอนต์เอง)
// =========================================================
public class GlobalQuestState : MonoBehaviour
{
    public static GlobalQuestState Instance { get; private set; }

    [Header("--- สถานะ Battle (ส่งข้าม Scene) ---")]
    public QuestData activeQuest;
    public CharacterData selectedCharacter;

    [Header("--- บทที่ + เควสที่ (เซฟได้) ---")]
    public int currentChapter = 1;
    public int currentQuestIndex = 0;

    [Header("--- ภาษา (อิง key เดิม PlayerPrefsSetLanguage) ---")]
    [Tooltip("0 = ENG, 1 = THAI, 2 = JP")]
    public int currentLanguage = 1;

    [Header("--- ฟอนต์ต่อภาษา ---")]
    public LanguageFontSettings fontSettings;

    public static readonly string SetLanguage = "PlayerPrefsSetLanguage";

    const string KeyChapter = "QuestState_Chapter";
    const string KeyQuestIndex = "QuestState_QuestIndex";
    const string KeyCompletedPrefix = "QuestState_Completed_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- Static accessors (ให้โค้ดเดิมที่ใช้ GlobalQuestState.CurrentChapter ยังใช้ได้) ---
    public static QuestData ActiveQuest { get => Get().activeQuest; set => Get().activeQuest = value; }
    public static CharacterData SelectedCharacter { get => Get().selectedCharacter; set => Get().selectedCharacter = value; }
    public static int CurrentChapter { get => Get().currentChapter; set => Get().currentChapter = value; }
    public static int CurrentQuestIndex { get => Get().currentQuestIndex; set => Get().currentQuestIndex = value; }
    public static int CurrentLanguage { get => Get().currentLanguage; set => Get().currentLanguage = value; }
    public static LanguageFontSettings FontSettings { get => Get().fontSettings; set => Get().fontSettings = value; }

    static GlobalQuestState Get()
    {
        if (Instance == null)
            Instance = FindFirstObjectByType<GlobalQuestState>();
        return Instance;
    }

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
        var g = Get();
        if (g != null) g.currentLanguage = Mathf.Clamp(language, 0, 2);
        PlayerPrefs.SetInt(SetLanguage, CurrentLanguage);
        PlayerPrefs.Save();
    }

    /// <summary> ดึงเซ็ตเควสที่สำเร็จแล้วของบทที่กำหนด (ใช้กรองรายชื่อเควส) </summary>
    public static HashSet<int> GetCompletedQuestIndicesForChapter(int chapter)
    {
        var set = new HashSet<int>();
        string key = KeyCompletedPrefix + chapter;
        if (!PlayerPrefs.HasKey(key)) return set;
        string raw = PlayerPrefs.GetString(key, "");
        if (string.IsNullOrEmpty(raw)) return set;
        foreach (string part in raw.Split(','))
        {
            if (int.TryParse(part.Trim(), out int idx))
                set.Add(idx);
        }
        return set;
    }

    /// <summary> บันทึกว่าเควสของบทนี้ที่ index นี้สำเร็จแล้ว (ลบออกจากรายการ) </summary>
    public static void MarkQuestCompleted(int chapter, int questIndex)
    {
        var set = GetCompletedQuestIndicesForChapter(chapter);
        set.Add(questIndex);
        PlayerPrefs.SetString(KeyCompletedPrefix + chapter, string.Join(",", set));
        PlayerPrefs.Save();
    }

    /// <summary> โหลดค่าจากเซฟ (เรียกตอนเริ่มเกม) </summary>
    public static void LoadState()
    {
        var g = Get();
        if (g == null) return;
        if (PlayerPrefs.HasKey(KeyChapter))
            g.currentChapter = PlayerPrefs.GetInt(KeyChapter, 1);
        if (PlayerPrefs.HasKey(KeyQuestIndex))
            g.currentQuestIndex = PlayerPrefs.GetInt(KeyQuestIndex, 0);
        if (PlayerPrefs.HasKey(SetLanguage))
            g.currentLanguage = PlayerPrefs.GetInt(SetLanguage, 1);
    }

    /// <summary> เซฟค่าลง (เรียกตอนเซฟเกม หรือเมื่อขยับบท/เควส) </summary>
    public static void SaveState()
    {
        var g = Get();
        if (g == null) return;
        PlayerPrefs.SetInt(KeyChapter, g.currentChapter);
        PlayerPrefs.SetInt(KeyQuestIndex, g.currentQuestIndex);
        PlayerPrefs.SetInt(SetLanguage, g.currentLanguage);
        PlayerPrefs.Save();
    }
}
