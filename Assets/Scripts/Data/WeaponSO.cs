using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Game Data/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string id;
    public string nameKr;
    public string nameEn;

    public WeaponAttribute attribute;
    public WeaponRarity rarity;
    public AttackSpeedType attackSpeed;

    public int attackMin;
    public int attackMax;

    public string feature;
    public string description;
}