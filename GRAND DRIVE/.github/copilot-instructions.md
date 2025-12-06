GRAND DRIVE: AETHER LINKS - AI Coding Instructions
🛑 IMPORTANT / ข้อสำคัญ
เอกสารนี้สำหรับ AI Coding Agents - กรุณาอ่านทั้งหมดก่อนเริ่มเขียนโค้ด
For AI Agents: Read this entire document before writing any code. Context is key.
1. 🏗️ Project Overview | ภาพรวมโปรเจกต์
Item	Details
Project Name	GRAND DRIVE: AETHER LINKS
Genre	Anime Fantasy Golf RPG (Casual & Strategy)
Engine	Unity 6 (6000.0.63f1)LTS + URP
Language	C#
Platform	Mobile (iOS/Android) & PC
Art Style	Cel-Shaded Anime (Genshin Impact style)
Theme	Floating Island "Arcadia", Wind Tech, Ancient Magic
Audio Signature	"SCH-WING!" (Sword cut + Glass bell on Perfect Impact)
2. ⚡ Critical Technical Rules | กฎทางเทคนิค (Unity 6)
2.1 Physics Migration (Unity 6 Required)
⚠️ STRICT RULE: Do not use deprecated properties.
ห้ามใช้คำสั่งเก่าที่ยกเลิกไปแล้วใน Unity 6
code
C#
// ❌ DO NOT USE (Deprecated)
rb.velocity = Vector3.zero;

// ✅ USE THIS instead
rb.linearVelocity = Vector3.zero;
rb.angularVelocity = Vector3.zero;
2.2 Execution Order
Update(): Input polling, UI updates.
FixedUpdate(): Physics calculations (AddForce, AddTorque).
LateUpdate(): Camera follow (ensure smooth movement after physics).
2.3 Automation Protocol
Rule: If a task requires Editor setup (creating objects, adding components), you MUST create an Editor Script ([MenuItem]) to automate it.
Do not ask the user to manually drag-and-drop unless impossible.
3. 📂 Architecture & Files | โครงสร้างไฟล์
3.1 Core Scripts
File	Responsibility	Status
Assets/GolfBallController.cs	Physics, Spin, Magnus Effect, Wind	✅ Active
Assets/BallCameraController.cs	Smooth Follow, Zoom	✅ Active
Assets/Scripts/SwingSystem.cs	3-Click Bar Mechanic (Pangya Style)	✅ Active
Assets/Scripts/SpecialShotSystem.cs	Impact Gauge, Shot Type Logic	✅ Active
3.2 Component Communication
Use standard dependency injection via Start():
code
C#
// Example: Controller finding Camera
void Start() {
    cameraController = FindFirstObjectByType<BallCameraController>();
}

// Event-driven usage
public void OnShotFired() => cameraController.StartFollowing();
public void OnBallStop() => cameraController.StopFollowing();
4. ⛳ Gameplay Mechanics | ระบบการเล่น
4.1 Swing System (3-Click)
Click 1: Start Bar (Cursor moves Left → Right).
Click 2: Set Power (Cursor stops, moves back Right → Left).
Click 3: Set Accuracy (Hit the "Perfect Zone" near start).
4.2 Impact Mechanics (Spin Control)
Players can adjust the impact point on the ball (Clock face metaphor).
Zone	Input Value	Effect	Physics Result
Top	impactVertical = 1	Topspin (Low trajectory, High run)	Forward Torque
Bottom	impactVertical = -1	Backspin (High trajectory, Stop/Back)	Backward Torque
Left	impactHorizontal = -1	Hook (Curves Left)	Y-Axis Torque (-)
Right	impactHorizontal = 1	Slice (Curves Right)	Y-Axis Torque (+)
4.3 Physics Implementation
code
C#
// 1. Direction & Power
Vector3 shotDir = (transform.forward + new Vector3(0, launchAngle, 0)).normalized;
rb.AddForce(shotDir * power * multiplier, ForceMode.Impulse);

// 2. Spin (Inverted X for vertical spin)
Vector3 spinAxis = new Vector3(-impactVertical, impactHorizontal, 0);
rb.AddTorque(spinAxis * spinMultiplier, ForceMode.Impulse);

