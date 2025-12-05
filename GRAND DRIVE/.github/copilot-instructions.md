# GRAND DRIVE: AETHER LINKS - AI Coding Instructions

> 🎯 **เอกสารนี้สำหรับ AI Coding Agents** - อ่านทั้งหมดก่อนเขียนโค้ด
> This document is for AI Coding Agents - Read completely before writing code

---

## 1. Project Overview | ภาพรวมโปรเจกต์

| Item | Value |
|------|-------|
| **Project Name** | GRAND DRIVE: AETHER LINKS |
| **Genre** | Anime Fantasy Golf RPG (Casual & Strategy) |
| **Engine** | Unity 6000.0.63f1 (URP) |
| **Language** | C# |
| **Platform** | Mobile (iOS/Android) & PC |
| **Art Style** | Cel-Shaded Anime (like Genshin Impact) |
| **Theme** | กอล์ฟบนเกาะลอยฟ้า "Arcadia" ผสมเทคโนโลยีลมและเวทมนตร์โบราณ |
| **Signature Sound** | "SCH-WING!" (เสียงดาบตัดอากาศ + กระดิ่งแก้ว เมื่อ Perfect Impact) |

---

## 2. Architecture | โครงสร้างโค้ด

### 2.1 Core Files (ไฟล์หลัก)
| File | Responsibility | Status |
|------|----------------|--------|
| `Assets/GolfBallController.cs` | Ball physics, spin, Magnus effect, wind | ✅ Implemented |
| `Assets/BallCameraController.cs` | Smooth follow camera with zoom | ✅ Implemented |
| `Assets/BallMat.physicMaterial` | Ball bounce/friction settings | ✅ Implemented |
| `Assets/InputSystem_Actions.inputactions` | New Input System config | ⏳ Not integrated |
| `Assets/Scenes/SampleScene.unity` | Main game scene | ✅ Implemented |

### 2.2 Component Communication Pattern
```csharp
// Controllers ค้นหากันใน Start() ด้วย FindFirstObjectByType<T>()
cameraController = FindFirstObjectByType<BallCameraController>();

// เรียกเมื่อตีลูก / When shooting:
cameraController.StartFollowing();

// เรียกเมื่อลูกหยุด / When ball stops:
cameraController.StopFollowing();
```

---

## 3. Unity 6 Critical Requirements | ข้อกำหนดสำคัญ Unity 6

```csharp
// ❌ WRONG (Deprecated in Unity 6)
rb.velocity = newVelocity;

// ✅ CORRECT
rb.linearVelocity = newVelocity;
rb.angularVelocity = spinVector;
```

| Loop | Purpose |
|------|---------|
| `Update()` | Input polling, UI updates |
| `FixedUpdate()` | Physics calculations (AddForce, AddTorque) |
| `LateUpdate()` | Camera follow (after physics) |

---

## 4. Core Gameplay Mechanics | ระบบการเล่นหลัก

### 4.1 Swing System (ระบบการตี)

| Input Type | Description |
|------------|-------------|
| **3-Click Bar** | กด 3 ครั้ง: Power → Accuracy → Impact |
| **Hold & Release** | กดค้าง → ปล่อยตอนจังหวะ Perfect |

| Impact Result | Effect |
|---------------|--------|
| **Perfect Impact** | ลูกพุ่งตรง + "SCH-WING!" effect + Max distance |
| **Thin (Miss)** | ตีท็อปหัวลูก → Low trajectory |
| **Fat (Miss)** | ขุดดิน → เสียระยะมาก |
| **Gear Effect (Toe)** | Hook (เลี้ยวซ้าย) อัตโนมัติ |
| **Gear Effect (Heel)** | Slice (เลี้ยวขวา) อัตโนมัติ |

### 4.2 Dynamic Ball Impact System (ระบบจุดตีบนลูก) ⭐ CRUCIAL

> ผู้เล่นเลื่อนจุดตีบนลูกกอล์ฟได้ (เหมือนหน้าปัดนาฬิกา)

```
        TOP (+Y) = Topspin
           ⬆️
    LEFT ⬅️ ⚪ ➡️ RIGHT
   (Hook)   ⬇️   (Slice)
      BOTTOM (-Y) = Backspin
```

