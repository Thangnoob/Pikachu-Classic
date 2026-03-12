using System;
using UnityEngine;

public class GameTimerManager : MonoBehaviour
{
    public static GameTimerManager Instance { get; private set; }

    [SerializeField] private float levelDuration = 180f;     // tổng thời gian 1 màn (giây)
    private float timeRemaining;

    public event EventHandler OnTimeOver;
    public float TimeRemaining => timeRemaining;
    public float TimeNormalized => levelDuration > 0f ? Mathf.Clamp01(timeRemaining / levelDuration) : 0f;

    private void Update()
    {
        if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                OnTimeOver?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void Start()
    {
        Instance = this;
        GameManager.Instance.OnGameStart += GameManager_OnGameStart;
    }

    private void GameManager_OnGameStart(object sender, System.EventArgs e)
    {
        timeRemaining = levelDuration;
    }
}
