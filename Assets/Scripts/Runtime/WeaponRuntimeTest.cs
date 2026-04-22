using UnityEngine;

public class WeaponRuntimeTest : MonoBehaviour
{
    [SerializeField] private WeaponDatabaseSO weaponDatabase;
    [SerializeField] private string testWeaponId = "101";

    private void Start()
    {
        if (weaponDatabase == null)
        {
            Debug.LogError("WeaponDatabaseSO가 연결되지 않았습니다.");
            return;
        }

        WeaponSO weapon = weaponDatabase.GetWeaponById(testWeaponId);

        if (weapon == null)
        {
            Debug.LogWarning("해당 ID의 무기를 찾지 못했습니다: " + testWeaponId);
            return;
        }

        Debug.Log("===== Weapon SO Test =====");
        Debug.Log("ID: " + weapon.id);
        Debug.Log("KR Name: " + weapon.nameKr);
        Debug.Log("EN Name: " + weapon.nameEn);
        Debug.Log("Attribute: " + weapon.attribute);
        Debug.Log("Rarity: " + weapon.rarity);
        Debug.Log("Attack: " + weapon.attackMin + " ~ " + weapon.attackMax);
        Debug.Log("Speed: " + weapon.attackSpeed);
    }
}