| Impact Point | Parameter | Trajectory | Wind | After Landing |
|--------------|-----------|------------|------|---------------|
| **Top** (Topspin) | `impactVertical = 1` | Low | ต้านได้ดี | วิ่งไกล (Run) |
| **Bottom** (Backspin) | `impactVertical = -1` | High | รับเต็มที่ | หยุด/ถอยหลัง |
| **Left** (Hook) | `impactHorizontal = -1` | Curves Left | - | Magnus Effect |
| **Right** (Slice) | `impactHorizontal = 1` | Curves Right | - | Magnus Effect |

### 4.3 Impact Control Mastery
> ขอบเขตวงกลมที่อนุญาตให้เลื่อนจุดตี ขึ้นอยู่กับ Equipment

```csharp
// วงกลมจุดตี = ไม้กอล์ฟ + ถุงมือ + แหวน + สกิล
float impactRadius = club.baseRadius + gloves.bonus + ring.bonus + skill.bonus;

// รูปร่างวงกลมขึ้นกับไม้
// Spin Clubs = วงรีตั้ง (เน้น Topspin/Backspin)
// Power Clubs = วงรีนอน (เน้น Hook/Slice)
```

---

## 5. Physics Implementation | การ Implement ฟิสิกส์

### 5.1 Current Shot Execution
```csharp
public void ShootBall(float powerPercentage)
{
    // 1. ทิศทาง: ไปข้างหน้า + งัดขึ้นเล็กน้อย
    Vector3 shotDir = (transform.forward + new Vector3(0, 0.3f, 0)).normalized;
    
    // 2. ใส่แรงระเบิด
    float totalPower = powerPercentage * powerMultiplier;
    rb.AddForce(shotDir * totalPower, ForceMode.Impulse);

    // 3. ใส่ Spin ตามจุด Impact
    // impactVertical: -1 (Backspin) to 1 (Topspin) → X-axis torque (inverted)
    // impactHorizontal: -1 (Hook) to 1 (Slice) → Y-axis torque
    Vector3 spinAxis = new Vector3(-impactVertical, impactHorizontal, 0);
    rb.AddTorque(spinAxis * spinMultiplier, ForceMode.Impulse);
}
```

### 5.2 Magnus Effect (Curve Physics)
```csharp
void ApplyEnvironmentEffects()
{
    // 1. แรงลม
    rb.AddForce(windDirection, ForceMode.Force);

    // 2. Magnus Effect: แรงยก = ความเร็ว × ความเร็วเชิงมุม
    Vector3 magnusForce = Vector3.Cross(rb.linearVelocity, rb.angularVelocity) * magnusCoefficient;
    rb.AddForce(magnusForce);
}
```

### 5.3 Ball State Detection
```csharp
// ลูกหยุดเมื่อ: ความเร็วต่ำ + อยู่ใกล้พื้น
bool isStopped = rb.linearVelocity.magnitude < 0.1f && transform.position.y < 0.6f;
```

---

## 6. Special Shots (ท่าไม้ตาย) | ✅ IMPLEMENTED

> ต้องสะสม **Impact Gauge** จนเต็มก่อนใช้
> ปุ่มเลือก: 1 = Normal, 2 = Spike, 3 = Tomahawk, 4 = Cobra
>
> **⚠️ IMPLEMENTATION RULE**: Special Shots (Spike/Tomahawk) MUST use **Apex Detection** (checking when vertical velocity < 0) to change trajectory mid-air. Do NOT rely on initial physics alone.
> **กฎการเขียนโค้ด**: ท่าไม้ตาย Spike และ Tomahawk ต้องใช้การเช็ค **จุดสูงสุด (Apex)** เพื่อหักวิถีลูกกลางอากาศ ห้ามใช้แค่แรงส่งตอนเริ่มเด็ดขาด

### 6.0 Shot Comparison Chart (กราฟเปรียบเทียบวิถี)
```
HEIGHT
  ↑
  │     🟡 Spike (สูงสุด!)
  │    ╱  ╲
  │   ╱    ╲  🔴 Tomahawk
  │  ╱      ╲╱ ╲
  │ ╱   🟢   ╲   ↓ (ดิ่งตรง)
  │╱  Normal  ╲
  │     ╱╲     ╲
  │🔵 ╱  ╲      ╲
  │Cobra ╲       ╲
  └────────────────────────→ DISTANCE

🟢 Normal (เขียว): โค้งปกติ, กลิ้งต่อได้
🟡 Spike (เหลือง): สูงที่สุด → เฉียงลง 45° → หยุดนิ่งทันที
🔴 Tomahawk (แดง): สูงมาก → ดิ่งลงตรงๆ → หยุดนิ่งทันที
🔵 Cobra (ฟ้า): ต่ำมาก → เด้งหลายครั้ง → กลิ้งต่อ
```

