# GRAND DRIVE: AETHER LINKS - AI Coding Instructions

## Project Overview
**Unity 6 (URP) | C# | Anime Fantasy Golf RPG**

Physics-driven golf game with Magnus effect spin mechanics, RPG progression, and anime special moves.

## Architecture

### Core Components
| File | Responsibility |
|------|----------------|
| `Assets/GolfBallController.cs` | Ball physics, spin, Magnus effect, wind |
| `Assets/BallCameraController.cs` | Smooth follow camera with zoom |
| `Assets/BallMat.physicMaterial` | Ball bounce/friction (0.6 bounciness, 0.4 friction) |
| `Assets/InputSystem_Actions.inputactions` | New Input System (not yet integrated in scripts) |

### Component Communication Pattern
```
GolfBallController → FindFirstObjectByType<BallCameraController>()
                   → StartFollowing() / StopFollowing()
```
Controllers find each other at `Start()` using `FindFirstObjectByType<T>()`.

## Unity 6 Specifics (Critical)
- **Use `rb.linearVelocity`** instead of deprecated `rb.velocity`
- **Use `rb.angularVelocity`** for spin tracking
- Physics runs in `FixedUpdate()`, input in `Update()`

## Physics Implementation Patterns

### Shot Execution (GolfBallController.ShootBall)
```csharp
// 1. Direction: forward + lift
Vector3 shotDir = (transform.forward + new Vector3(0, 0.3f, 0)).normalized;
rb.AddForce(shotDir * totalPower, ForceMode.Impulse);

// 2. Spin from impact point (-1 to 1 range)
Vector3 spinAxis = new Vector3(-impactVertical, impactHorizontal, 0);
rb.AddTorque(spinAxis * spinMultiplier, ForceMode.Impulse);
```

### Magnus Effect (ApplyEnvironmentEffects)
```csharp
Vector3 magnusForce = Vector3.Cross(rb.linearVelocity, rb.angularVelocity) * magnusCoefficient;
rb.AddForce(magnusForce);
```

### Ball State Detection
Ball stops when: `rb.linearVelocity.magnitude < 0.1f && transform.position.y < 0.6f`

## Impact System
- `impactHorizontal`: -1 (Hook/Left) to 1 (Slice/Right) → Y-axis torque
- `impactVertical`: -1 (Backspin/Bottom) to 1 (Topspin/Top) → X-axis torque (inverted)

## Camera System (BallCameraController)
- Smooth follow using `Vector3.SmoothDamp` and `Quaternion.Slerp`
- Zoom: Mouse scroll wheel, clamped `minDistance` to `maxDistance`
- Updates in `LateUpdate()` for post-physics positioning

## Developer Testing
| Key | Action |
|-----|--------|
| `Spacebar` | Shoot ball (100% power) |
| `R` | Reset ball to origin |
| `Scroll` | Zoom camera |

## Planned Features (Not Yet Implemented)
- **Special Shots**: Tomahawk, Spike, Cobra (implement via state machine + physics overrides)
- **Ground Types**: Use `PhysicMaterial` per terrain (Green, Fairway, Rough, Ice)
- **Equipment Stats**: Power, Control, Spin modifiers from external scripts
- **New Input System Integration**: `InputSystem_Actions.inputactions` exists but uses legacy `Input.GetKeyDown`

## Code Conventions
- Thai comments in existing code (maintain or translate consistently)
- Inspector-exposed settings via `[Header]`, `[Tooltip]`, `[Range]` attributes
- State tracking via `bool isInAir` pattern
- Debug.Log for physics values during development
