using UnityEngine;
using System.Collections.Generic;

// เลือกประเภทเควส: เรื่องราว (บทสนทนา) หรือ ต่อสู้
public enum QuestType
{
    Story,   // เควสเรื่องราว มี Story Flow + บทสนทนา
    Battle   // เควสต่อสู้ เสกผู้เล่น + มอนสเตอร์ แล้วให้รางวัล
}

// สร้างเมนูสำหรับคลิกขวาเพื่อสร้าง Asset คัมภีร์เควส
[CreateAssetMenu(fileName = "NewQuestData", menuName = "MissionSystem/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("--- ข้อมูลทั่วไปของภารกิจ ---")]
    public string questName; // ชื่อเควสเอาไว้อ่านเอง
    [TextArea] public string questDescription;
    [Tooltip("รูปภาพสำหรับแสดงใน Questpaper (ถ้าว่างจะไม่แสดงรูป)")]
    public Sprite questImage;

    [Header("--- ซีนที่โหลดเมื่อเริ่มเควส ---")]
    [Tooltip("ใช้เมื่อเป็น Story; ถ้าว่างจะเล่นในฉากเดิม. Battle ใช้ battleConfig.battleSceneName")]
    public string sceneToLoad;

    [Header("--- ประเภทเควส ---")]
    [Tooltip("Story = เรื่องราว/บทสนทนา | Battle = ต่อสู้ (เสกผู้เล่น+มอนสเตอร์)")]
    public QuestType questType = QuestType.Story;

    [Header("--- ลำดับเหตุการณ์ (Story Flow) ตามตัวละคร ---")]
    [Tooltip("ใช้เมื่อ questType = Story — เลือกตัวละครไหนจะใช้ storyFlow ของ entry นั้น")]
    public List<CharacterStoryFlow> characterStoryFlows = new List<CharacterStoryFlow>();

    /// <summary> ดึง storyFlow ที่ตรงกับตัวละคร (ถ้าไม่มีใช้ตัวที่ character เป็น null) </summary>
    public List<StoryStep> GetStoryFlowForCharacter(CharacterData character)
    {
        if (characterStoryFlows == null || characterStoryFlows.Count == 0) return null;
        foreach (var entry in characterStoryFlows)
        {
            if (entry != null && entry.character == character && entry.storyFlow != null)
                return entry.storyFlow;
        }
        foreach (var entry in characterStoryFlows)
        {
            if (entry != null && entry.character == null && entry.storyFlow != null)
                return entry.storyFlow;
        }
        return null;
    }

    [Header("--- การตั้งค่าเควสต่อสู้ (Battle) ---")]
    [Tooltip("ใช้เมื่อ questType = Battle")]
    public BattleQuestConfig battleConfig;
}

// ---------------------------------------------------------
// ข้อมูลเควสต่อสู้: ตัวละครที่เสก, มอนสเตอร์ + จุดเสก, รางวัล
// ---------------------------------------------------------
[System.Serializable]
public class BattleQuestConfig
{
    [Header("--- 🌌 มิติสมรภูมิ (Scene) ---")]
    [Tooltip("พิมพ์ชื่อ Scene ที่ต้องการโหลดให้ตรงเป๊ะ! (อย่าลืมเอา Scene ไปใส่ใน Build Settings ด้วยนะขอรับ)")]
    public string battleSceneName = "BattleArena_Forest";

    [Header("--- 💰 รางวัล ---")]
    [Tooltip("เงินที่ได้เมื่อจบการต่อสู้")]
    public int rewardGold;

    [Header("--- 👿 กองทัพมอนสเตอร์ ---")]
    [Tooltip("รายการมอนสเตอร์ที่จะเสก (แต่ละตัวระบุ Prefab และดัชนีจุดเสกใน BattleQuestController)")]
    public List<MonsterSpawnEntry> monsterSpawns = new List<MonsterSpawnEntry>();
}

[System.Serializable]
public class MonsterSpawnEntry
{
    [Tooltip("Prefab ของมอนสเตอร์ที่จะเสก")]
    public GameObject monsterPrefab;

    // ⚡ สิ่งที่เพิ่มเข้ามา: จำนวนมอนสเตอร์ที่ต้องการเสก!
    [Tooltip("จำนวนมอนสเตอร์ที่จะเสกจาก Prefab นี้")]
    public int amount = 1;

    [Tooltip("ดัชนีจุดเสก (ตรงกับลำดับใน BattleQuestController.monsterSpawnPoints)")]
    public int spawnPointIndex;
}

// ---------------------------------------------------------
// เงื่อนไขตัวละคร + บทสนทนา (ใส่ใน QuestData แทน StoryProgressData)
// ---------------------------------------------------------
[System.Serializable]
public class CharacterStoryFlow
{
    [Tooltip("ตัวละครที่ใช้บทสนทนานี้ (ว่าง = ใช้เป็น default เมื่อไม่ตรงตัวอื่น)")]
    public CharacterData character;
    [Tooltip("ลำดับเหตุการณ์เมื่อเลือกตัวละครนี้")]
    public List<StoryStep> storyFlow = new List<StoryStep>();
}

// ---------------------------------------------------------
// โครงสร้างของแต่ละ "ก้าว" ในเนื้อเรื่อง
// ---------------------------------------------------------
[System.Serializable]
public class StoryStep
{
    public string stepName;
    public List<StoryOption> options;
}

// ---------------------------------------------------------
// โครงสร้างของ "ทางเลือก" ในแต่ละก้าว
// ---------------------------------------------------------
[System.Serializable]
public class StoryOption
{
    public string buttonText;
    public Textbase targetTextbase;

    [Header("--- 💰 รางวัล (Reward) ---")]
    public int rewardGold;

    [Header("--- เงื่อนไข Stat (ใส่ 0 คือไม่เช็ค) ---")]
    public int reqHP;
    public int reqVIT;
    public int reqSTR;
    public int reqDEX;
    public int reqINT;
    public int reqCHA;
    public int reqLUCK;

    [Header("--- เงื่อนไข Skill ---")]
    public string skillNeed;
}