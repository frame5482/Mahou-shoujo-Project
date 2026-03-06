using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ตัวแสดงหนึ่งแผ่นเควส (Questpaper Prefab) มีรูป ชื่อ รายละเอียด
/// เมื่อกดจะเรียก QuestManager.OpenQuestDetail(questData, questIndex)
/// </summary>
[RequireComponent(typeof(Button))]
public class QuestPaperItem : MonoBehaviour
{
    [Header("--- UI อ้างอิง (ใส่ใน Prefab) ---")]
    public Image questImage;
    public TextMeshProUGUI questNameText;
    public TextMeshProUGUI questDetailText;

    QuestData _questData;
    int _questIndex;
    QuestManager _questManager;

    /// <summary> ใส่ข้อมูลเควสและ index ในบทปัจจุบัน แล้วอัปเดต UI </summary>
    public void Setup(QuestManager manager, QuestData data, int index)
    {
        _questManager = manager;
        _questData = data;
        _questIndex = index;

        if (questNameText != null && data != null)
        {
            questNameText.text = data.questName;
            GlobalQuestState.ApplyLanguageFont(questNameText);
        }
        if (questDetailText != null && data != null)
        {
            questDetailText.text = data.questDescription;
            GlobalQuestState.ApplyLanguageFont(questDetailText);
        }
        if (questImage != null && data != null)
        {
            if (data.questImage != null)
            {
                questImage.sprite = data.questImage;
                questImage.gameObject.SetActive(true);
            }
            else
            {
                questImage.gameObject.SetActive(false);
            }
        }

        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        if (_questManager != null && _questData != null)
            _questManager.OpenQuestDetail(_questData, _questIndex);
    }
}
