using System.Collections;
using UnityEngine;

public class SoundInstance : MonoBehaviour
{
    public string soundName = "TempAudio";  // 사운드 이름 (AudioManager에서 관리할 때 사용)
    public float volumeMultiple = 1;    // 볼륨 배수 (AudioManager의 전체 볼륨과 곱해서 최종 볼륨 결정)
    public AudioSource AS;  

    Transform followTarget; // 따라다닐 대상
    bool isStopping = false;    // 사운드가 멈추는 중인지 여부
    bool isFollowingTarget = false; // 따라다니는 중인지 여부
    Coroutine volumeChangeCoroutine;    // 볼륨 변화 코루틴 참조

    private void Update()
    {
        if (isFollowingTarget)  // 따라다니는 중이면 위치 업데이트
        {
            if (followTarget != null) transform.position = followTarget.position;
            else isFollowingTarget = false;
        }
    }

    public void FollowObject(Transform target)  // 특정 오브젝트를 따라다니게 설정
    {
        followTarget = target;
        isFollowingTarget = true;
    }

    public void StopFollowObject() => isFollowingTarget = false;    // 따라다니는 거 멈춰라

    public void SetVolume(float value, float duration = 0f) // 볼륨 설정 (duration이 0보다 크면 서서히 변화)
    {
        if (isStopping) return;

        if (duration > 0)
        {
            if (volumeChangeCoroutine != null) StopCoroutine(volumeChangeCoroutine);
            volumeChangeCoroutine = StartCoroutine(SetVolumeCoroutine(value, duration));
        }
        else AS.volume = value * volumeMultiple;
    }

    IEnumerator SetVolumeCoroutine(float targetVolume, float duration)    // SetVolume에서 호출하는 볼륨을 서서히 변화시키는 코루틴
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

    public void SetNewVolumeMultiple(float newVolumeMultipleValue) => volumeMultiple = newVolumeMultipleValue;  // 볼륨 배수 설정 (AudioManager에서 전체 볼륨이 바뀌었을 때 이걸로 개별 사운드의 볼륨 재설정)

    public void StopSound(string targetSoundName, float duration)   // 사운드 정지 (targetSoundName이 자기 이름이랑 같으면 멈춤, duration이 0보다 크면 페이드 아웃)
    {
        if (isStopping) return;

        if ((targetSoundName == soundName) || string.IsNullOrEmpty(targetSoundName))
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

    IEnumerator StopSound(float duration)   // StopSound에서 호출하는 사운드를 서서히 줄여서 멈추는 코루틴
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