# GRAND DRIVE: AETHER LINKS - AI Coding Instructions

## Project Overview
- **Engine**: Unity 6 (URP)
- **Language**: C#
- **Genre**: Anime Fantasy Golf RPG
- **Core Concept**: A fantasy golf game combining realistic physics (Magnus effect, spin) with RPG progression and anime-style special moves.

## Architecture & Key Components
- **Physics-Driven Gameplay**: The core loop relies heavily on Unity's physics engine (`Rigidbody`).
  - **Key File**: `Assets/GolfBallController.cs` handles ball physics, including Magnus effect and wind.
- **Input System**: Uses Unity's Input System (see `Assets/InputSystem_Actions.inputactions`).
- **Folder Structure**:
  - `Assets/Scenes`: Contains game scenes (e.g., `SampleScene.unity`).
  - `Assets/Settings`: Project settings and configuration assets.

## Core Gameplay Mechanics
### 1. Swing System
- **Input**: 3-click bar or hold-and-release.
- **Outcomes**:
  - **Perfect Impact**: Straight shot, "SCH-WING!" visual.
  - **Miss Hit**: Thin (low/fast) or Fat (dig/slow).
  - **Gear Effect**: Toe hit = Hook, Heel hit = Slice.

### 2. Dynamic Ball Impact System
- **Mechanism**: Player adjusts impact point on a "clock face" on the ball.
- **Effects**:
  - **Top Impact**: Topspin (low trajectory, high run).
  - **Bottom Impact**: Backspin (high trajectory, quick stop).
  - **Side Impact**: Curve (Magnus Effect).
- **RPG Integration**: Impact circle radius is defined by Equipment stats (Clubs, Rings, Gloves).

## Advanced Physics (Special Shots)
Implement these using `Rigidbody` manipulation and state flags:
1.  **Tomahawk**: Normal arc -> Vertical drop -> Explosion -> Dead stop (flat) or Bounce (slope).
2.  **Spike**: High apex -> Sharp dive -> Buries (high friction) -> Stop.
3.  **Cobra**: Low skim (ignores wind) -> Sharp rise -> Normal drop.

## Coding Conventions & Patterns
- **Unity 6 Specifics**: Use `rb.linearVelocity` instead of `rb.velocity`.
- **Physics Implementation**:
  - Use `Rigidbody.AddForce` for shots and wind.
  - Use `Rigidbody.AddTorque` for spin.
  - Calculate Magnus Force: `Vector3.Cross(rb.linearVelocity, rb.angularVelocity) * coefficient`.
- **Environment Interaction**:
  - Use `PhysicMaterial` for ground interactions (Fairway, Green, Ice).
  - Detect ground type to adjust friction and bounce.
- **Modularity**:
  - Keep physics logic separate from input logic.
  - Design stats (Power, Control, Spin) to be modifiable by external Equipment scripts.

## Developer Workflow
- **Testing**:
  - Use `Spacebar` to shoot and `R` to reset ball in `GolfBallController.cs` for quick physics iteration.
  - Debug logs are essential for tracking spin values and impact points.
