using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI controlsText;

    [SerializeField] public RectTransform progressBarFill;

    public TextMeshProUGUI timerText;

    private float originalWidth = -1f;

    private void Awake()
    {
        if (progressBarFill == null)
        {
            Debug.LogWarning("Progress Bar Fill is not assigned in UIManager.");
            return;
        }
        originalWidth = progressBarFill.rect.width;
    }

    private void Start()
    {
        UpdateControlsText();
    }

    private void UpdateControlsText()
    {
        if (controlsText != null)
        {
            controlsText.text = @"<b>Camera Controls:</b>
<b>WASD</b> - Move Forward/Back/Left/Right
<b>Q/E</b> - Move Down/Up
<b>Shift/Space</b> - Move Down/Up
<b>Arrow Keys</b> - Rotate View
<b>Ctrl+Mouse</b> - Rotate View";
        }
    }

    /// 更新进度条和倒计时文本
    /// Update the progress bar and timer text
    /// progress: 0 ~ 1
    /// resetInterval: seconds of an episode
    public void UpdateProgressBar(float progress, float resetInterval)
    {
        progress = Mathf.Clamp01(progress);
        progress = 1f - progress;  // 反转进度条 (Reverse the progress bar)

        if (progressBarFill != null && originalWidth > 0f)
        {
            float newWidth = originalWidth * progress;
            progressBarFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }

        if (timerText != null)
        {
            float timeRemaining = (1f - progress) * resetInterval;
            timerText.text = $"Next Reset: {timeRemaining:F1}s";
        }
    }
}
