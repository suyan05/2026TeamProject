using System.IO;
using UnityEditor;
using UnityEngine;

public static class WeaponImporterEditor
{
    private const string JsonPath = "Assets/Data/Json/weaponData.json";
    private const string WeaponFolder = "Assets/Data/ScriptableObjects/Weapons";
    private const string DatabasePath = "Assets/Data/ScriptableObjects/Databases/WeaponDatabase.asset";

    [MenuItem("Tools/Import Weapons From JSON")]
    public static void ImportWeapons()
    {
        TextAsset jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(JsonPath);

        if (jsonAsset == null)
        {
            Debug.LogError($"JSON 파일을 찾지 못했습니다: {JsonPath}");
            return;
        }

        EnsureFolderExists("Assets/Data");
        EnsureFolderExists("Assets/Data/ScriptableObjects");
        EnsureFolderExists("Assets/Data/ScriptableObjects/Weapons");
        EnsureFolderExists("Assets/Data/ScriptableObjects/Databases");

        WeaponJsonWrapper wrapper = JsonUtility.FromJson<WeaponJsonWrapper>(jsonAsset.text);

        if (wrapper == null || wrapper.weapons == null)
        {
            Debug.LogError("JSON 파싱 실패: weapons 데이터가 없습니다.");
            return;
        }

        WeaponDatabaseSO database = AssetDatabase.LoadAssetAtPath<WeaponDatabaseSO>(DatabasePath);

        if (database == null)
        {
            database = ScriptableObject.CreateInstance<WeaponDatabaseSO>();
            AssetDatabase.CreateAsset(database, DatabasePath);
        }

        database.weapons.Clear();

        foreach (WeaponJsonItem item in wrapper.weapons)
        {
            if (string.IsNullOrWhiteSpace(item.id))
            {
                Debug.LogWarning("ID가 비어 있는 무기를 건너뜁니다.");
                continue;
            }

            string safeFileName = $"{item.id}_{SanitizeFileName(item.nameEn)}.asset";
            string assetPath = $"{WeaponFolder}/{safeFileName}";

            WeaponSO weapon = AssetDatabase.LoadAssetAtPath<WeaponSO>(assetPath);

            if (weapon == null)
            {
                weapon = ScriptableObject.CreateInstance<WeaponSO>();
                AssetDatabase.CreateAsset(weapon, assetPath);
            }

            weapon.id = item.id;
            weapon.nameKr = item.nameKr;
            weapon.nameEn = item.nameEn;

            weapon.attribute = ParseAttribute(item.attribute);
            weapon.rarity = ParseRarity(item.rarity);
            weapon.attackSpeed = ParseSpeed(item.attackSpeed);

            weapon.attackMin = item.attackMin;
            weapon.attackMax = item.attackMax;
            weapon.feature = item.feature;
            weapon.description = item.description;

            EditorUtility.SetDirty(weapon);

            if (!database.weapons.Contains(weapon))
            {
                database.weapons.Add(weapon);
            }
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"무기 ScriptableObject 생성 완료: {database.weapons.Count}개");
    }

    private static WeaponAttribute ParseAttribute(string value)
    {
        return value switch
        {
            "Nature" => WeaponAttribute.Nature,
            "Wind" => WeaponAttribute.Wind,
            "Earth" => WeaponAttribute.Earth,
            "Fire" => WeaponAttribute.Fire,
            "Ice" => WeaponAttribute.Ice,
            "Lightning" => WeaponAttribute.Lightning,
            "Dark" => WeaponAttribute.Dark,
            _ => WeaponAttribute.Nature
        };
    }

    private static WeaponRarity ParseRarity(string value)
    {
        return value switch
        {
            "Common" => WeaponRarity.Common,
            "Rare" => WeaponRarity.Rare,
            "Epic" => WeaponRarity.Epic,
            "Unique" => WeaponRarity.Unique,
            _ => WeaponRarity.Common
        };
    }

    private static AttackSpeedType ParseSpeed(string value)
    {
        return value switch
        {
            "Slow" => AttackSpeedType.Slow,
            "Normal" => AttackSpeedType.Normal,
            "Fast" => AttackSpeedType.Fast,
            "Very Fast" => AttackSpeedType.VeryFast,
            "VeryFast" => AttackSpeedType.VeryFast,
            _ => AttackSpeedType.Normal
        };
    }

    private static void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
        string folderName = Path.GetFileName(folderPath);

        if (string.IsNullOrEmpty(parent))
            return;

        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolderExists(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "UnnamedWeapon";

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c.ToString(), "_");
        }

        return fileName.Trim();
    }
}