### 6.1 Normal Shot (🟢 เขียว) - Default
| Property | Value |
|----------|-------|
| **มุมยิง** | ~30-45° |
| **วิถี** | โค้ง Parabola ปกติ |
| **หลังตก** | กลิ้งต่อได้ตามปกติ |
| **ใช้เมื่อ** | การตีทั่วไป |

### 6.2 Spike Shot (🟡 เหลือง) - สูงสุด → เฉียงลง → หยุดนิ่ง
```
วิถี:    🚀 (มุม 75°+ สูงที่สุด!)
        ╱
       ╱
      ╱
     ╱     📍 APEX (จุดสูงสุด)
    │         ╲
    │          ╲  (พุ่งเฉียงลง 45°)
    │           ╲
    │            ╲
    └─────────────💥 หยุดนิ่งทันที!
```
| Property | Value |
|----------|-------|
| **มุมยิง** | 75°+ (สูงที่สุดในทุก shot) |
| **Apex** | ถึงจุดสูงสุดแล้วพุ่งเฉียงลง 45° |
| **หลังตก** | **หยุดนิ่งทันที** (Dead Stop) |
| **ใช้เมื่อ** | ข้ามสิ่งกีดขวางสูง + ต้องการหยุดตรงจุด |

```csharp
// Spike Physics
spikeLaunchAngle = 75f;   // มุมยิงสูงสุด
spikeDiveAngle = 45f;     // มุมเฉียงลงเมื่อถึง apex
// เมื่อตกพื้น → StopBallImmediately()
```

### 6.3 Tomahawk Shot (🔴 แดง) - สูงมาก → หยุดนิ่ง
```
วิถี:    🚀 (มุม 65° สูงมาก)
        ╱
       ╱
      ╱   📍 APEX
      │        ╲
      │         ╲  (โค้งลงปกติ)
      │          ╲
      │           ╲
      └────────────💥 หยุดนิ่งทันที!
```
| Property | Value |
|----------|-------|
| **มุมยิง** | 65° (สูงมาก แต่ต่ำกว่า Spike) |
| **Apex** | ถึงจุดสูงสุดแล้ว **โค้งลงตามปกติ** (ไม่ดิ่ง) |
| **หลังตก** | **หยุดนิ่งทันที** (Dead Stop) |
| **ใช้เมื่อ** | ข้ามต้นไม้ + ต้องการตกตรงจุด |

```csharp
// Tomahawk Physics
tomahawkLaunchAngle = 65f;   // มุมยิงสูงมาก
// ไม่มีการหักลงที่ Apex (ปล่อยให้โค้งตามฟิสิกส์)
// เมื่อตกพื้น → StopBallImmediately()
```

### 6.4 Cobra Shot (🔵 ฟ้า) - ต่ำมาก → เด้งหลายครั้ง
```
วิถี:  ══════►  (มุม 12° ต่ำมาก)
              ╲
               ⚪  (เด้ง 1)
                ╲
                 ⚪  (เด้ง 2)
                  ╲
                   ⚪  (เด้ง 3)
                    ╲___🏌️ กลิ้งต่อ
```
| Property | Value |
|----------|-------|
| **มุมยิง** | 12° (ต่ำที่สุด) |
| **วิถี** | แทบไม่ขึ้นสูง |
| **หลังตก** | **เด้งหลายครั้ง** แล้วกลิ้งต่อ |
| **ใช้เมื่อ** | ลอดใต้สิ่งกีดขวาง + ต้องการระยะ run |

```csharp
// Cobra Physics  
cobraLaunchAngle = 12f;      // มุมยิงต่ำมาก
cobraForwardForce = 30f;     // แรงไปข้างหน้า
cobraBounciness = 0.6f;      // เด้งหลายครั้ง
// ไม่หยุดนิ่ง ให้กลิ้งต่อตามปกติ
```

