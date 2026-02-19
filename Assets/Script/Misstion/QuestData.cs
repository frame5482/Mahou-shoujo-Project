using UnityEngine;
using System.Collections.Generic;

// สร้างเมนูสำหรับคลิกขวาเพื่อสร้าง Asset คัมภีร์เควส
[CreateAssetMenu(fileName = "NewQuestData", menuName = "MissionSystem/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("--- ข้อมูลทั่วไปของภารกิจ ---")]
    public string questName; // ชื่อเควสเอาไว้อ่านเอง
    [TextArea] public string questDescription;

    [Header("--- ลำดับเหตุการณ์ (Story Flow) ---")]
    public List<StoryStep> storyFlow; // ลิสต์ของเหตุการณ์ที่จะเกิดขึ้นในเควสนี้
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