using UnityEngine;
using TMPro;

// =========================================================
// ฟอนต์ต่อภาษา (ใส่ฟอนต์เองใน Inspector)
// ใช้ร่วมกับ GlobalQuestState.CurrentLanguage
// =========================================================
[CreateAssetMenu(fileName = "LanguageFontSettings", menuName = "Settings/LanguageFontSettings")]
public class LanguageFontSettings : ScriptableObject
{
    [Tooltip("0 = ENG")]
    public TMP_FontAsset fontEnglish;
    [Tooltip("1 = THAI")]
    public TMP_FontAsset fontThai;
    [Tooltip("2 = JP")]
    public TMP_FontAsset fontJapanese;

    /// <summary> ใส่ฟอนต์ของภาษาปัจจุบันให้ข้อความ (อิง GlobalQuestState.CurrentLanguage) </summary>
    public void ApplyTo(TextMeshProUGUI text)
    {
        if (text == null) return;
        switch (GlobalQuestState.CurrentLanguage)
        {
            case 0: if (fontEnglish != null) text.font = fontEnglish; break;
            case 1: if (fontThai != null) text.font = fontThai; break;
            case 2: if (fontJapanese != null) text.font = fontJapanese; break;
            default: if (fontThai != null) text.font = fontThai; break;
        }
    }
}
