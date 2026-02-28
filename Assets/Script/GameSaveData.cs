using UnityEngine;
using System;

/// <summary>
/// ข้อมูลที่ใช้บันทึกสถานะเกม (ต้องมี [Serializable] เพื่อให้ JsonUtility แปลงเป็น JSON ได้)
/// </summary>
[Serializable]
public class GameSaveData
{
    // เวอร์ชันของรูปแบบข้อมูล (ถ้าเปลี่ยนโครงสร้างในอนาคต ใช้เช็คก่อนโหลด)
    public int saveVersion = 1;

    // ========== สถานะผู้เล่น (Character Status) ==========
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerRotY; // การหมุนแกน Y (หันซ้าย-ขวา)

    // ========== ทรัพย์สมบัติ ==========
    public int gold;

    // ========== ความคืบหน้าเนื้อเรื่อง (Story Progress) ==========
    /// <summary>ลำดับภารกิจปัจจุบัน (QuestManager.currentGlobalQuestIndex)</summary>
    public int currentGlobalQuestIndex;
    /// <summary>ชื่อตัวละครที่เลือกสำหรับภารกิจล่าสุด (สกิล/สถานะมาจาก CharacterData นี้)</summary>
    public string participatingCharacterName;

    // ฉากที่เล่นอยู่ (ชื่อ Scene)
    public string currentSceneName;

    // เวลาที่เซฟ (สำหรับแสดงใน UI โหลดเกม)
    public string saveTimeString;
}
