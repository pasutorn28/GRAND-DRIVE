using UnityEngine;

/// <summary>
/// Attribute สำหรับกำหนดจำนวนทศนิยมใน Inspector
/// Usage: [DecimalPlaces(4)] public float myValue;
/// </summary>
public class DecimalPlacesAttribute : PropertyAttribute
{
    public int places;
    
    public DecimalPlacesAttribute(int decimalPlaces = 4)
    {
        places = decimalPlaces;
    }
}
