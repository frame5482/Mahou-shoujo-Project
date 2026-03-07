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

    [Header("--- ซีนหน้าเควสหลัก ---")]
    [Tooltip("ชื่อซีนที่จะโหลดเมื่อจบเควส (เช่น หน้าจอเลือกเควส) ใส่ใน Build Settings ด้วย")]
    public string mainQuestSceneName = "";

    [Header("--- บทที่ + เควสที่ (เซฟได้) ---")]
    public int currentChapter = 1;
    public int currentQuestIndex = 0;

    [Header("--- ความคืบหน้าเนื้อเรื่อง (รับจาก QuestManager) ---")]
    public StoryProgressData storyProgressData;

    /// <summary> รายชื่อเควสทั้งหมดของบทที่กำลังทำงานอยู่ (ที่ยังไม่ทำ) — เรียกใหม่จาก StoryProgressData เฉพาะเมื่อขึ้นบทถัดไป หรือจำนวนเควสเหลือ 0 </summary>
    List<(int originalIndex, QuestData data)> listQuest = new List<(int, QuestData)>();

    [Header("--- 📜 รายชื่อเควสบทปัจจุบัน (แสดงใน Inspector) ---")]
    [Tooltip("รายการเควสที่ยังไม่ทำ — ซิงค์กับ listQuest ตอนรัน ใช้ดูใน Inspector ได้")]
    public List<QuestData> listQuestDisplay = new List<QuestData>();

    [Header("--- ภาษา (อิง key เดิม PlayerPrefsSetLanguage) ---")]
    [Tooltip("0 = ENG, 1 = THAI, 2 = JP")]
    public int currentLanguage = 1;

    [Header("--- ฟอนต์ต่อภาษา ---")]
    public LanguageFontSettings fontSettings;

    public static readonly string SetLanguage = "PlayerPrefsSetLanguage";

    public bool isResetStory;

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

        if (isResetStory == true)
        {
            currentChapter = 0;
            currentQuestIndex = 0;
            SaveState();
        }

    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- Static accessors (ให้โค้ดเดิมที่ใช้ GlobalQuestState.CurrentChapter ยังใช้ได้) ---
    public static QuestData ActiveQuest { get => Get().activeQuest; set => Get().activeQuest = value; }
    public static CharacterData SelectedCharacter { get => Get().selectedCharacter; set => Get().selectedCharacter = value; }
    public static string MainQuestSceneName { get => Get().mainQuestSceneName; set { var g = Get(); if (g != null) g.mainQuestSceneName = value; } }
    public static int CurrentChapter { get => Get().currentChapter; set => Get().currentChapter = value; }
    public static int CurrentQuestIndex { get => Get().currentQuestIndex; set => Get().currentQuestIndex = value; }
    public static StoryProgressData StoryProgressData { get => Get().storyProgressData; set { var g = Get(); if (g != null) g.storyProgressData = value; } }
    /// <summary> รายชื่อเควสบทปัจจุบัน — เรียกจากที่อื่นได้ (อ่านอย่างเดียว ไม่ควรแก้ list โดยตรง ใช้ RemoveQuestCompleted แทน) </summary>
    public static List<(int originalIndex, QuestData data)> ListQuest => Get()?.listQuest;
    /// <summary> จำนวนเควสที่เหลือใน listQuest (บทปัจจุบัน) </summary>
    public static int ListQuestCount { get { var g = Get(); return g?.listQuest?.Count ?? 0; } }
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

    /// <summary> รีเซ็ตการทำเควสใหม่หมด — ลบสถานะ "ทำแล้ว" ทุกบท (ใช้ตอนกดเริ่มเกมใหม่) </summary>
    public static void ResetAllQuestProgress(StoryProgressData storyData)
    {
        if (storyData != null && storyData.routes != null)
        {
            foreach (var route in storyData.routes)
            {
                if (route != null)
                    PlayerPrefs.DeleteKey(KeyCompletedPrefix + route.chapterNumber);
            }
        }
        for (int ch = 0; ch < 100; ch++)
            PlayerPrefs.DeleteKey(KeyCompletedPrefix + ch);
        PlayerPrefs.Save();
    }

    /// <summary> ตั้งไปบทแรกตาม StoryProgressData (route แรกที่มีเควส) แล้วเซฟ </summary>
    public static void SetToFirstChapter(StoryProgressData storyData)
    {
        var g = Get();
        if (g == null || storyData == null || storyData.routes == null || storyData.routes.Count == 0) return;
        foreach (var route in storyData.routes)
        {
            if (route == null || route.exclusiveQuestList == null || route.exclusiveQuestList.Count == 0)
                continue;
            g.currentChapter = route.chapterNumber;
            g.currentQuestIndex = 0;
            SaveState();
            return;
        }
    }

    /// <summary> เรียกรายชื่อเควสใหม่จาก StoryProgressData — เรียกเฉพาะเมื่อขึ้นบทถัดไป หรือจำนวนเควสเหลือ 0 (และจะขยับบทแล้วโหลดบทใหม่) </summary>
    public static void ReloadQuestListFromStoryProgressData()
    {
        var g = Get();
        if (g == null || g.storyProgressData == null || g.storyProgressData.routes == null) return;
        int ch = g.currentChapter;
        var completed = GetCompletedQuestIndicesForChapter(ch);
        g.listQuest.Clear();
        g.listQuestDisplay.Clear();
        foreach (var route in g.storyProgressData.routes)
        {
            if (route == null || route.exclusiveQuestList == null || route.chapterNumber != ch) continue;
            for (int i = 0; i < route.exclusiveQuestList.Count; i++)
            {
                if (!completed.Contains(i))
                {
                    g.listQuest.Add((i, route.exclusiveQuestList[i]));
                    g.listQuestDisplay.Add(route.exclusiveQuestList[i]);
                }
            }
            return;
        }
    }

    /// <summary> รายชื่อเควสของบทปัจจุบัน (listQuest) — ให้ที่อื่นเรียกไปใช้ (index, QuestData) </summary>
    public static List<(int index, QuestData data)> GetAvailableQuestList()
    {
        var g = Get();
        if (g == null || g.listQuest == null) return new List<(int, QuestData)>();
        var result = new List<(int, QuestData)>();
        for (int i = 0; i < g.listQuest.Count; i++)
            result.Add((i, g.listQuest[i].data));
        return result;
    }

    /// <summary> เควสที่เลือกอยู่ (ตาม CurrentQuestIndex ใน listQuest) — ให้ StoryFlowController ดึงไปเล่น </summary>
    public static QuestData GetCurrentSelectedQuest()
    {
        var g = Get();
        if (g == null || g.listQuest == null || g.listQuest.Count == 0) return null;
        int idx = Mathf.Clamp(g.currentQuestIndex, 0, g.listQuest.Count - 1);
        return g.listQuest[idx].data;
    }

    /// <summary> เควสที่ index ใน listQuest (สำหรับดึงไปเล่น) </summary>
    public static QuestData GetQuestAt(int indexInList)
    {
        var g = Get();
        if (g == null || g.listQuest == null || indexInList < 0 || indexInList >= g.listQuest.Count) return null;
        return g.listQuest[indexInList].data;
    }

    /// <summary> รายการ QuestData ของบทปัจจุบัน (จาก listQuest) — ให้ที่อื่นเรียกไปใช้ </summary>
    public static List<QuestData> GetCurrentChapterQuestDataList()
    {
        var g = Get();
        if (g == null || g.listQuest == null) return new List<QuestData>();
        var list = new List<QuestData>();
        foreach (var (_, data) in g.listQuest)
            list.Add(data);
        return list;
    }

    /// <summary> เมื่อทำเควสเสร็จ — ลบเควสนั้นออกจาก listQuest และบันทึกว่าทำแล้ว </summary>
    public static void RemoveQuestCompleted(int indexInList)
    {
        var g = Get();
        if (g == null || g.listQuest == null || indexInList < 0 || indexInList >= g.listQuest.Count) return;
        var (originalIndex, _) = g.listQuest[indexInList];
        MarkQuestCompleted(g.currentChapter, originalIndex);
        g.listQuest.RemoveAt(indexInList);
        if (g.listQuestDisplay != null && indexInList >= 0 && indexInList < g.listQuestDisplay.Count)
            g.listQuestDisplay.RemoveAt(indexInList);
        g.currentQuestIndex = Mathf.Clamp(g.currentQuestIndex, 0, Mathf.Max(0, g.listQuest.Count - 1));
        SaveState();
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
