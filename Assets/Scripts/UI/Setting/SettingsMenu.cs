using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SettingsMenu : MonoBehaviour
{
    [Header("Volume")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Toggle vsyncToggle;

    [Header("Key Binding Button Parent")]
    public Transform keyButtonParent; // 버튼 자동 생성 위치
    public GameObject keyButtonPrefab; // 버튼 프리팹

    Dictionary<string, System.Action> rebindActions = new Dictionary<string, System.Action>();

    Resolution[] resolutions;

    private void Awake()
    {
        SetupVolume();
        SetupGraphics();
        SetupKeyBindingButtons();
    }

    // ============================
    // 볼륨
    // ============================
    void SetupVolume()
    {
        bgmSlider.value = AudioManager.Instance.bgmVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;

        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }

    // ============================
    // 그래픽
    // ============================
    void SetupGraphics()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
                currentIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string> { "Low", "Medium", "High", "Ultra" });
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.onValueChanged.AddListener(SetQuality);

        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
        vsyncToggle.onValueChanged.AddListener(SetVSync);
    }

    // ============================
    // 키 바인딩 버튼 자동 생성
    // ============================
    void SetupKeyBindingButtons()
    {
        rebindActions.Clear();
        Debug.Log("버튼 생성 함수 실행됨");
        rebindActions.Add("Weapon1", RebindWeapon1);
        rebindActions.Add("Weapon2", RebindWeapon2);
        rebindActions.Add("Skill1", RebindSkill1);
        rebindActions.Add("Skill2", RebindSkill2);
        rebindActions.Add("Inventory", RebindInventory);
        rebindActions.Add("Left", RebindLeft);
        rebindActions.Add("Right", RebindRight);
        rebindActions.Add("Jump", RebindJump);
        rebindActions.Add("Roll", RebindRoll);

        foreach (var pair in rebindActions)
        {
            GameObject btnObj = Instantiate(keyButtonPrefab, keyButtonParent);
            TMP_Text label = btnObj.transform.Find("Label").GetComponent<TMP_Text>();
            TMP_Text keyText = btnObj.transform.Find("KeyText").GetComponent<TMP_Text>();
            Button btn = btnObj.GetComponent<Button>();

            label.text = pair.Key;
            keyText.text = GetKeyDisplay(pair.Key);

            btn.onClick.AddListener(() =>
            {
                pair.Value.Invoke();
            });
        }
    }

    string GetKeyDisplay(string keyName)
    {
        var kb = KeyBindingManager.Instance;

        return keyName switch
        {
            "Weapon1" => kb.weapon1IsMouse ? kb.weapon1Mouse.ToString() : kb.weapon1Key.ToString(),
            "Weapon2" => kb.weapon2IsMouse ? kb.weapon2Mouse.ToString() : kb.weapon2Key.ToString(),
            "Skill1" => kb.skill1Key.ToString(),
            "Skill2" => kb.skill2Key.ToString(),
            "Inventory" => kb.inventory.ToString(),
            "Left" => kb.leftKey.ToString(),
            "Right" => kb.rightKey.ToString(),
            "Jump" => kb.jumpKey.ToString(),
            "Roll" => kb.rollKey.ToString(),
            _ => "None"
        };
    }

    // ============================
    // 그래픽 설정 함수들
    // ============================
    void SetResolution(int index)
    {
        Resolution r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
    }

    void SetFullscreen(bool full)
    {
        Screen.fullScreen = full;
    }

    void SetVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }

    // ============================
    // 리바인딩 함수들
    // ============================
    public void RebindWeapon1()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            var kb = KeyBindingManager.Instance;
            kb.weapon1IsMouse = isMouse;
            kb.weapon1Key = key;
            kb.weapon1Mouse = mouseKey;

            RefreshUI();
        });
    }

    public void RebindWeapon2()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            var kb = KeyBindingManager.Instance;
            kb.weapon2IsMouse = isMouse;
            kb.weapon2Key = key;
            kb.weapon2Mouse = mouseKey;

            RefreshUI();
        });
    }

    public void RebindSkill1()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.skill1Key = key;
            RefreshUI();
        });
    }

    public void RebindSkill2()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.skill2Key = key;
            RefreshUI();
        });
    }

    public void RebindInventory()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.inventory = key;
            RefreshUI();
        });
    }

    public void RebindLeft()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.leftKey = key;
            RefreshUI();
        });
    }

    public void RebindRight()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.rightKey = key;
            RefreshUI();
        });
    }

    public void RebindJump()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.jumpKey = key;
            RefreshUI();
        });
    }

    public void RebindRoll()
    {
        KeyBindingManager.Instance.StartRebind((key, isMouse, mouseKey) =>
        {
            KeyBindingManager.Instance.rollKey = key;
            RefreshUI();
        });
    }

    void RefreshUI()
    {
        foreach (Transform child in keyButtonParent)
            Destroy(child.gameObject);

        SetupKeyBindingButtons();
    }
}
