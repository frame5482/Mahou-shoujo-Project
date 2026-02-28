using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// โหลดข้อมูลอัตโนมัติจาก GlobalQuestState ทันทีที่เปิดฉากต่อสู้
/// และทำการเสกโมเดลตัวละคร (Prefab) พร้อมกองทัพมอนสเตอร์ลงสู่สมรภูมิ
/// </summary>
public class BattleQuestController : MonoBehaviour
{
    [Header("--- จุดเสก (Spawn Points) ---")]
    [Tooltip("จุดที่ผู้เล่นจะถูกย้าย/เสกมาเมื่อเริ่มเควสต่อสู้")]
    public Transform playerSpawnPoint;

    [Tooltip("รายการจุดเสกมอนสเตอร์ (ลำดับ index ตรงกับ MonsterSpawnEntry.spawnPointIndex ใน QuestData)")]
    public List<Transform> monsterSpawnPoints = new List<Transform>();

    [Header("--- อ้างอิง ---")]
    [Tooltip("ถ้าเว้นว่างไว้ ระบบจะพยายามค้นหาหรือจดจำตัวที่เพิ่งเสกมา")]
    public Player player;

    private int _pendingRewardGold;

    // ⚡ ทันทีที่โหลดมิติสมรภูมิเสร็จ ให้ทำงานทันที!
    void Start()
    {
        StartBattle();
    }

