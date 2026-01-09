using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
public class MusicManager : BaseManager<MusicManager>
{
    private AudioSource bkMusic;
    private List<AudioSource> sounds = new List<AudioSource>();
    private float bkMusicVolume;
    private float soundVolume;

    // 音量大小属性
    public float BkMusicVolume
    {
        set
        {
            bkMusicVolume = value;
            if (bkMusic == null) return;
            bkMusic.volume = bkMusicVolume;
        }

        get => bkMusicVolume;
    }

    // 音效大小属性
    public float SoundVolume
    {
        set
        {
            soundVolume = value;
            foreach (AudioSource sound in sounds)
                sound.volume = soundVolume;
        }

        get => soundVolume;
    }

    /// <summary>
    /// 构造函数，通过MonoManager向Update中添加CheckStopSound事件
    /// </summary>
    public MusicManager()
    {
        MonoManager.Instance.AddEventListener(CheckStopSound, ETriggerTiming.Awake);
    }

    /// <summary>
    /// 传入背景音乐路径，播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayBkMusic(string name)
    {
        if (bkMusic == null)
        {
            GameObject musicObj = new GameObject("BkMusic");
            GameObject.DontDestroyOnLoad(musicObj);
            bkMusic = musicObj.AddComponent<AudioSource>();
        }

        LoadResourceManager.Instance.LoadResourcesAsync<AudioClip>(name, (clip) =>
        {
            bkMusic.clip = clip;
            bkMusic.loop = true;
            bkMusic.volume = BkMusicVolume;
            bkMusic.Play();
        });
    }

    /// <summary>
    /// 设置背景音乐是否静音
    /// </summary>
    /// <param name="isMute"></param>
    public void SetBkMusicMute(bool isMute)
    {
        if (bkMusic == null) return;

        bkMusic.mute = isMute;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name">音效路径</param>
    /// <param name="obj">挂载音效对象</param>
    /// <param name="isLoop">音效是否循环</param>
    /// <param name="callback">异步加载音效，通过传入回调函数获取加载音效</param>
    public void PlaySound(string name, GameObject obj, bool isLoop, UnityAction<AudioSource> callback = null)
    {
        LoadResourceManager.Instance.LoadResourcesAsync<AudioClip>(name, (clip) =>
        {
            AudioSource audioSource = obj.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.loop = isLoop;
            audioSource.volume = SoundVolume;
            audioSource.Play();
            sounds.Add(audioSource);
            callback?.Invoke(audioSource);
        });
    }

    /// <summary>
    /// 设置音效是否静音
    /// </summary>
    /// <param name="isMute"></param>
    public void SetSoundMute(bool isMute)
    {
        foreach (AudioSource sound in sounds)
        {
            sound.mute = isMute;
        }
    }

    /// <summary>
    /// 停止音效播放并移除
    /// </summary>
    /// <param name="sound"></param>
    public void StopSound(AudioSource sound)
    {
        if (sounds.Contains(sound))
        {
            GameObject.Destroy(sound);
            sounds.Remove(sound);
        }
    }

    /// <summary>
    /// 检测播放完毕的音效并移除
    /// </summary>
    public void CheckStopSound()
    {
        for (int i = sounds.Count - 1; i >= 0; --i)
        {
            if (!sounds[i].isPlaying)
            {
                GameObject.Destroy(sounds[i]);
                sounds.RemoveAt(i);
            }
        }
    }
}

**/