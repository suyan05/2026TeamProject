using System.Collections;
using UnityEngine;

public class SoundInstance : MonoBehaviour
{
    public string soundName = "TempAudio";
    public float volumeMultiple = 1;
    public AudioSource AS;

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
            volumeChangeCoroutine = StartCoroutine(ChangeVolume(value, duration));
        }
        else AS.volume = value * volumeMultiple;
    }

    IEnumerator ChangeVolume(float targetVolume, float duration)
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

    public void StopSound(string targetSoundName, float duration)
    {
        if (isStopping) return;

        if (targetSoundName == soundName)
        {
            isStopping = true;

            if (duration > 0)
            {
                StartCoroutine(StopSound(duration));
            }
            else
            {
                JUST_SHUT_THE_BUCK_UP();
            }
        }
    }

    IEnumerator StopSound(float duration)
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



    // 사운드를 당장 멈추지 않겠다? ㅋㅋ
    // 긴장해라 느그 몸뚱아리가 삼투압 현상이 뭔지 제대로 경험할 거다.
    public void JUST_SHUT_THE_BUCK_UP() => Destroy(gameObject);
}