using TMPro;
using UnityEngine;

/// <summary>
/// Displays the remaining escape time.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class TimerDisplay : MonoBehaviour
{
    private TMP_Text timerText;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnTimerUpdated += UpdateTimerText;
        UpdateTimerText(GameManager.Instance.TimeRemaining);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimerUpdated -= UpdateTimerText;
        }
    }

    private void UpdateTimerText(float secondsRemaining)
    {
        int totalSeconds = Mathf.CeilToInt(secondsRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}