using UnityEngine;

public enum MouseKey
{
    None = -1,
    Left = 0,
    Right = 1,
    Middle = 2
}

/// <summary>
/// 키 리바인딩을 관리하는 매니저
/// SettingsMenu에서 키를 변경하면 PlayerMovement가 자동으로 반영됨
/// </summary>

public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance;

    // 공격키 (마우스/키보드 혼합 지원)
    public bool weapon1IsMouse = true;
    public MouseKey weapon1Mouse = MouseKey.Left;
    public KeyCode weapon1Key = KeyCode.None;

    public bool weapon2IsMouse = true;
    public MouseKey weapon2Mouse = MouseKey.Right;
    public KeyCode weapon2Key = KeyCode.None;

    // 기타 키
    public KeyCode skill1Key = KeyCode.E;
    public KeyCode skill2Key = KeyCode.Q;
    public KeyCode inventory = KeyCode.Tab;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode rollKey = KeyCode.LeftShift;

    KeyCode waitingKey = KeyCode.None;
    System.Action<KeyCode, bool, MouseKey> onKeySet;

    void Awake()
    {
        Instance = this;
        LoadKeyBindings();
    }

    void Update()
    {
        if (waitingKey != KeyCode.None)
        {
            // ============================
            // 키보드 입력 체크
            // ============================
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    onKeySet?.Invoke(key, false, MouseKey.None);
                    waitingKey = KeyCode.None;
                    SaveKeyBindings();
                    return;
                }
            }

            // ============================
            // Tab 직접 체크 (Unity가 Enum 순회에서 놓치는 경우 방지)
            // ============================
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                onKeySet?.Invoke(KeyCode.Tab, false, MouseKey.None);
                waitingKey = KeyCode.None;
                SaveKeyBindings();
                return;
            }

            // ============================
            // 마우스 입력 체크
            // ============================
            if (Input.GetMouseButtonDown(0))
            {
                onKeySet?.Invoke(KeyCode.None, true, MouseKey.Left);
                waitingKey = KeyCode.None;
                SaveKeyBindings();
                return;
            }
            if (Input.GetMouseButtonDown(1))
            {
                onKeySet?.Invoke(KeyCode.None, true, MouseKey.Right);
                waitingKey = KeyCode.None;
                SaveKeyBindings();
                return;
            }
            if (Input.GetMouseButtonDown(2))
            {
                onKeySet?.Invoke(KeyCode.None, true, MouseKey.Middle);
                waitingKey = KeyCode.None;
                SaveKeyBindings();
                return;
            }
        }
    }

    public void StartRebind(System.Action<KeyCode, bool, MouseKey> callback)
    {
        waitingKey = KeyCode.Backspace;
        onKeySet = callback;
    }

    // ============================
    // 저장
    // ============================
    public void SaveKeyBindings()
    {
        PlayerPrefs.SetInt("weapon1IsMouse", weapon1IsMouse ? 1 : 0);
        PlayerPrefs.SetInt("weapon1Mouse", (int)weapon1Mouse);
        PlayerPrefs.SetInt("weapon1Key", (int)weapon1Key);

        PlayerPrefs.SetInt("weapon2IsMouse", weapon2IsMouse ? 1 : 0);
        PlayerPrefs.SetInt("weapon2Mouse", (int)weapon2Mouse);
        PlayerPrefs.SetInt("weapon2Key", (int)weapon2Key);

        PlayerPrefs.SetInt("skill1Key", (int)skill1Key);
        PlayerPrefs.SetInt("skill2Key", (int)skill2Key);
        PlayerPrefs.SetInt("inventory", (int)inventory);
        PlayerPrefs.SetInt("leftKey", (int)leftKey);
        PlayerPrefs.SetInt("rightKey", (int)rightKey);
        PlayerPrefs.SetInt("jumpKey", (int)jumpKey);
        PlayerPrefs.SetInt("rollKey", (int)rollKey);

        PlayerPrefs.Save();
    }

    // ============================
    // 불러오기
    // ============================
    public void LoadKeyBindings()
    {
        weapon1IsMouse = PlayerPrefs.GetInt("weapon1IsMouse", 1) == 1;
        weapon1Mouse = (MouseKey)PlayerPrefs.GetInt("weapon1Mouse", 0);
        weapon1Key = (KeyCode)PlayerPrefs.GetInt("weapon1Key", (int)KeyCode.None);

        weapon2IsMouse = PlayerPrefs.GetInt("weapon2IsMouse", 1) == 1;
        weapon2Mouse = (MouseKey)PlayerPrefs.GetInt("weapon2Mouse", 1);
        weapon2Key = (KeyCode)PlayerPrefs.GetInt("weapon2Key", (int)KeyCode.None);

        skill1Key = (KeyCode)PlayerPrefs.GetInt("skill1Key", (int)KeyCode.E);
        skill2Key = (KeyCode)PlayerPrefs.GetInt("skill2Key", (int)KeyCode.Q);
        inventory = (KeyCode)PlayerPrefs.GetInt("inventory", (int)KeyCode.Tab);
        leftKey = (KeyCode)PlayerPrefs.GetInt("leftKey", (int)KeyCode.A);
        rightKey = (KeyCode)PlayerPrefs.GetInt("rightKey", (int)KeyCode.D);
        jumpKey = (KeyCode)PlayerPrefs.GetInt("jumpKey", (int)KeyCode.Space);
        rollKey = (KeyCode)PlayerPrefs.GetInt("rollKey", (int)KeyCode.LeftShift);
    }
}