### 6.5 Special Shot Summary Table
| Shot | สี | มุมยิง | Apex Behavior | หลังตก |
|------|-----|--------|---------------|--------|
| **Normal** | 🟢 เขียว | 30-45° | โค้งปกติ | กลิ้งต่อ |
| **Spike** | 🟡 เหลือง | **75°+** | เฉียงลง 45° | **หยุดนิ่ง** |
| **Tomahawk** | 🔴 แดง | 65° | โค้งปกติ | **หยุดนิ่ง** |
| **Cobra** | 🔵 ฟ้า | 12° | ไม่มี (ต่ำมาก) | เด้งหลายครั้ง |

### 6.6 Key Differences: Spike vs Tomahawk
| | 🟡 Spike | 🔴 Tomahawk |
|--|----------|-------------|
| **ความสูง** | **สูงที่สุด** | สูงมาก |
| **ตกลง** | เฉียงลง ↘ (45°) | โค้งลง ↘ (ปกติ) |
| **ระยะทาง** | ไกลกว่า | ใกล้กว่า |
| **หยุด** | หยุดนิ่งทันที | หยุดนิ่งทันที |

---

## 7. RPG System | ระบบ RPG

### 7.1 Character Stats
| Stat | Effect |
|------|--------|
| **Power** | ระยะตีพื้นฐาน |
| **Control** | ขนาด Perfect Zone |
| **Impact** | ขอบเขต Impact Circle |
| **Spin** | ความแรง Topspin/Backspin |
| **Curve** | ความแรง Hook/Slice |

### 7.2 Starter Characters (4 ตัวละครเริ่มต้น)

| Character | Type | Passive Gift | Effect |
|-----------|------|--------------|--------|
| **Kaito** | Power | "Tidal Rush" | ตีเกิน 95% ระยะ → บัฟพลังตาถัดไป |
| **Luna** | Precision | "Celestial Guide" | ลดผลกระทบ Slope + ไกด์ไลน์พัตต์ยาวขึ้น |
| **Faye** | Technical | "Sleight of Hand" | ลด Impact Gauge cost 25% (Cobra/Spike) |
| **Rex** | Survival | "Survivor's Instinct" | ลด Penalty จาก Rough/Bunker 20% |

### 7.3 Equipment System (อุปกรณ์)

| Slot | Item | Mechanic Effect |
|------|------|-----------------|
| **Main** | ไม้กอล์ฟ | กำหนด Shape วงกลมจุดตี (วงรีตั้ง/นอน) |
| **Head** | หมวก/แว่น | มองเห็นในหมอก/ฝน, ชะลอเกจ |
| **Ears** | ตุ้มหู | บอกตัวเลขลมละเอียด, อ่าน Slope กรีน |
| **Neck** | สร้อยคอ | เร่ง Impact Gauge, เพิ่มโชค |
| **Body** | เสื้อ | เพิ่มระยะพื้นฐาน, ความเสถียร |
| **Legs** | กางเกง | ลดโทษ Rough/Bunker, เพิ่มช่องไอเทม |
| **Support** | แคดดี้ | Passive Buff + ช่วยเก็บของ |

### 7.4 Impact Items (ไอเทมกดใช้)
> จัดเซ็ตลงสนามได้ 3 ช่อง

| Rarity | Item | Effect |
|--------|------|--------|
| **Common** | Power Drink | เพิ่มระยะ |
| **Common** | Focus Cookie | ขยาย Perfect Zone |
| **Rare** | Aero-Gel | ลบล้างลม |
| **Rare** | Spin Potion | ขยายขอบเขตจุดตีเต็มใบ |
| **Epic** | Titan Serum | การันตี Perfect Impact |
| **Epic** | Phoenix Tear | Mulligan (ตีใหม่ได้) |

---

## 8. Ground Types | ประเภทพื้น

> ใช้ `PhysicMaterial` swapping

| Ground | Friction | Bounce | Effect |
|--------|----------|--------|--------|
| **Fairway** | 0.4 | 0.6 | Normal play |
| **Green** | 0.3 | 0.4 | Low bounce, rolls far |
| **Rough** | 0.7 | 0.3 | ลดระยะ, ยากตี |
| **Bunker** | 0.9 | 0.1 | ลดระยะมาก, แทบไม่กระดอน |
| **Ice** | 0.1 | 0.5 | ลื่นมาก, วิ่งไม่หยุด |

