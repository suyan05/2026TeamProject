using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public static List<SoundInstance> activeAudio = new List<SoundInstance>();
    [HideInInspector] public float volume { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        ResetActiveAudioVolume();
    }

    void ResetActiveAudioVolume()
    {
        foreach (var instance in activeAudio)
        {
            instance.SetVolume(volume);
        }
    }

    IEnumerator CleanupAudioSource(AudioSource AS, float delay)
    {
        yield return new WaitForSeconds(delay);
        //???
    }

    public SoundInstance Play3DSound
        (AudioClip clip, Vector3 point, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        return Playsound(clip, point, soundName, volumeMultiple, pitch, true, isLoop);
    }

    public SoundInstance PlayRandom3DSound
        (AudioClip[] clips, Vector3 point, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        AudioClip clip = GetRandomSound(clips);
        if (clip == null) return null;
        return Playsound(clip, point, soundName, volumeMultiple, pitch, true, isLoop);
    }

    public SoundInstance Play2DSound(AudioClip clip, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        return Playsound(clip, Vector3.zero, soundName, volumeMultiple, pitch, false, isLoop);
    }

    public SoundInstance PlayRandom2DSound(AudioClip[] clips, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        AudioClip clip = GetRandomSound(clips);
        if (clip == null) return null;
        return Playsound(clip, Vector3.zero, soundName, volumeMultiple, pitch, false, isLoop);
    }

    SoundInstance Playsound
        (AudioClip clip, Vector3 point, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool is3D = true, bool isLoop = false)
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
        soundInstance.SetVolume(volume);
        AS.clip = clip;
        AS.pitch = pitch;
        AS.spatialBlend = is3D ? 1.0f : 0.0f;
        AS.loop = isLoop;
        AS.Play();

        activeAudio.Add(soundInstance);

        if (!isLoop)
        {
            float soundDelay = clip.length + 0.3887f;
            StartCoroutine(CleanupAudioSource(AS, soundDelay));
            Object.Destroy(tempGO, soundDelay);
        }

        return soundInstance;
    }

    public void StopSound(string soundName = "", float duration = 0)
    {
        if (string.IsNullOrEmpty(soundName))
        {
            foreach (var instance in activeAudio)
            {
                instance.JUST_SHUT_THE_BUCK_UP();
            }
        }
        else
        {
            foreach (var instance in activeAudio)
            {
                instance.StopSound(soundName, duration);
            }
        }
    }

    public static AudioClip GetRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }

}