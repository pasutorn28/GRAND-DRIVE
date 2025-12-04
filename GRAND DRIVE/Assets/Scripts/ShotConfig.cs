using UnityEngine;

/// <summary>
/// ScriptableObject สำหรับเก็บค่า config ของ Special Shots ทั้งหมด
/// สามารถสร้างหลาย preset ได้ เช่น Easy, Normal, Pro
/// 
/// วิธีใช้: Right-click ใน Project > Create > Golf > Shot Config
/// </summary>
[CreateAssetMenu(fileName = "ShotConfig", menuName = "Golf/Shot Config", order = 1)]
public class ShotConfig : ScriptableObject
{
    // ==================== Constants ====================
    public const float METERS_PER_YARD = 0.9144f;
    public const float YARDS_PER_METER = 1.09361f;

    [Header("=== Base Parameters ===")]
    [Tooltip("ค่าหลักคูณพลังทุก shot (ที่ power 100% จะได้ระยะ = targetDistance)")]
    [DecimalPlaces(4)]
    public float powerMultiplier = 1.0000f;
    
    [Tooltip("ระยะทางเป้าหมายที่ power 100% (เมตร)")]
    [SerializeField][DecimalPlaces(4)] private float _targetDistanceMeters = 183.0000f;
    
    [Tooltip("ระยะทางเป้าหมายที่ power 100% (yards)")]
    [SerializeField][DecimalPlaces(4)] private float _targetDistanceYards = 200.0000f;

    // ==================== Properties ====================
    
    public float targetDistance
    {
        get => _targetDistanceMeters;
        set => _targetDistanceMeters = value;
    }

    public float targetDistanceYards
    {
        get => _targetDistanceYards;
        set => _targetDistanceYards = value;
    }

#if UNITY_EDITOR
    // เก็บค่าก่อนหน้าเพื่อตรวจจับการเปลี่ยนแปลง
    private float _lastMeters;
    private float _lastYards;

    private void OnEnable()
    {
        _lastMeters = _targetDistanceMeters;
        _lastYards = _targetDistanceYards;
    }

    private void OnValidate()
    {
        // ตรวจจับว่าค่าไหนเปลี่ยน
        bool metersChanged = Mathf.Abs(_targetDistanceMeters - _lastMeters) > 0.01f;
        bool yardsChanged = Mathf.Abs(_targetDistanceYards - _lastYards) > 0.01f;

        if (metersChanged)
        {
            // Meters เปลี่ยน → sync Yards
            _targetDistanceYards = _targetDistanceMeters * YARDS_PER_METER;
            _lastYards = _targetDistanceYards;
        }
        else if (yardsChanged)
        {
            // Yards เปลี่ยน → sync Meters
            _targetDistanceMeters = _targetDistanceYards * METERS_PER_YARD;
            _lastMeters = _targetDistanceMeters;
        }

        // อัพเดทค่าก่อนหน้า
        _lastMeters = _targetDistanceMeters;
        _lastYards = _targetDistanceYards;
    }
#endif

    [Header("=== Normal Shot ===")]
    [Tooltip("มุมยิง Normal (องศา)")]
    [Range(20f, 45f)]
    public float normalLaunchAngle = 30f;
    
    [Tooltip("ตัวคูณพลัง Normal")]
    [DecimalPlaces(4)]
    public float normalPowerMod = 2.3600f;
    
    [Tooltip("ตัวปรับระยะ Normal (ชดเชย non-linear physics)")]
    [DecimalPlaces(6)]
    public float normalDistanceScale = 0.966814f;

    [Header("=== Spike Shot ===")]
    [Tooltip("มุมยิง Spike (องศา) - สูงกว่า Normal")]
    [Range(40f, 60f)]
    public float spikeLaunchAngle = 50f;
    
    [Tooltip("ตัวคูณพลัง Spike")]
    [DecimalPlaces(4)]
    public float spikePowerMod = 2.7600f;
    
    [Tooltip("ตัวปรับระยะ Spike (ชดเชย non-linear physics)")]
    [DecimalPlaces(6)]
    public float spikeDistanceScale = 0.930128f;
    
    [Tooltip("มุมดิ่งลงที่ apex (องศา)")]
    [Range(20f, 45f)]
    public float spikeDiveAngle = 30f;
    
    [Tooltip("ตัวคูณความเร็วดิ่ง")]
    [DecimalPlaces(4)]
    public float spikeDiveSpeedMultiplier = 3.5000f;

    [Header("=== Tomahawk Shot ===")]
    [Tooltip("มุมยิง Tomahawk (องศา)")]
    [Range(30f, 50f)]
    public float tomahawkLaunchAngle = 40f;
    
    [Tooltip("ตัวคูณพลัง Tomahawk")]
    [DecimalPlaces(4)]
    public float tomahawkPowerMod = 2.9800f;
    
