using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบ Stats ของตัวละคร
/// Character Stats System - PWR/CTL/SPN/CRV affect swing mechanics
/// </summary>
[System.Serializable]
public class CharacterStats : MonoBehaviour
{
    [Header("--- Base Stats (1-50 scale) ---")]
    [Tooltip("PWR (Power) - พลังตี ยิ่งสูงระยะยิ่งไกล")]
    [Range(1, 50)] public int power = 25;
    
    [Tooltip("CTL (Control) - ความแม่นยำ ยิ่งสูง Perfect Zone ยิ่งกว้าง")]
    [Range(1, 50)] public int control = 25;
    
    [Tooltip("SPN (Spin) - การหมุน ยิ่งสูง spin effect ยิ่งแรง")]
    [Range(1, 50)] public int spin = 25;
    
    [Tooltip("CRV (Curve) - การเลี้ยว ยิ่งสูง Magnus effect ยิ่งแรง")]
    [Range(1, 50)] public int curve = 25;

    [Header("--- Stat Effects ---")]
    [Tooltip("โบนัสระยะต่อ PWR point (yards)")]
    public float powerDistanceBonus = 2f;  // +2 yards per PWR point
    
    [Tooltip("โบนัส Perfect Zone ต่อ CTL point")]
    public float controlZoneBonus = 0.001f;  // +0.1% per CTL point
    
    [Tooltip("โบนัส Spin ต่อ SPN point")]
    public float spinBonus = 1f;  // +1 spin force per SPN point
    
    [Tooltip("โบนัส Magnus ต่อ CRV point")]
    public float curveBonus = 0.02f;  // +0.02 magnus per CRV point

    [Header("--- Events ---")]
    public UnityEvent OnStatsChanged;

    // Calculated bonuses / ค่าโบนัสที่คำนวณแล้ว
    public float TotalPowerBonus => power * powerDistanceBonus;  // ระยะเพิ่ม
    public float TotalPerfectZoneBonus => control * controlZoneBonus;  // Perfect Zone เพิ่ม
    public float TotalSpinBonus => spin * spinBonus;  // Spin เพิ่ม
    public float TotalCurveBonus => curve * curveBonus;  // Magnus เพิ่ม

    // Total stat points
    public int TotalStats => power + control + spin + curve;

    void OnValidate()
    {
        // แจ้งเมื่อ stats เปลี่ยน (ใน Editor)
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ตั้งค่า Stats ทั้งหมด
    /// </summary>
    public void SetStats(int pwr, int ctl, int spn, int crv)
    {
        power = Mathf.Clamp(pwr, 1, 50);
        control = Mathf.Clamp(ctl, 1, 50);
        spin = Mathf.Clamp(spn, 1, 50);
        curve = Mathf.Clamp(crv, 1, 50);
        
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// คำนวณระยะสูงสุดหลังจากใช้ PWR bonus
    /// </summary>
    public float GetMaxDistanceWithBonus(float baseDistance)
    {
        return baseDistance + TotalPowerBonus;
    }

    /// <summary>
    /// คำนวณ Perfect Zone Size หลังจากใช้ CTL bonus
    /// </summary>
    public float GetPerfectZoneSizeWithBonus(float baseSize)
    {
        return baseSize + TotalPerfectZoneBonus;
    }

    /// <summary>
    /// คำนวณ Spin Multiplier หลังจากใช้ SPN bonus
    /// </summary>
    public float GetSpinMultiplierWithBonus(float baseMultiplier)
    {
        return baseMultiplier + TotalSpinBonus;
    }

    /// <summary>
    /// คำนวณ Magnus Coefficient หลังจากใช้ CRV bonus
    /// </summary>
    public float GetMagnusCoefficientWithBonus(float baseCoefficient)
    {
        return baseCoefficient + TotalCurveBonus;
    }
}
