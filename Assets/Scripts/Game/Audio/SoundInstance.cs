using System.Collections;
using UnityEngine;

public enum AudioType
{
    BGM,
    SFX
}


public class SoundInstance : MonoBehaviour
{
    public string soundName = "TempAudio";
    public float volumeMultiple = 1;
    public AudioSource AS;

    public AudioType audioType = AudioType.SFX;

    Transform followTarget;
    bool isStopping = false;
    bool isFollowingTarget = false;
    Coroutine volumeChangeCoroutine;

    private void Update()
    {
        if (isFollowingTarget)
        {
            if (followTarget != null) transform.position = followTarget.position;
            else isFollowingTarget = false;
        }
    }

    public void FollowObject(Transform target)
    {
        followTarget = target;
        isFollowingTarget = true;
    }

    public void StopFollowObject() => isFollowingTarget = false;

    public void SetVolume(float value, float duration = 0f)
    {
        if (isStopping) return;

        if (duration > 0)
        {
            if (volumeChangeCoroutine != null) StopCoroutine(volumeChangeCoroutine);
            volumeChangeCoroutine = StartCoroutine(SetVolumeCoroutine(value, duration));
        }
        else AS.volume = value * volumeMultiple;
    }

    IEnumerator SetVolumeCoroutine(float targetVolume, float duration)
    {
        float time = 0;
        float startVolume = AS.volume;

        while (time < duration)
        {
            AS.volume = Mathf.Lerp(startVolume, targetVolume * volumeMultiple, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        AS.volume = targetVolume * volumeMultiple;
        volumeChangeCoroutine = null;
    }

    public void SetNewVolumeMultiple(float newVolumeMultipleValue) => volumeMultiple = newVolumeMultipleValue;

    public void StopSound(float duration)
    {
        if (isStopping) return;

        isStopping = true;
        if (duration > 0) StartCoroutine(StopSoundCoroutine(duration));
        else JUST_SHUT_THE_BUCK_UP();
    }

    IEnumerator StopSoundCoroutine(float duration)
    {
        float time = 0;
        float startVolume = AS.volume;

        while (time < duration)
        {
            AS.volume = Mathf.Lerp(startVolume, 0, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        JUST_SHUT_THE_BUCK_UP();
    }

    public void JUST_SHUT_THE_BUCK_UP() => Destroy(gameObject);
}
