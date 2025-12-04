/// <summary>
/// ประเภท Special Shot ที่ใช้ร่วมกันทั้งโปรเจค
/// Shared Special Shot Type enum for the entire project
/// 
/// 🟢 Normal: โค้งปกติ กลิ้งต่อได้
/// 🟡 Spike: ขึ้นสูงที่สุด → ถึง apex แล้วพุ่งเฉียงลง → หยุดนิ่งทันที
/// 🔴 Tomahawk: ขึ้นสูงมาก → ดิ่งลงตรงๆ → หยุดนิ่งทันที  
/// 🔵 Cobra: ต่ำมาก → พุ่งขึ้นที่ 2/3 ระยะ → ลงตรงๆ
/// </summary>
public enum SpecialShotType
{
    Normal,     // 🟢 ตีปกติ โค้งปกติ
    Spike,      // 🟡 ขึ้นสูงสุด → เฉียงลง → หยุดนิ่ง
    Tomahawk,   // 🔴 ขึ้นสูงมาก → ดิ่งตรง → หยุดนิ่ง
    Cobra       // 🔵 ต่ำมาก → พุ่งขึ้น → ลงตรง
}
