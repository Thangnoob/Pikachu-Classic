using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnTileSelected;
    public event EventHandler OnMatchSuccess;
    public event EventHandler OnMatchFailure;

    public event EventHandler OnGameStart;

    [Header("References")]
    [SerializeField] private MatchPathfinder matchPathfinder;
    [SerializeField] private PathRenderer pathRenderer;

    [Header("Gameplay Settings")]
    [SerializeField] private int maxManualShuffle = 3;       // số lần shuffle người chơi được dùng

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

        GridManager.Instance.Initialize(16, 9);
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

    
}