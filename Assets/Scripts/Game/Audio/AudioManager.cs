using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{


    public static AudioManager Instance { get; private set; }

    public static List<SoundInstance> activeAudio = new List<SoundInstance>();
    public static Dictionary<string, List<SoundInstance>> activeAudioDictionary = new Dictionary<string, List<SoundInstance>>();

    [HideInInspector] public float bgmVolume = 1f;
    [HideInInspector] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        ResetActiveAudioVolume();
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        ResetActiveAudioVolume();
    }

    void ResetActiveAudioVolume()
    {
        foreach (var instance in activeAudio)
        {
            float baseVolume = instance.audioType == AudioType.BGM ? bgmVolume : sfxVolume;
            instance.SetVolume(baseVolume);
        }
    }

    public SoundInstance PlayBGM(AudioClip clip, bool loop = true)
    {
        var inst = Playsound(clip, Vector3.zero, "BGM", 1f, 1f, false, loop);
        inst.audioType = AudioType.BGM;
        return inst;
    }

    public SoundInstance PlaySFX(AudioClip clip, Vector3 pos)
    {
        var inst = Playsound(clip, pos, "SFX", 1f, 1f, true, false);
        inst.audioType = AudioType.SFX;
        return inst;
    }

    SoundInstance Playsound(AudioClip clip, Vector3 point, string soundName,
        float volumeMultiple, float pitch, bool is3D, bool isLoop)
    {
        if (clip == null) return null;

        var tempGO = new GameObject(soundName);
        tempGO.transform.position = point;
        tempGO.transform.SetParent(transform);

        SoundInstance soundInstance = tempGO.AddComponent<SoundInstance>();
        var AS = tempGO.AddComponent<AudioSource>();

        soundInstance.AS = AS;
        soundInstance.soundName = soundName;
        soundInstance.volumeMultiple = volumeMultiple;

        AS.clip = clip;
        AS.pitch = pitch;
        AS.spatialBlend = is3D ? 1f : 0f;
        AS.loop = isLoop;

        float baseVolume = soundInstance.audioType == AudioType.BGM ? bgmVolume : sfxVolume;
        soundInstance.SetVolume(baseVolume);

        AS.Play();

        activeAudio.Add(soundInstance);

        if (!activeAudioDictionary.ContainsKey(soundName))
            activeAudioDictionary[soundName] = new List<SoundInstance>();

        activeAudioDictionary[soundName].Add(soundInstance);

        if (!isLoop)
        {
            float soundDelay = clip.length + 0.3887f;
            Destroy(tempGO, soundDelay);
        }

        return soundInstance;
    }

    public void StopSound(string soundName = "", float duration = 0)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            foreach (var instance in activeAudio.ToList()) instance.StopSound(duration);
        }
        else if (activeAudioDictionary.TryGetValue(soundName, out List<SoundInstance> instances))
        {
            foreach (var instance in instances.ToList()) instance.StopSound(duration);
        }
    }
}