---

## 9. Dev Testing Keys | ปุ่มทดสอบ

| Key | Action |
|-----|--------|
| `Spacebar` | Shoot ball (100% power) |
| `R` | Reset ball to origin (0, 0.5, 0) |
| `Mouse Scroll` | Zoom camera |

---

## 10. Code Conventions | แนวทางเขียนโค้ด

### 10.1 Comments (ความคิดเห็น)
```csharp
// ✅ OK: สองภาษาได้ (Bilingual is fine)
// คำนวณแรงยก Magnus / Calculate Magnus lift force
Vector3 magnusForce = Vector3.Cross(rb.linearVelocity, rb.angularVelocity);
```

### 10.2 Inspector Attributes
```csharp
[Header("--- Golf Physics Settings ---")]
[Tooltip("ความแรงในการตี / Shot power multiplier")]
[Range(0f, 100f)]
public float powerMultiplier = 20f;
```

### 10.3 State Pattern
```csharp
// ใช้ Boolean flags สำหรับสถานะ
private bool isInAir = false;
private bool isFollowing = true;

// หรือ Enum สำหรับ Special Shots
public enum SpecialShotType { None, Tomahawk, Spike, Cobra }
private SpecialShotType currentSpecialShot = SpecialShotType.None;
```

### 10.4 Modular Design (สำหรับ Equipment)
```csharp
// ออกแบบให้ Stat ถูก modify จาก Equipment ได้
public float GetFinalPower()
{
    return basePower 
         + equipment.club.powerBonus 
         + equipment.body.powerBonus 
         + character.powerStat;
}
```

---

## 11. Key Packages | แพ็คเกจหลัก

| Package | Version | Purpose |
|---------|---------|---------|
| `com.unity.inputsystem` | 1.16.0 | New Input System |
| `com.unity.render-pipelines.universal` | 17.0.4 | URP Rendering |
| `com.unity.ai.navigation` | 2.0.9 | AI Navigation |

---

## 12. TODO / Not Yet Implemented | สิ่งที่ยังไม่ได้ทำ

- [x] Special Shots (Spike, Tomahawk, Cobra) ✅
- [x] Impact Gauge system ✅
- [x] Pangya-style 3-Click Swing System ✅
- [x] Perfect Impact "SCH-WING!" sound effect ✅
- [x] Dual Minimap with trajectory visualization ✅
- [ ] Equipment system with stat modifiers
- [ ] Character selection with Passive Gifts
- [ ] Ground type PhysicMaterial swapping
- [x] New Input System integration ✅
- [ ] Impact Items (consumables)
- [ ] Wind visualization

---

## 12.1 Development Progress Log | บันทึกความคืบหน้า

### 2024-12-04 Session
**Features Implemented:**
1. **Pangya-style 3-Click Swing System** (`Assets/Scripts/SwingSystem.cs`)
   - Click 1: Start power bar (moves left → right → left loop)
   - Click 2: Set distance (stop bar position)
   - Click 3: Hit in Perfect Zone for accuracy
   - Perfect Zone: Center at -0.75f, size 0.2f

2. **SCH-WING! Sound Effect**
   - Plays on Perfect Impact (not "PANGYA!" - per user request)
   - Normal hit sound for non-perfect shots

3. **Dual Minimap System** (`Assets/Scripts/MinimapSetup.cs`)
   - Left camera: Wide view (shows full trajectory)
   - Right camera: Follow view (tracks ball)
   - Trajectory line: Green (start) → Yellow (end)
   - Fairway guide line: White

4. **Physics Bug Fixes** (`Assets/GolfBallController.cs`)
   - Fixed: Cannot set velocity on kinematic body error
   - Fixed: Magnus effect explosion when ball is slow (speed < 1 m/s)
   - Fixed: Order of operations - set velocity BEFORE enabling kinematic
   - Clamped magnus force to max 50 units

**Files Modified:**
- `Assets/GolfBallController.cs` - Ball physics, special shots, bug fixes
- `Assets/Scripts/SwingSystem.cs` - 3-click swing mechanic
- `Assets/Scripts/SwingUI.cs` - Swing bar UI with TextMeshPro
- `Assets/Scripts/MinimapSetup.cs` - Dual camera minimap
- `Assets/Scripts/SpecialShotSystem.cs` - Gauge management