    [Tooltip("ตัวปรับระยะ Tomahawk (ชดเชย non-linear physics)")]
    [DecimalPlaces(6)]
    public float tomahawkDistanceScale = 0.941505f;

    [Header("=== Cobra Shot ===")]
    [Space(10)]
    [Header("Phase 1: Low Flight")]
    [Tooltip("มุมยิง Phase 1 (องศา) - บินต่ำ")]
    [Range(3f, 15f)]
    public float cobraPhase1Angle = 6f;
    
    [Tooltip("ตัวคูณพลัง Phase 1")]
    [DecimalPlaces(4)]
    public float cobraPowerMod = 2.6000f;
    
    [Tooltip("ตัวปรับระยะ Cobra (ชดเชย non-linear physics)")]
    [DecimalPlaces(6)]
    public float cobraDistanceScale = 0.985970f;
    
    [Tooltip("สัดส่วนระยะ trigger Phase 2 (4/6 = 0.667)")]
    [Range(0.5f, 0.8f)][DecimalPlaces(4)]
    public float cobraTriggerRatio = 0.6670f;
    
    [Space(10)]
    [Header("Phase 2: Launch Up")]
    [Tooltip("มุมพุ่งขึ้น Phase 2 (องศา)")]
    [Range(55f, 80f)]
    public float cobraLaunchAngle = 68f;
    
    [Tooltip("ตัวคูณความเร็ว Phase 2")]
    [DecimalPlaces(4)]
    public float cobraSpeedMultiplier = 1.1720f;

    [Header("=== Physics Settings ===")]
    [Tooltip("จำนวน bounce สูงสุดก่อนหยุด")]
    public int maxBounces = 10;
    
    [Tooltip("ความเร็วต่ำสุดก่อนหยุด")]
    [DecimalPlaces(4)]
    public float stopVelocityThreshold = 0.1500f;

    /// <summary>
    /// คำนวณ Scale Factor สำหรับเปลี่ยน target distance
    /// </summary>
    public float GetScaleFactor(float newTargetDistance)
    {
        return newTargetDistance / targetDistance;
    }

    /// <summary>
    /// คำนวณ powerMultiplier ใหม่สำหรับ target distance ที่ต้องการ
    /// </summary>
    public float GetScaledPowerMultiplier(float newTargetDistance)
    {
        return powerMultiplier * GetScaleFactor(newTargetDistance);
    }

    /// <summary>
    /// คำนวณ cobraSpeedMultiplier สำหรับระยะที่ต้องการ
    /// ใช้สมการ: cobraSpeed = 1.1720 + (distance - 183) / 59.1
    /// </summary>
    public float GetCobraSpeedForDistance(float desiredDistance)
    {
        return 1.1720f + (desiredDistance - 183f) / 59.1f;
    }

    /// <summary>
    /// คำนวณระยะ Cobra trigger จาก expected distance
    /// </summary>
    public float GetCobraTriggerDistance(float expectedDistance)
    {
        return expectedDistance * cobraTriggerRatio;
    }

    /// <summary>
    /// คำนวณ tan ของมุม (สำหรับ dive direction)
    /// </summary>
    public float GetSpikeDiveTan()
    {
        return Mathf.Tan(spikeDiveAngle * Mathf.Deg2Rad);
    }

#if UNITY_EDITOR
    [ContextMenu("Log All Parameters")]
    void LogAllParameters()
    {
        Debug.Log("=== Shot Config Parameters ===");
        Debug.Log($"Power Multiplier: {powerMultiplier}");
        Debug.Log($"Target Distance: {targetDistance:F1}m ({targetDistanceYards:F1}y) @ 100% power");
        Debug.Log("");
        Debug.Log($"Normal: {normalLaunchAngle}° × {normalPowerMod}");
        Debug.Log($"Spike: {spikeLaunchAngle}° × {spikePowerMod} (dive: {spikeDiveAngle}° @ {spikeDiveSpeedMultiplier}x)");
        Debug.Log($"Tomahawk: {tomahawkLaunchAngle}° × {tomahawkPowerMod}");
        Debug.Log($"Cobra P1: {cobraPhase1Angle}° × {cobraPowerMod} (trigger: {cobraTriggerRatio:P0})");
        Debug.Log($"Cobra P2: {cobraLaunchAngle}° @ {cobraSpeedMultiplier}x");
    }

    [ContextMenu("Calculate for 200m Target")]
    void CalculateFor200m()
    {
        float newPower = GetScaledPowerMultiplier(200f);
        float newCobraSpeed = GetCobraSpeedForDistance(200f);
        Debug.Log($"For 200m target:");
        Debug.Log($"  powerMultiplier = {newPower:F2}f");
        Debug.Log($"  cobraSpeedMultiplier = {newCobraSpeed:F4}f");
    }
#endif
}
