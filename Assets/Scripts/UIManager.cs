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
            controlsText.text = @"<b>Desktop</b>
  WASD / Arrow Keys - Rotate
  Right Mouse Drag - Rotate
  Scroll / Q/E - Zoom

<b>Touch Screen</b>
  Swipe - Rotate
  Pinch - Zoom";
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
            float timeRemaining = progress * resetInterval;
            timerText.text = $"Next Reset: {timeRemaining:F1}s";
        }
    }
}
