using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text shuffleText;
    [SerializeField] private Image timerBarImage;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        UpdateShuffleUI();
        UpdateTimerBar();   
    }


    private void UpdateShuffleUI()
    {
        if (shuffleText != null)
        {
            shuffleText.text = $"{GameManager.Instance.ShuffleRemaining}";
        }
    }

    // Gọi hàm này từ Button OnClick trong Unity
    public void OnShuffleButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ManualShuffle();
        }
    }

    private void UpdateTimerBar()
    {
        timerBarImage.fillAmount = GameTimerManager.Instance.TimeNormalized;
    }
}
