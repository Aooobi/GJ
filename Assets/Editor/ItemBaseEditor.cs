using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ItemBase))]//关联ItemBase脚本
public class ItemBaseEditor : Editor {

    private SerializedObject itemObject;//序列化对象
    private SerializedProperty nameProp, icon, description, maxStack, itemType, damage, health, function;//定义各种属性

    void OnEnable()
    {
        itemObject = new SerializedObject(target);
        nameProp = itemObject.FindProperty("name");
        icon = itemObject.FindProperty("icon");
        description = itemObject.FindProperty("description");
        maxStack = itemObject.FindProperty("maxStack");
        itemType = itemObject.FindProperty("itemType");
        damage = itemObject.FindProperty("damage");
        health = itemObject.FindProperty("health");
        function = itemObject.FindProperty("function");
    }

    public override void OnInspectorGUI()
    {
        itemObject.Update();//更新对象

        EditorGUILayout.PropertyField(nameProp, new GUIContent("物品名称"));
        EditorGUILayout.PropertyField(icon, new GUIContent("物品图标"));
        EditorGUILayout.PropertyField(description, new GUIContent("物品描述"));
        EditorGUILayout.PropertyField(maxStack, new GUIContent("堆叠数量"));
        EditorGUILayout.PropertyField(itemType, new GUIContent("物品类型"));

        // 根据选择的枚举类型显示不同的字段
        if (itemType.enumValueIndex == (int)ItemBase.ItemType.装备) { // 装备显示血量
            EditorGUILayout.PropertyField(health, new GUIContent("血量"));
        }
        else if (itemType.enumValueIndex == (int)ItemBase.ItemType.武器) { // 武器显示伤害
            EditorGUILayout.PropertyField(damage, new GUIContent("伤害"));
        }
        else if (itemType.enumValueIndex == (int)ItemBase.ItemType.道具) { // 道具显示功能
            EditorGUILayout.PropertyField(function, new GUIContent("功能"));
        }
        // 其他类型不显示额外内容

        itemObject.ApplyModifiedProperties();//应用修改
    }
}