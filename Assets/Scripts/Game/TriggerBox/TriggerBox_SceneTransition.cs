using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerBox_SceneTransition : MonoBehaviour, ITriggerBox
{
    [Header("Scene Transition")]
    public bool loadThisScene = false;
    public string customSceneName;
    [Header("Fade")]
    public float standbyTime = 0f;
    public float fadeOutDuration = 1f;
    public float fadeInDuration = 1f;
    [Header("Trigger")]
    public bool destroyAfterTrigger = true;

    bool canTrigger = true;
    string targetSceneName;

    public void TriggerIn()
    {
        if (!canTrigger) return;
        canTrigger = false;

        targetSceneName = loadThisScene ? SceneManager.GetActiveScene().name : customSceneName;

        if (DoesSceneExist(targetSceneName)) StartCoroutine(ScreenTransition());
        else Debug.LogWarning($"Scene \"{targetSceneName}\" 이 존재하지 않음");
    }

    public void TriggerOut() { }

    IEnumerator ScreenTransition()
    {
        DontDestroyOnLoad(gameObject);
        //standby
        yield return new WaitForSeconds(standbyTime);
        // Fade out
        UIManager.Instance.SetCurtainToggle(true, fadeOutDuration);
        yield return new WaitForSeconds(fadeOutDuration + 0.25f);
        // Load the new scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        // Fade in
        UIManager.Instance.SetCurtainToggle(false, fadeInDuration, 0.25f);

        if (destroyAfterTrigger) Destroy(gameObject);
        else this.enabled = false;
    }

    /// <summary>
    /// 빌드 설정에 특정 이름의 씬이 존재하는지 확인
    /// </summary>
    /// <param name="sceneName">찾고자 하는 Scene 이름</param>
    /// <returns>sceneName을 이름으로 하는 Scene이 존재하면 true</returns>
    public bool DoesSceneExist(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string nameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (nameFromPath == sceneName) return true;
        }

        return false;
    }
}
