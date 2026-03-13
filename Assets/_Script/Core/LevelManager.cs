using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event EventHandler OnLevelLoaded;

    [Header("References")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Levels")]
    [SerializeField] private LevelDataSO[] levels;
    [SerializeField] private int startLevelIndex = 0;

    [Header("Progress (PlayerPrefs)")]
    [SerializeField] private bool saveProgressToPlayerPrefs = true;
    [SerializeField] private string prefsKey_LastBonusGrantedLevel = "Pikachu.LastBonusGrantedLevel";
    [SerializeField] private string prefsKey_TotalShuffleBonusEarned = "Pikachu.TotalShuffleBonusEarned";

    private int currentLevelIndex;
    private int lastBonusGrantedLevel = -1;
    private int totalShuffleBonusEarned = 0;

    public int CurrentLevelIndex => currentLevelIndex;
    public LevelDataSO CurrentLevel => (levels != null && levels.Length > 0) ? levels[currentLevelIndex] : null;
    public int TotalShuffleBonusEarned => Mathf.Max(0, totalShuffleBonusEarned);

    private void Awake()
    {
        Instance = this;
        LoadProgress();
    }

    public int GetManualShuffleLimit(int baseManualShuffle, int maxCap)
    {
        int baseValue = Mathf.Max(0, baseManualShuffle);
        int cap = Mathf.Max(0, maxCap);
        return Mathf.Min(cap, baseValue + TotalShuffleBonusEarned);
    }

    public void MarkLevelCompleted(int levelIndex)
    {
        if (levels == null || levels.Length == 0) return;
        if (levelIndex < 0 || levelIndex >= levels.Length) return;

        // Bonus chỉ cộng 1 lần khi qua level tương ứng
        if (levelIndex <= lastBonusGrantedLevel) return;

        int bonus = Mathf.Max(0, levels[levelIndex].manualShuffleBonus);
        totalShuffleBonusEarned += bonus;
        lastBonusGrantedLevel = levelIndex;

        SaveProgress();
    }

    private void LoadProgress()
    {
        if (!saveProgressToPlayerPrefs) return;
        lastBonusGrantedLevel = PlayerPrefs.GetInt(prefsKey_LastBonusGrantedLevel, -1);
        totalShuffleBonusEarned = Mathf.Max(0, PlayerPrefs.GetInt(prefsKey_TotalShuffleBonusEarned, 0));
    }

    private void SaveProgress()
    {
        if (!saveProgressToPlayerPrefs) return;
        PlayerPrefs.SetInt(prefsKey_LastBonusGrantedLevel, lastBonusGrantedLevel);
        PlayerPrefs.SetInt(prefsKey_TotalShuffleBonusEarned, totalShuffleBonusEarned);
        PlayerPrefs.Save();
    }

    public void LoadStartLevel()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelManager: chưa cấu hình mảng levels!");
            return;
        }

        int clamped = Mathf.Clamp(startLevelIndex, 0, levels.Length - 1);
        LoadLevel(clamped);
    }

    public void LoadLevel(int index)
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelManager: chưa cấu hình mảng levels!");
            return;
        }

        if (index < 0 || index >= levels.Length)
        {
            Debug.LogError($"LevelManager: index level {index} không hợp lệ!");
            return;
        }

        currentLevelIndex = index;
        LevelDataSO level = levels[currentLevelIndex];

        // Grid
        GridManager.Instance.Initialize(level.cols, level.rows);
        GridManager.Instance.SetGravityMode(level.gravityMode);

        // Timer
        GameTimerManager.Instance.SetDuration(level.levelDuration);

        // Background
        if (backgroundRenderer != null && level.backgroundSprite != null)
        {
            backgroundRenderer.sprite = level.backgroundSprite;
        }

        if (level.bgm != null) MusicManager.Instance.PlayBGM(level.bgm);

        OnLevelLoaded?.Invoke(this, EventArgs.Empty);
    }
}