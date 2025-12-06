using UnityEngine;
using UnityEditor;

/// <summary>
/// Property Drawer สำหรับ DecimalPlacesAttribute
/// แสดงทศนิยมตามจำนวนที่กำหนดใน Inspector
/// </summary>
[CustomPropertyDrawer(typeof(DecimalPlacesAttribute))]
public class DecimalPlacesDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        DecimalPlacesAttribute attr = (DecimalPlacesAttribute)attribute;
        
        if (property.propertyType == SerializedPropertyType.Float)
        {
            EditorGUI.BeginChangeCheck();
            
            // ใช้ DelayedFloatField เพื่อให้พิมพ์เสร็จก่อนค่อย apply
            float newValue = EditorGUI.DelayedFloatField(position, label, property.floatValue);
            
            if (EditorGUI.EndChangeCheck())
            {
                // Round ให้ตรงกับจำนวนทศนิยม
                float multiplier = Mathf.Pow(10, attr.places);
                property.floatValue = Mathf.Round(newValue * multiplier) / multiplier;
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
