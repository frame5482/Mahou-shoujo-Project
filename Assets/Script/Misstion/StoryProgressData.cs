using UnityEngine;
using System.Collections.Generic;

// =========================================================
// 📦 เก็บแต่ละบท = เลขบท + รายการเควส (บทสนทนาแยกตามตัวละครอยู่ที่ QuestData.characterStoryFlows)
// =========================================================

[System.Serializable]
public class StoryChapterRoute
{
    [Tooltip("เลขบท (QuestManager จะดึง route นี้เมื่อ GlobalQuestState.CurrentChapter ตรงกับค่านี้)")]
    public int chapterNumber = 1;
    [Tooltip("รายการเควสของบทนี้")]
    public List<QuestData> exclusiveQuestList = new List<QuestData>();
}

[CreateAssetMenu(fileName = "NewStoryProgress", menuName = "MissionSystem/StoryProgressData")]
public class StoryProgressData : ScriptableObject
{
    [Tooltip("แต่ละ Element = หนึ่งบท ใส่ chapterNumber ให้ตรงกับเลขบทที่ต้องการ (ไม่ต้องเรียงลำดับ)")]
    public List<StoryChapterRoute> routes = new List<StoryChapterRoute>();
}
