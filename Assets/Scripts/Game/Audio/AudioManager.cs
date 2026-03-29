using System.Collections.Generic;
using System.Linq;  // 오로지 ToList() 메서드를 사용하기 위함 (StopSound 메서드 참고)
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }   // 싱글톤 인스턴스
    public static List<SoundInstance> activeAudio = new List<SoundInstance>();  // 현재 재생 중인 사운드 인스턴스 목록
    public static Dictionary<string, List<SoundInstance>> activeAudioDictionary = new Dictionary<string, List<SoundInstance>>();   // 사운드 이름과 클립을 매핑하는 딕셔너리
    [HideInInspector] public float volume { get; private set; } // 전체 볼륨 (0.0f ~ 1.0f)

    private void Awake()    // 싱글톤 패턴
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 인수 : 볼륨 (0.0 ~ 1.0)
    /// </summary>
    public void SetVolume(float newVolume)  // 전체 볼륨 설정
    {
        volume = Mathf.Clamp01(newVolume);
        ResetActiveAudioVolume();
    }

    void ResetActiveAudioVolume()   // 현재 재생 중인 모든 사운드 인스턴스의 볼륨을 업데이트
    {
        foreach (var instance in activeAudio)
        {
            instance.SetVolume(volume);
        }
    }

    /// <summary>
    /// 인수 : 클립, 위치, 사운드 이름, 볼륨 배수, 피치, 루프 여부
    /// </summary>
    public SoundInstance Play3DSound    // 3D 사운드 재생
        (AudioClip clip, Vector3 point, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        return Playsound(clip, point, soundName, volumeMultiple, pitch, true, isLoop);
    }

    /// <summary>
    /// 인수 : 클립 배열, 위치, 사운드 이름, 볼륨 배수, 피치, 루프 여부
    /// </summary>
    public SoundInstance PlayRandom3DSound  // 3D 사운드 재생 (랜덤 클립)
        (AudioClip[] clips, Vector3 point, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        AudioClip clip = GetRandomSound(clips);
        if (clip == null) return null;
        return Playsound(clip, point, soundName, volumeMultiple, pitch, true, isLoop);
    }

    /// <summary>
    /// 인수 : 클립, 사운드 이름, 볼륨 배수, 피치, 루프 여부
    /// </summary>
    public SoundInstance Play2DSound    // 2D 사운드 재생
        (AudioClip clip, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        return Playsound(clip, Vector3.zero, soundName, volumeMultiple, pitch, false, isLoop);
    }

    /// <summary>
    /// 인수 : 클립 배열, 사운드 이름, 볼륨 배수, 피치, 루프 여부
    /// </summary>
    public SoundInstance PlayRandom2DSound  // 2D 사운드 재생 (랜덤 클립)
        (AudioClip[] clips, string soundName = "NamelessSound",
        float volumeMultiple = 1.0f, float pitch = 1.0f, bool isLoop = false)
    {
        AudioClip clip = GetRandomSound(clips);
        if (clip == null) return null;
        return Playsound(clip, Vector3.zero, soundName, volumeMultiple, pitch, false, isLoop);
    }

    SoundInstance Playsound // 사운드 재생의 공통 로직 (3D/2D 구분 없이)
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
        activeAudioDictionary[soundName].Add(soundInstance);

        if (!isLoop)
        {
            float soundDelay = clip.length + 0.3887f;
            Object.Destroy(tempGO, soundDelay);
        }

        return soundInstance;
    }

    /// <summary>
    /// 인수 : 사운드 이름, 페이드 아웃 시간
    /// </summary>
    public void StopSound(string soundName = "", float duration = 0)    // 사운드 정지 (soundName이 빈 문자열이면 모든 사운드 정지)
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

    /// <summary>
    /// 인수 : 클립 배열
    /// </summary>
    public static AudioClip GetRandomSound(AudioClip[] clips)   // 클립 배열에서 랜덤 클립 선택 (배열이 null이거나 비어있으면 null 반환)
    {
        if (clips == null || clips.Length == 0) return null;
        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }

}