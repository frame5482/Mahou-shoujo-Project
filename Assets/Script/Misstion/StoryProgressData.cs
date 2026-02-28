using UnityEngine;
using System.Collections.Generic;

// =========================================================
// 📦 เก็บค่าแบบเดียวกับ CharacterQuestRoute ใน StoryFlowController
// แต่ละบท (เลข 1 = บทที่ 1) = ตัวละคร + รายการเควส
// เลข 1 จะเล่นบทที่ 1 → ดึง routes[0] มาใช้
// =========================================================

[System.Serializable]
public class StoryChapterRoute
{
    [Tooltip("ตัวละครของบทนี้ (เหมือน CharacterQuestRoute.targetCharacter)")]
    public CharacterData targetCharacter;
    [Tooltip("รายการเควสของบทนี้ (เหมือน exclusiveQuestList)")]
    public List<QuestData> exclusiveQuestList = new List<QuestData>();
}

[CreateAssetMenu(fileName = "NewStoryProgress", menuName = "MissionSystem/StoryProgressData")]
public class StoryProgressData : ScriptableObject
{
    [Tooltip("แต่ละ Element = บทที่ 1, บทที่ 2, ... (เลข 1 = Element 0)")]
    public List<StoryChapterRoute> routes = new List<StoryChapterRoute>();
}
