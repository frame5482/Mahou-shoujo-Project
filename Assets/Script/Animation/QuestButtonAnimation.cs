using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// สคริปต์จัดการอนิเมชันของปุ่มตัวละครใน QuestManager
/// </summary>
public class QuestButtonAnimation : MonoBehaviour
{
    [Header("--- ✨ ตั้งค่าอนิเมชันปุ่มตัวละคร ---")]
    [Tooltip("ความเร็วของอนิเมชันเมื่อปุ่มปรากฏ (วินาที)")]
    public float buttonAppearDuration = 0.3f;
    [Tooltip("ความเร็วของอนิเมชันเมื่อกดปุ่มส่งตัวละคร (วินาที)")]
    public float characterSendDuration = 0.3f;
    [Tooltip("ขนาดที่ปุ่มจะขยายเมื่อกด (เช่น 1.05 = ขยาย 5%)")]
    public float buttonClickScale = 1.05f;

    /// <summary>
    /// อนิเมชันเมื่อปุ่มปรากฏ (Scale + Fade In)
    /// </summary>
    public void PlayButtonAppearAnimation(GameObject buttonObj, float delay)
    {
        StartCoroutine(AnimateButtonAppear(buttonObj, delay));
    }

    /// <summary>
    /// อนิเมชันเมื่อกดปุ่มส่งตัวละครออกมา (Pulse + Fade Out)
    /// </summary>
    public void PlayCharacterSendAnimation(GameObject buttonObj, CharacterData character, System.Action<CharacterData> onComplete, GameObject characterSelectionPanel)
    {
        StartCoroutine(AnimateCharacterSend(buttonObj, character, onComplete, characterSelectionPanel));
    }

    // ✨ อนิเมชันเมื่อปุ่มปรากฏ (Scale + Fade In)
    IEnumerator AnimateButtonAppear(GameObject buttonObj, float delay)
    {
        yield return new WaitForSeconds(delay);

        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = buttonObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = buttonObj.AddComponent<CanvasGroup>();
        }

        // ตั้งค่าเริ่มต้น
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < buttonAppearDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonAppearDuration;
            
            // ใช้ Ease Out Back เพื่อให้มี bounce effect สวยงาม
            float easeOutBack = 1f - Mathf.Pow(1f - t, 3f);
            float bounce = 1f + (1f - easeOutBack) * 0.3f; // เพิ่ม bounce เล็กน้อย
            
            rectTransform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, easeOutBack * bounce);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            yield return null;
        }

        // ตั้งค่าสุดท้ายให้แน่ใจว่าเป็น 1
        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    // ✨ อนิเมชันเมื่อกดปุ่มส่งตัวละครออกมา (Pulse เล็กน้อย + Fade Out)
    IEnumerator AnimateCharacterSend(GameObject buttonObj, CharacterData character, System.Action<CharacterData> onComplete, GameObject characterSelectionPanel)
    {
        RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
        Image btnImage = buttonObj.GetComponent<Image>();
        Button clickedButton = buttonObj.GetComponent<Button>();
        Vector3 originalScale = rectTransform.localScale;
        Color originalColor = btnImage != null ? btnImage.color : Color.white;

        // ปิดการคลิกเฉพาะปุ่มที่กดไปแล้ว (ไม่ปิดปุ่มอื่นๆ)
        if (clickedButton != null)
        {
            clickedButton.interactable = false;
        }

        // Phase 1: Pulse เล็กน้อย (ขยายเล็กน้อยแล้วกลับ)
        float elapsed = 0f;
        float pulseDuration = characterSendDuration * 0.3f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pulseDuration;
            
            // Pulse: ขยายเล็กน้อยแล้วกลับมา
            float scale = 1f + (buttonClickScale - 1f) * Mathf.Sin(t * Mathf.PI);
            rectTransform.localScale = originalScale * scale;
            
            // เพิ่มความสว่างเล็กน้อย
            if (btnImage != null)
            {
                float brightness = 1f + 0.2f * Mathf.Sin(t * Mathf.PI);
                btnImage.color = originalColor * brightness;
            }
            
            yield return null;
        }

        // Phase 2: Fade Out (จางหาย)
        elapsed = 0f;
        float fadeDuration = characterSendDuration * 0.7f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            
            // คืนขนาดกลับมา
            rectTransform.localScale = originalScale;
            
            // จางหาย
            if (btnImage != null)
            {
                Color currentColor = originalColor;
                currentColor.a = Mathf.Lerp(1f, 0f, t);
                btnImage.color = currentColor;
            }
            
            yield return null;
        }

        // เรียก callback เพื่อส่งตัวละครจริงๆ
        if (onComplete != null)
        {
            onComplete(character);
        }

        // รอสักครู่แล้วคืนค่าปุ่มที่กดไปแล้ว (ถ้ายังเปิดอยู่)
        yield return new WaitForSeconds(0.1f);
        
        // ถ้ายังเปิดหน้าต่างเลือกตัวละครอยู่ และปุ่มยังอยู่ใน scene ให้เปิดปุ่มที่กดไปแล้วกลับมา
        if (clickedButton != null && clickedButton.gameObject != null && characterSelectionPanel != null && characterSelectionPanel.activeSelf)
        {
            clickedButton.interactable = true;
            // คืนค่าสีและขนาดกลับมา
            if (btnImage != null)
            {
                btnImage.color = originalColor;
            }
            rectTransform.localScale = originalScale;
        }
    }
}
