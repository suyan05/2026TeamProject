using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    SerializedProperty itemTypeProp;

    SerializedProperty weaponTypeProp;
    SerializedProperty weaponAttackPowerProp;
    SerializedProperty weaponAttackSpeedProp;
    SerializedProperty elementTypeProp;

    SerializedProperty bonusMaxHpProp;
    SerializedProperty bonusBaseDamageProp;
    SerializedProperty bonusAttackSpeedProp;

    SerializedProperty equipPrefabProp;

    void OnEnable()
    {
        itemTypeProp = serializedObject.FindProperty("itemType");

        weaponTypeProp = serializedObject.FindProperty("weaponType");
        weaponAttackPowerProp = serializedObject.FindProperty("weaponAttackPower");
        weaponAttackSpeedProp = serializedObject.FindProperty("weaponAttackSpeed");
        elementTypeProp = serializedObject.FindProperty("elementType");

        bonusMaxHpProp = serializedObject.FindProperty("bonusMaxHp");
        bonusBaseDamageProp = serializedObject.FindProperty("bonusBaseDamage");
        bonusAttackSpeedProp = serializedObject.FindProperty("bonusAttackSpeed");

        equipPrefabProp = serializedObject.FindProperty("equipPrefab");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspectorExceptCustom();

        ItemType type = (ItemType)itemTypeProp.enumValueIndex;

        EditorGUILayout.Space();

        // -----------------------------
        // 무기 전용 스탯 + equipPrefab
        // -----------------------------
        if (type == ItemType.Weapon)
        {
            EditorGUILayout.LabelField("무기 전용 설정", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(equipPrefabProp, new GUIContent("장착용 프리팹"));
            EditorGUILayout.PropertyField(weaponTypeProp);
            EditorGUILayout.PropertyField(weaponAttackPowerProp);
            EditorGUILayout.PropertyField(weaponAttackSpeedProp);
            EditorGUILayout.PropertyField(elementTypeProp);
        }

        // -----------------------------
        // 아이템 전용 스탯
        // -----------------------------
        if (type == ItemType.Item)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("아이템 전용 스탯 보너스", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(bonusMaxHpProp);
            EditorGUILayout.PropertyField(bonusBaseDamageProp);
            EditorGUILayout.PropertyField(bonusAttackSpeedProp);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawDefaultInspectorExceptCustom()
    {
        SerializedProperty prop = serializedObject.GetIterator();
        prop.NextVisible(true);

        while (prop.NextVisible(false))
        {
            if (prop.name == "weaponType" ||
                prop.name == "weaponAttackPower" ||
                prop.name == "weaponAttackSpeed" ||
                prop.name == "elementType" ||
                prop.name == "bonusMaxHp" ||
                prop.name == "bonusBaseDamage" ||
                prop.name == "bonusAttackSpeed" ||
                prop.name == "equipPrefab")
                continue;

            EditorGUILayout.PropertyField(prop, true);
        }
    }
}