// 3. Magnus Effect (In FixedUpdate)
Vector3 magnusForce = Vector3.Cross(rb.linearVelocity, rb.angularVelocity) * magnusCoef;
rb.AddForce(magnusForce);
5. 🔥 Special Shots | ท่าไม้ตาย (Implemented)
Implementation Rule: Special shots (Spike/Tomahawk) MUST use Apex Detection (checking rb.linearVelocity.y < 0) to trigger their mid-air behavior.
5.1 Shot Comparison
code
Mermaid
graph TD;
  Height --> Distance;
  Spike(🟡 Spike: Highest) -->|Dead Stop| Ground;
  Tomahawk(🔴 Tomahawk: High) -->|Dead Stop| Ground;
  Normal(🟢 Normal: Medium) -->|Rolls| Ground;
  Cobra(🔵 Cobra: Lowest) -->|Skips/Rolls| Ground;
5.2 Detailed Specs
Shot	Color	Trajectory	Key Mechanic	Landing
Normal	🟢	Parabola	Standard physics	Rolls naturally
Spike	🟡	Super High 	Apex: Dives down 	Dead Stop (No roll)
Tomahawk	🔴	High 	Apex: Drops straight down	Dead Stop (No roll)
Cobra	🔵	Super Low (12°)	Skims low, rises late	Skips/Bounces forward
6. ⚔️ RPG & Equipment System
6.1 Stats
Power: Distance.
Control: Accuracy zone size.
Impact: Spin/Curve potential circle size.
Spin: Torque multiplier.
Curve: Horizontal curve strength.
6.2 Equipment Slots
Main (Club): Defines Impact Circle shape (Ellipse Vertical/Horizontal).
Support (Caddy): Passive buffs.
Wearables: Stat modifiers.
6.3 Ground Physics (PhysicMaterial)
Type	Friction	Bounce	Note
Fairway	0.4	0.6	Standard
Rough	0.7	0.3	Penalty (Distance loss)
Bunker	0.9	0.1	Heavy Penalty (No roll)
Ice	0.1	0.5	Slippery
7. 🧪 Calibration & Testing | การทดสอบ
7.1 Debug Keys
Key	Action
Space	Shoot Ball (100% Power)
R	Reset Ball Position
Scroll	Zoom Camera
7.2 Calibration Protocol (Standard)
When tuning distance/physics, use "Bracket Method":
Precision: Use 5 decimal places (F5).
Reporting: Scale_Low (Neg Error) - Scale_High (Pos Error).
Example: 0.95300 (-0.19m) - 0.95410 (+0.19m)
Goal: Narrow the bracket until Error is 0.00m.
8. 📝 Status & Logs | สถานะล่าสุด
Features Done (✅)
Unity 6 Physics (linearVelocity)
Pangya-style 3-Click Swing
Special Shots (Spike, Tomahawk, Cobra)
Dual Minimap (Wide + Follow)
"SCH-WING!" Perfect Sound
To-Do (⏳)
Equipment System (Stat Modifiers)
Character Passives
Wind Visualization UI
Impact Consumables (Potions)
9. 📌 Quick Reference Card
code
Text
┌──────────────────────────────────────────────────────────┐
│ GRAND DRIVE: AETHER LINKS - CHEAT SHEET                  │
├──────────────────────────────────────────────────────────┤
│ 🔧 ENGINE: Unity 6 (URP)                                 │
│ ⚠️ PHYSICS: Use rb.linearVelocity (NOT rb.velocity)      │
│                                                          │
│ 🎯 SHOTS:                                                │
│   1. Normal 🟢 (Rolls)                                   │
│   2. Spike  🟡 (Super High -> Dive -> Stop)          │
│   3. Toma   🔴 (High -> Vertical Drop -> Stop)           │
│   4. Cobra  🔵 (Low -> Skims -> Rises)                   │
│                                                          │
│ 🌪️ SPIN:                                                 │
│   Top/Back = Inverted X Torque                           │
│   Hook/Slice = Y Torque                                  │
│   Magnus = Cross(Velocity, AngularVelocity)              │
│                                                          │
│ 📂 REFERENCE: Save user assets to /References folder     │
└──────────────────────────────────────────────────────────┘

10. Character & Gift Logic
Implement as CharacterProfile ScriptableObjects
Starter Characters
Kaito (Power):
Gift: "Tidal Rush"
Code Logic: if (lastShotDistance > maxDistance * 0.95) nextShotPower *= 1.05;
Luna (Precision):
Gift: "Celestial Guide"
Code Logic: puttingLineLength *= 1.2f; greenSlopeEffect *= 0.7f;
Faye (Technique):
Gift: "Sleight of Hand"
Code Logic: cobraCost = 0.75f; spikeCost = 0.75f;
Rex (Survival):
Gift: "Survivor's Instinct"
Code Logic: if (terrain == Rough || terrain == Bunker) penaltyMultiplier = 0.8f;