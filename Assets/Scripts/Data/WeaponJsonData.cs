using System;
using System.Collections.Generic;

[Serializable]
public class WeaponJsonItem
{
    public string id;
    public string nameKr;
    public string nameEn;
    public string attribute;
    public string rarity;
    public int attackMin;
    public int attackMax;
    public string attackSpeed;
    public string feature;
    public string description;
}

[Serializable]
public class WeaponJsonWrapper
{
    public List<WeaponJsonItem> weapons;
}