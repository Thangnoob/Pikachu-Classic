using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnTileSelected;
    public event EventHandler OnMatchSuccess;
    public event EventHandler OnMatchFailure;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnPaused;

    public event EventHandler OnGameStart;

    [Header("References")]
    [SerializeField] private MatchPathfinder matchPathfinder;
    [SerializeField] private PathRenderer pathRenderer;

    [Header("Shuffle Settings")]
    [SerializeField] private int baseManualShuffle = 3;      // base = 3

    [Header("Gameplay Settings (runtime)")]
    [SerializeField] private int maxManualShuffle = 3;       // base + cumulative bonus theo level

    [SerializeField] private int manualShuffleCap = 7;       // giới hạn tối đa = 7

    private Tile firstSelected;

    private int shuffleRemaining;
    private bool isPlaying;

    public int ShuffleRemaining => shuffleRemaining;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameTimerManager.Instance.OnTimeOver += GameTimerManager_OnTimeOver;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelLoaded += LevelManager_OnLevelLoaded;
            LevelManager.Instance.LoadStartLevel();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy LevelManager. Dùng cấu hình mặc định của GameManager.");
            maxManualShuffle = Mathf.Max(0, baseManualShuffle);
        }

        StartLevel();
    }

    private void Update()
    {
        if (!isPlaying) return;
    }

    private void StartLevel()
    {
        OnGameStart?.Invoke(this, EventArgs.Empty);
        shuffleRemaining = maxManualShuffle;
        isPlaying = true;
    }

    private void LevelManager_OnLevelLoaded(object sender, EventArgs e)
    {
        ApplyLevelConfigForGameplay();
    }

    private void ApplyLevelConfigForGameplay()
    {
        if (LevelManager.Instance == null)
        {
            maxManualShuffle = Mathf.Min(Mathf.Max(0, manualShuffleCap), Mathf.Max(0, baseManualShuffle));
            return;
        }

        // base + bonus đã kiếm được (chỉ cộng 1 lần khi qua level) và có cap
        maxManualShuffle = LevelManager.Instance.GetManualShuffleLimit(baseManualShuffle, manualShuffleCap);
    }

    private void GameTimerManager_OnTimeOver(object sender, System.EventArgs e)
    {
        isPlaying = false;
        Debug.Log("Hết thời gian!");
        // Tùy bạn xử lý: hiện popup thua, dừng input, v.v.
    }

    public void OnTileClicked(Tile clicked)
    {
        if (!isPlaying) return;

        OnTileSelected?.Invoke(this, EventArgs.Empty);
        if (firstSelected == null)
        {
            firstSelected = clicked;
            firstSelected.SetSelected(true);
            Debug.Log($"Đã chọn tile: {clicked.GridPos}");
            return;
        }

        if (firstSelected == clicked)
        {
            firstSelected.SetSelected(false);
            firstSelected = null;
            return;
        }

        // tạm thời highlight tile thứ hai
        clicked.SetSelected(true);

        // Kiểm tra match
        if (matchPathfinder.TryGetPath(firstSelected, clicked, out var path))
        {
            Debug.Log("MATCH SUCCESS!");

            // Vẽ line đỏ theo path
            pathRenderer.DrawConnection(path);

            GridManager.Instance.RemoveTile(firstSelected);
            GridManager.Instance.RemoveTile(clicked);

            OnMatchSuccess?.Invoke(this, EventArgs.Empty);

            // Nếu hết cặp → shuffle đảm bảo còn đường
            if (!matchPathfinder.HasAnyValidPair())
            {
                Debug.Log("Hết cặp → Auto Shuffle...");
                GridManager.Instance.ShuffleGrid(true); // ← ensure valid pair
            }

            // Nếu muốn: kiểm tra thắng khi không còn tile nào
            if (GridManager.Instance.GetActiveTiles().Count == 0) { 
                isPlaying = false; 
                Debug.Log("Win!"); 

                // Cộng bonus đúng 1 lần khi qua level này, nhưng bị giới hạn bởi manualShuffleCap
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.MarkLevelCompleted(LevelManager.Instance.CurrentLevelIndex);
                    ApplyLevelConfigForGameplay();
                }
            }
        }
        else
        {
            OnMatchFailure?.Invoke(this, EventArgs.Empty);
            Debug.Log("Không nối được");
            // Nếu không nối được thì bỏ chọn tile thứ hai
            clicked.SetSelected(false);
        }

        if (firstSelected != null)
        {
            firstSelected.SetSelected(false);
        }

        firstSelected = null;
    }

    public void ManualShuffle()
    {
        if (!isPlaying) return;
        if (shuffleRemaining <= 0)
        {
            Debug.Log("Hết lượt shuffle!");
            return;
        }

        shuffleRemaining--;
        GridManager.Instance.ShuffleGrid(true);
        Debug.Log($"Shuffle thủ công, còn lại: {shuffleRemaining}");
    }

    private void PauseUnpauseGame()
    {
        if (Time.timeScale == 1f)
        {
            PauseGame();
        }
        else
        {
            UnPauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        OnGamePaused?.Invoke(this, EventArgs.Empty);
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1f;
        OnGameUnPaused?.Invoke(this, EventArgs.Empty);
    }

}