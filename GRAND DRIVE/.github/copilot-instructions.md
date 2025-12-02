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

## 6. Special Shots (ท่าไม้ตาย) | ⏳ TO IMPLEMENT

> ต้องสะสม **Impact Gauge** จนเต็มก่อนใช้

### 6.1 Tomahawk (ลูกระเบิด) 💥
```
วิถี: พุ่งโค้งปกติ ──────► ดิ่งลงแนวตั้ง 💣
                              │
                              ▼
```
| Ground Type | Physics Behavior |
|-------------|------------------|
| **Flat** | Dead Stop (Velocity = 0) หยุดนิ่งทันที |
| **Slope/Cliff** | กระเด้งตาม Slope Normal + Gravity (ไม่ติด!) |

```csharp
// Pseudo-code for Tomahawk
void ExecuteTomahawk()
{
    // Phase 1: Normal arc
    ApplyNormalTrajectory();
    
    // Phase 2: At apex, switch to vertical drop
    if (reachedApex) {
        rb.linearVelocity = Vector3.down * tomahawkDropSpeed;
    }
    
    // Phase 3: On impact
    if (hitGround) {
        if (IsFlat(groundNormal)) {
            rb.linearVelocity = Vector3.zero; // Dead stop
        } else {
            // Bounce based on slope
            Vector3 bounceDir = Vector3.Reflect(rb.linearVelocity, groundNormal);
            rb.linearVelocity = bounceDir * bounceFactor;
        }
        PlayExplosionVFX();
    }
}
```

### 6.2 Spike (ลูกตบ/ปัก) 📌
```
วิถี: พุ่งขึ้นสูงมาก 🚀
           │
           │  (Super High Apex)
           │
           └──► ตบดิ่งลงแนวเฉียง 45°
                        ▼
                   [ปักพื้น]
```
| Feature | Description |
|---------|-------------|
| **Use Case** | ข้ามสิ่งกีดขวางสูง, หยุดเร็วมาก |
| **Physics** | High friction on impact, buries into ground |
| **Wind** | ดีต่อการข้าม wind เพราะวิถีสูง |

```csharp
// Pseudo-code for Spike
void ExecuteSpike()
{
    // Phase 1: Super high launch
    rb.AddForce(Vector3.up * spikeLaunchForce, ForceMode.Impulse);
    
    // Phase 2: At apex, dive diagonally
    if (reachedApex) {
        Vector3 diveDir = (targetPos - transform.position).normalized;
        diveDir.y = -1f; // Force downward
        rb.linearVelocity = diveDir.normalized * spikeDiveSpeed;
    }
    
    // Phase 3: Bury into ground (high friction)
    if (hitGround) {
        rb.linearVelocity *= 0.1f; // Almost stop
        // Or use PhysicMaterial with high friction
    }
}
```

### 6.3 Cobra (ลูกเลียด) 🐍
```
วิถี:  ═══════════►  เลียดพื้น (Ground Hug)
                        │
                        └──► เหินขึ้นสูง 🚀
                                    │
                                    ▼ ตกปกติ
```
| Phase | Wind Effect | Description |
|-------|-------------|-------------|
| **Phase 1: Skim** | ❌ Ignores Wind | เลียดพื้นต่ำมาก, รอดใต้สิ่งกีดขวาง |
| **Phase 2: Rise** | ✅ Normal | เหินขึ้นปกติ |
| **Phase 3: Drop** | ✅ Normal | ตกลงปกติ |

```csharp
// Pseudo-code for Cobra
void ExecuteCobra()
{
    // Phase 1: Low skim (ignore wind)
    if (isSkimPhase) {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0.1f, rb.linearVelocity.z);
        ignoreWind = true;
    }
    
    // Phase 2: Sharp rise
    if (skimDistance >= targetSkimDistance) {
        rb.AddForce(Vector3.up * cobraRiseForce, ForceMode.Impulse);
        ignoreWind = false;
    }
    
    // Phase 3: Normal drop (handled by gravity)
}
```

### 6.4 Special Shot + Spin Combos
> สามารถผสมจุดตี (Spin) เข้ากับท่าไม้ตายได้!

| Combo | Result |
|-------|--------|
| **Tomahawk + Topspin** | ระเบิดแล้วพุ่งไปข้างหน้าเร็ว |
| **Tomahawk + Backspin** | ระเบิดแล้วหยุดนิ่งสนิท |
| **Spike + Sidespin** | ตบลงแล้วเลี้ยวโค้ง |
| **Cobra + Topspin** | เลียดแล้ววิ่งไกลหลังตก |

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

- [ ] Special Shots (Tomahawk, Spike, Cobra)
- [ ] Impact Gauge system
- [ ] Equipment system with stat modifiers
- [ ] Character selection with Passive Gifts
- [ ] Ground type PhysicMaterial swapping
- [ ] New Input System integration
- [ ] Impact Items (consumables)
- [ ] Wind visualization
- [ ] Perfect Impact "SCH-WING!" effect

---

## 13. Quick Reference Card | สรุปด่วน

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
│  Special Shots:                                     │
│    🔥 Tomahawk = Vertical drop + Explosion stop    │
│    📌 Spike = High apex + Diagonal dive + Bury     │
│    🐍 Cobra = Low skim (no wind) + Sharp rise      │
│                                                     │
│  Loop Order:                                        │
│    Update() → FixedUpdate() → LateUpdate()         │
│    (Input)    (Physics)       (Camera)             │
└─────────────────────────────────────────────────────┘
```
