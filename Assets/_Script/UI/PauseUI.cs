using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private Button musicButton;
    [SerializeField] private TextMeshProUGUI musicButtonText;
    [SerializeField] private Button sfxButton;
    [SerializeField] private TextMeshProUGUI sfxButtonText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeMusicVolume();
            musicButtonText.text = MusicManager.Instance.GetMusicVolume() == 0 ? "Off" : MusicManager.Instance.GetMusicVolume().ToString();
        });
        sfxButton.onClick.AddListener(() => {
            SoundManager.Instance.ChangeSoundVolume();
            sfxButtonText.text = SoundManager.Instance.GetSoundVolume() == 0 ? "Off" : SoundManager.Instance.GetSoundVolume().ToString();
        });
        resumeButton.onClick.AddListener(() => {
            GameManager.Instance.UnPauseGame();
        });
        mainMenuButton.onClick.AddListener(() => {
            //Load scene main menu
            //SceneLoader.LoadScene(SceneLoader.Scene.MainMenuScene);
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGamePaused += GameManager_OnGamePaused;
        GameManager.Instance.OnGameUnPaused += GameManager_OnGameUnPaused;

        sfxButtonText.text = SoundManager.Instance.GetSoundVolume() == 0 ? "Off" : SoundManager.Instance.GetSoundVolume().ToString();
        musicButtonText.text = MusicManager.Instance.GetMusicVolume() == 0 ? "Off" : MusicManager.Instance.GetMusicVolume().ToString();
        Hide();
    }

    private void GameManager_OnGamePaused(object sender, EventArgs e)
    {
        Show();
    }

    private void GameManager_OnGameUnPaused(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
