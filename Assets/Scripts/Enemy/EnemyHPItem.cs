using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHPItem : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Slider hpBar;

    public void SetHP(string enemyName, float current, float max)
    {
        nameText.text = enemyName;
        hpBar.value = current / max;
    }
}
