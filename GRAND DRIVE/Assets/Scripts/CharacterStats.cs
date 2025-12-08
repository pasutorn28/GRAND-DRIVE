using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// ระบบ Stats ของตัวละคร
/// Character Stats System - PWR/CTL/SPN/CRV affect swing mechanics
/// </summary>
[System.Serializable]
public class CharacterStats : MonoBehaviour
{
    [Header("--- Base Stats (0-50 scale) ---")]
    [Tooltip("PWR (Power) - พลังตี ยิ่งสูงระยะยิ่งไกล")]
    [Range(0, 50)] public int power = 0;
    
    void Awake()
    {
        // FORCE RESET STATS TO 0 TO FIX INSPECTOR OVERRIDES
        power = 0;
        control = 0;
        accuracy = 0;
        spin = 0;
        curve = 0;
    }
    
    [Tooltip("CTL (Control) - การควบคุม ยิ่งสูงเกจยิ่งวิ่งช้า")]
    [Range(0, 50)] public int control = 0;

    [Tooltip("ACC (Accuracy) - ความแม่นยำ ยิ่งสูง Perfect Zone ยิ่งกว้าง")]
    [Range(0, 50)] public int accuracy = 0;
    
    [Tooltip("SPN (Spin) - การหมุน ยิ่งสูง spin effect ยิ่งแรง")]
    [Range(0, 50)] public int spin = 0;
    
    [Tooltip("CRV (Curve) - การเลี้ยว ยิ่งสูง Magnus effect ยิ่งแรง")]
    [Range(0, 50)] public int curve = 0;

    [Header("--- Stat Effects ---")]
    [Tooltip("โบนัสระยะต่อ PWR point (yards)")]
    public float powerDistanceBonus = 2f;  // +2 yards per PWR point
    
    [Tooltip("โบนัสลดความเร็วเกจต่อ CTL point")]
    public float controlSpeedBonus = 0.02f;  // -0.02 speed per CTL point
    
    [Tooltip("โบนัส Perfect Zone ต่อ ACC point")]
    public float accuracyZoneBonus = 0.001f;  // +0.1% per ACC point
    
    [Tooltip("โบนัส Spin ต่อ SPN point")]
    public float spinBonus = 1f;  // +1 spin force per SPN point
    
    /// <summary>
    /// คำนวณ Bar Speed หลังจากใช้ CTL bonus (Control สูง = เกจช้า = ตีง่าย)
    /// </summary>
    public float GetBarSpeedWithBonus(float baseSpeed)
    {
        // สูตร: Speed ลดลงตาม Control
        // ตัวอย่าง: Base 2.0, Ctrl 12 -> 2.0 - (12 * 0.02) = 1.76
        // ตัวอย่าง: Base 2.0, Ctrl 50 -> 2.0 - (50 * 0.02) = 1.0 (ช้าลงเยอะ)
        float reduction = control * controlSpeedBonus;
        return Mathf.Max(0.5f, baseSpeed - reduction); // Clamp speed not to be 0
    }
    
    [Tooltip("โบนัส Magnus ต่อ CRV point")]
    public float curveBonus = 0.02f;  // +0.02 magnus per CRV point

    [Header("--- Events ---")]
    public UnityEvent OnStatsChanged;

    // Calculated bonuses / ค่าโบนัสที่คำนวณแล้ว
    public float TotalPowerBonus => power * powerDistanceBonus;  // ระยะเพิ่ม
    public float TotalAccuracyBonus => accuracy * accuracyZoneBonus;  // Perfect Zone เพิ่ม (จาก ACC)
    public float TotalSpinBonus => spin * spinBonus;  // Spin เพิ่ม
    public float TotalCurveBonus => curve * curveBonus;  // Magnus เพิ่ม

    // Total stat points
    public int TotalStats => power + control + accuracy + spin + curve;

    void OnValidate()
    {
        // แจ้งเมื่อ stats เปลี่ยน (ใน Editor)
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// ตั้งค่า Stats ทั้งหมด
    /// </summary>
    public void SetStats(int pwr, int ctl, int acc, int spn, int crv)
    {
        power = Mathf.Clamp(pwr, 0, 50);
        control = Mathf.Clamp(ctl, 0, 50);
        accuracy = Mathf.Clamp(acc, 0, 50);
        spin = Mathf.Clamp(spn, 0, 50);
        curve = Mathf.Clamp(crv, 0, 50);
        
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
    /// คำนวณ Perfect Zone Size หลังจากใช้ ACC bonus
    /// </summary>
    public float GetPerfectZoneSizeWithBonus(float baseSize)
    {
        return baseSize + TotalAccuracyBonus;
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
