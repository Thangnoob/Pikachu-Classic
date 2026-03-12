using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private const int MUSIC_VOLUME_MAX = 10;

    public static MusicManager Instance { get; private set; }

    private static int musicVolume = 6;
    private static float musicTime;

    private AudioSource musicAudioSource;

    private void Awake()
    {
        Instance = this;
        musicAudioSource = GetComponent<AudioSource>();
        musicAudioSource.time = musicTime;
    }

    private void Update()
    {
        musicTime = musicAudioSource.time;
    }

    private void Start()
    {
        musicAudioSource.volume = GetMusicVolumeNormalized();
    }

    private void ChangeMusicVolume()
    {
        musicVolume = (musicVolume + 1) % MUSIC_VOLUME_MAX; 
        musicAudioSource.volume = GetMusicVolumeNormalized();   
    }

    public int GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetMusicVolumeNormalized()
    {
        return ((float)musicVolume) / MUSIC_VOLUME_MAX;
    }
}
