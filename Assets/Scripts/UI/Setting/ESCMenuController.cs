using UnityEngine;

public class ESCMenuController : MonoBehaviour
{
    public GameObject escMenu;
    public GameObject settingsMenu;

    bool isOpen = false;

    private void Start()
    {
        escMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // SettingsMenu가 열려있으면 ESC 누르면 ESCMenu로 돌아가기
            if (settingsMenu.activeSelf)
            {
                settingsMenu.SetActive(false);
                escMenu.SetActive(true);
                return;
            }

            ToggleESC();
        }
    }

    public void ToggleESC()
    {
        isOpen = !isOpen;

        escMenu.SetActive(isOpen);
        Time.timeScale = isOpen ? 0 : 1; // 일시정지 / 재개
    }

    public void OnClickContinue()
    {
        isOpen = false;
        escMenu.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnClickSettings()
    {
        escMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void OnClickExit()
    {
        Application.Quit();
    }
}