**Known Issues (To Test):**
- [ ] Ball may still shoot unexpectedly when stopping (needs testing)
- [ ] powerMultiplier tuning for realistic distance

---

## 13. Asset & Reference Storage | การจัดเก็บไฟล์อ้างอิง

> **Rule**: Save all images, documents, or reference files provided by the User into the `References/` folder at the project root.
> **กฎ**: ให้บันทึกรูปภาพ เอกสาร หรือไฟล์อ้างอิงทั้งหมดที่ผู้ใช้ส่งให้ ลงในโฟลเดอร์ `References/` ที่ root ของโปรเจกต์

---

---

## 14. Unity Editor Automation & Communication | กฎการทำงานกับ Unity Editor

> **Rule 1**: If a task requires setup in the Unity Editor (creating Objects, adding Components), you MUST:
> 1.  Create an **Editor Script** (`[MenuItem]`) to automate it if possible.
> 2.  Tell the user exactly what to click (e.g., "Click `Tools > Setup`").
> 3.  Clarify if they need to **DELETE** old objects first or if the script handles updates.
>
> **กฎข้อที่ 1**: หากงานต้องมีการตั้งค่าใน Unity Editor (สร้างของ, ใส่สคริปต์) คุณต้อง:
> 1.  เขียน **Editor Script** เพื่อทำให้มันอัตโนมัติ
> 2.  บอกผู้ใช้ว่าต้องกดเมนูไหน
> 3.  ระบุให้ชัดว่าต้อง **ลบของเก่าก่อนไหม** หรือกดทับได้เลย

---

## 15. Quick Reference Card | สรุปด่วน

```
┌─────────────────────────────────────────────────────┐
│  GRAND DRIVE: AETHER LINKS - Quick Reference        │
├─────────────────────────────────────────────────────┤
│  Engine: Unity 6 (6000.0.63f1) + URP               │
│  Physics: rb.linearVelocity (NOT rb.velocity!)     │
│                                                     │
│  Impact System:                                     │
│    impactVertical:   -1 (Backspin) to 1 (Topspin) │
│    impactHorizontal: -1 (Hook) to 1 (Slice)       │
│                                                     │
│  Magnus Effect:                                     │
│    Force = Cross(velocity, angularVelocity)        │
│                                                     │
│  Special Shots (ปุ่ม 1-4):                          │
│    1️⃣ Normal  🟢 = โค้งปกติ, กลิ้งต่อ              │
│    2️⃣ Spike   🟡 = สูงสุด → เฉียงลง → หยุดนิ่ง     │
│    3️⃣ Tomahawk🔴 = สูงมาก → ดิ่งตรง → หยุดนิ่ง     │
│    4️⃣ Cobra   🔵 = ต่ำมาก → เด้งหลายครั้ง          │
│                                                     │
│  Loop Order:                                        │
│    Update() → FixedUpdate() → LateUpdate()         │
│    (Input)    (Physics)       (Camera)             │
└─────────────────────────────────────────────────────┘
```
## 16. Calibration Precision Protocol | มาตรฐานการจูนระยะ
> **Rule**: การจูนระยะต้องใช้มาตรฐาน "Bracket Method" และทศนิยม 5 ตำแหน่งเสมอ

1.  **Format**: เมื่อรายงานช่วงการปรับ ต้องใช้รูปแบบ:
    `Scale_Low (Negative Error) - Scale_High (Positive Error)`
    Example: `0.95300 (-0.19m) - 0.95410 (+0.19m)`
2.  **Precision**: ค่า Scale ต้องใช้ทศนิยม 5 ตำแหน่ง (F5) เช่น `0.95410`
3.  **Targeting**: ต้องปรับค่าจนกว่าจะบีบช่วง (Bracket) เข้าหา 0.00m ที่สุด หรือจนกว่าทศนิยมตำแหน่งที่ 5 จะตัน
4.  **Acceptance**: งานจะผ่านก็ต่อเมื่อมี Bracket ที่ครอบคลุมค่า 0 หรือ Error เป็น 0.00m เท่านั้น

---