    /// <summary>
    /// เริ่มเควสต่อสู้: ดึงข้อมูลจาก GlobalQuestState แล้วเสกทุกอย่าง
    /// </summary>
    public void StartBattle()
    {
        // 🕊️ ล้วงเอาความทรงจำจาก "ผู้ส่งสารข้ามมิติ" มาใช้!
        QuestData quest = GlobalQuestState.ActiveQuest;
        CharacterData selectedCharacter = GlobalQuestState.SelectedCharacter;

        // เช็คว่ามีข้อมูลส่งมาจริงๆ หรือไม่
        if (quest == null || quest.questType != QuestType.Battle)
        {
            Debug.LogWarning("⚠️ [BattleQuestController] ไม่พบข้อมูลเควสต่อสู้ใน GlobalQuestState! (อาจจะเปิดฉากนี้ตรงๆ โดยไม่ผ่าน QuestManager)");
            return;
        }

        var config = quest.battleConfig;
        if (config == null)
        {
            Debug.LogError($"❌ [BattleQuestController] Quest '{quest.questName}' เป็น Battle แต่ battleConfig เป็น null!");
            return;
        }

        // กักเก็บรางวัลเตรียมรอจ่ายตอนชนะ
        _pendingRewardGold = config.rewardGold;

        // ---------------------------------------------------
        // 1. จัดการผู้เล่น (เสกกายหยาบจาก Prefab หรือวาร์ปตัวที่มี)
        // ---------------------------------------------------
        if (selectedCharacter != null && selectedCharacter.characterPrefab != null)
        {
            if (playerSpawnPoint != null)
            {
                // ⚡ มหาเวทเสกร่างอวตาร! อัญเชิญ Prefab ของทหารที่เลือกมาลงสู่สมรภูมิ!
                GameObject spawnedPlayer = Instantiate(selectedCharacter.characterPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

                // เชื่อมโยงจิตวิญญาณให้สภาบัญชาการรู้จักตัวที่เพิ่งเสก
                player = spawnedPlayer.GetComponent<Player>();

                Debug.Log($"✨ [BattleQuestController] อัญเชิญวีรชน: {selectedCharacter.characterName} ลงสู่สมรภูมิ ณ จุดเกิดสำเร็จ!");
            }
            else
            {
                Debug.LogWarning("⚠️ [BattleQuestController] ไม่มี playerSpawnPoint ใน Inspector! ไม่รู้จะเสกวีรชนไว้ที่ใด!");
            }
        }
        else
        {
            // 🛑 พลังสำรอง: หากไม่มี Prefab ให้ใช้วิชาวาร์ปตัวที่อยู่ในฉากมาแทน!
            if (player == null) player = FindObjectOfType<Player>();
            if (player != null && playerSpawnPoint != null)
            {
                // ปิด CharacterController หรือ Rigidbody ชั่วคราวก่อนวาร์ป (เพื่อป้องกันฟิสิกส์ต้านการวาร์ป)
                player.transform.position = playerSpawnPoint.position;
                player.transform.rotation = playerSpawnPoint.rotation;

                Debug.Log($"🌌 [BattleQuestController] ไม่พบ Prefab ประจำตัว! จึงทำการเคลื่อนย้ายร่างอวตารเดิมไปยังจุดเสก: {playerSpawnPoint.name}");
            }
            else if (playerSpawnPoint == null)
            {
                Debug.LogWarning("⚠️ [BattleQuestController] ไม่มี playerSpawnPoint ใน Inspector!");
            }
        }

        // ---------------------------------------------------
        // 2. อัญเชิญกองทัพมอนสเตอร์
        // ---------------------------------------------------
        if (config.monsterSpawns != null && config.monsterSpawns.Count > 0)
        {
            foreach (var entry in config.monsterSpawns)
            {
                if (entry.monsterPrefab == null) continue;
                if (entry.spawnPointIndex < 0 || entry.spawnPointIndex >= monsterSpawnPoints.Count)
                {
                    Debug.LogWarning($"⚠️ [BattleQuestController] spawnPointIndex {entry.spawnPointIndex} ไม่อยู่ในช่วง 0-{monsterSpawnPoints.Count - 1}");
                    continue;
                }

                Transform spawnT = monsterSpawnPoints[entry.spawnPointIndex];
                if (spawnT == null) continue;

                // วนลูปเสกมอนสเตอร์ตามจำนวนที่ระบุ (amount)
                for (int i = 0; i < entry.amount; i++)
                {
                    Vector3 randomOffset = Vector3.zero;
                    if (entry.amount > 1)
                    {
                        // สุ่มจุดเกิดไม่ให้ทับซ้อนกัน
                        Vector2 circleOffset = Random.insideUnitCircle * 2.0f;
                        randomOffset = new Vector3(circleOffset.x, 0f, circleOffset.y);
                    }

                    Vector3 finalSpawnPosition = spawnT.position + randomOffset;
                    GameObject monster = Instantiate(entry.monsterPrefab, finalSpawnPosition, spawnT.rotation);

                    Debug.Log($"👿 [BattleQuestController] อัญเชิญมอนสเตอร์: {entry.monsterPrefab.name} (ตัวที่ {i + 1}/{entry.amount})");
                }
            }
        }

        Debug.Log($"⚔️ [BattleQuestController] สมรภูมิพร้อมแล้ว! เควส: {quest.questName} | รางวัล: {_pendingRewardGold} G");
    }

    /// <summary>
    /// ให้รางวัลเงินที่เก็บไว้ (เรียกเมื่อผู้เล่นชนะการต่อสู้)
    /// </summary>
    public void GiveBattleReward()
    {
        if (_pendingRewardGold > 0 && ResourceManager.Instance != null)
        {
            ResourceManager.Instance.AddGold(_pendingRewardGold);
            Debug.Log($"💰 [BattleQuestController] มอบรางวัล {_pendingRewardGold} G");
            _pendingRewardGold = 0;
        }
    }

    /// <summary>
    /// เรียกเมื่อการต่อสู้จบ (ชนะ/จบ)
    /// </summary>
    public void OnBattleEnd()
    {
        GiveBattleReward();

        // ⚠️ คำเตือนจากราชครู: เมื่อจบการต่อสู้ใน Scene นี้ 
        // นายท่านอาจจะต้องใช้ SceneManager.LoadScene() กลับไปยังมิติหลัก (ฉากเมือง) ด้วยนะขอรับ!
        var qm = FindObjectOfType<QuestManager>();
        if (qm != null)
        {
            qm.OnQuestCompleted();
        }
    }
}