using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "MissionSystem/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("--- ข้อมูลทั่วไป ---")]
    public string characterName; // ชื่อ เช่น Aria
    public Sprite portrait;      // รูปหน้าตัวละคร

    // ⚡ สิ่งที่เพิ่มเข้ามา: ภาชนะเนื้อแท้สำหรับลงสมรภูมิ!
    [Header("--- ภาชนะแห่งวิญญาณ (โมเดล 3D) ---")]
    [Tooltip("ลาก Prefab ตัวละครแบบ 3D มาใส่ตรงนี้เลยขอรับ!")]
    public GameObject characterPrefab;

    [Header("--- ค่าสถานะ (Stats) ---")]
    public int HP;
    public int VIT;
    public int STR;
    public int DEX;
    public int INT;
    public int CHA;
    public int LUCK;

    [Header("--- ความสามารถพิเศษ ---")]
    public List<string> skills; // เช่น "Stealth", "Magic"

    // ฟังก์ชันเช็คว่ามีสกิลไหม
    public bool HasSkill(string skillName)
    {
        return skills.Contains(skillName);
    }
}