using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// จัดการบันทึกและโหลดสถานะเกม (ใช้ Application.persistentDataPath สำหรับทุกแพลตฟอร์ม)
/// </summary>
public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "save.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ไม่ให้ถูกทำลายเมื่อเปลี่ยนฉาก
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    // สำหรับมือถือ: บันทึกเมื่อเกมถูกพัก (กด Home หรือสลับแอป)
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    /// <summary>
    /// บันทึกสถานะเกมลงไฟล์ JSON
    /// </summary>
    public void SaveGame()
    {
        var data = CollectSaveData();
        data.saveTimeString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);

        Debug.Log($"💾 [GameSaveManager] บันทึกเกมสำเร็จ: {SavePath}");
    }

    /// <summary>
    /// โหลดสถานะเกมจากไฟล์ (ถ้ามี)
    /// </summary>
    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log($"📂 [GameSaveManager] ไม่พบไฟล์เซฟ จะเริ่มเกมใหม่");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<GameSaveData>(json);

        ApplySaveData(data);
        Debug.Log($"📂 [GameSaveManager] โหลดเกมสำเร็จ (เซฟเมื่อ: {data.saveTimeString})");
    }

    /// <summary>
    /// รวบรวมข้อมูลจาก Player, ResourceManager ฯลฯ มาใส่ใน GameSaveData
    /// </summary>
    private GameSaveData CollectSaveData()
    {
        var data = new GameSaveData();

        // จาก Player
        var player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            data.playerPosX = player.transform.position.x;
            data.playerPosY = player.transform.position.y;
            data.playerPosZ = player.transform.position.z;
            data.playerRotY = player.transform.eulerAngles.y;
        }

        // จาก ResourceManager
        if (ResourceManager.Instance != null)
        {
            data.gold = ResourceManager.Instance.currentGold;
        }

        // ฉากปัจจุบัน
        data.currentSceneName = SceneManager.GetActiveScene().name;

        // ความคืบหน้าเนื้อเรื่อง + ตัวละครที่เลือก (สกิลมาจาก CharacterData ของตัวละครนั้น)
        var questManager = FindAnyObjectByType<QuestManager>();
        var storyFlow = FindAnyObjectByType<StoryFlowController>();
        if (questManager != null)
            data.currentGlobalQuestIndex = questManager.currentGlobalQuestIndex;
        if (storyFlow != null)
            data.participatingCharacterName = storyFlow.GetParticipatingCharacterName();

        return data;
    }

    /// <summary>
    /// นำข้อมูลที่โหลดมาไปใช้กับ Player, ResourceManager
    /// </summary>
    private void ApplySaveData(GameSaveData data)
    {
        // ตรวจสอบเวอร์ชัน (สำหรับอนาคต)
        if (data.saveVersion != 1)
        {
            Debug.LogWarning($"[GameSaveManager] เวอร์ชันเซฟ ({data.saveVersion}) อาจไม่ตรงกับเกมปัจจุบัน");
        }

        // นำไปใช้กับ Player (เฉพาะเมื่ออยู่ฉากเดียวกับที่เซฟ)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == data.currentSceneName)
        {
            var player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                player.transform.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
                player.transform.eulerAngles = new Vector3(0f, data.playerRotY, 0f);
            }
        }
        else
        {
            Debug.Log($"[GameSaveManager] ฉากปัจจุบัน ({currentScene}) ไม่ตรงกับเซฟ ({data.currentSceneName}) - ไม่ย้ายตำแหน่งผู้เล่น");
        }

        // นำไปใช้กับ ResourceManager (ทองคำใช้ได้ทุกฉาก)
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.currentGold = data.gold;
       //     ResourceManager.Instance.UpdateUI();
        }

        // ความคืบหน้าเนื้อเรื่อง + ตัวละครที่เลือก (โหลดแล้วต้องเลือกตัวละครใหม่เมื่อเปิด Quest Board)
        var questManager = FindAnyObjectByType<QuestManager>();
        var storyFlow = FindAnyObjectByType<StoryFlowController>();
        if (questManager != null)
            questManager.currentGlobalQuestIndex = data.currentGlobalQuestIndex;
        if (storyFlow != null && !string.IsNullOrEmpty(data.participatingCharacterName) && questManager != null)
        {
            foreach (var ch in questManager.availableCharacters)
            {
                if (ch != null && ch.characterName == data.participatingCharacterName)
                {
                    storyFlow.SetCharacter(ch);
                    Debug.Log($"[GameSaveManager] โหลดตัวละคร: {data.participatingCharacterName} (สกิล/สถานะจาก CharacterData)");
                    break;
                }
            }
        }
    }
}
