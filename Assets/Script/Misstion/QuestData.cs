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

    [Header("--- ลำดับเหตุการณ์ (Story Flow) ---")]
    [Tooltip("ใช้เมื่อ questType = Story")]
    public List<StoryStep> storyFlow; // ลิสต์ของเหตุการณ์ที่จะเกิดขึ้นในเควสนี้

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