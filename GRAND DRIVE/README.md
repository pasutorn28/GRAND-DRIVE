# 🏌️ GRAND DRIVE: AETHER LINKS

> **Anime Fantasy Golf RPG** - A fantasy golf game combining realistic physics with RPG progression and anime-style special moves.

![Unity](https://img.shields.io/badge/Unity-6.0-blue?logo=unity)
![License](https://img.shields.io/badge/License-MIT-green)

## 🎮 Game Concept

GRAND DRIVE: AETHER LINKS is a unique blend of:
- **Realistic Golf Physics** - Magnus effect, spin dynamics, wind resistance
- **RPG Progression** - Stats, equipment, and character growth
- **Anime Special Moves** - Tomahawk, Spike, Cobra shots

## 🎯 Core Mechanics

### Swing System
- 3-click bar or hold-and-release input
- Perfect Impact = "SCH-WING!" straight shot
- Gear Effect: Toe/Heel hits cause Hook/Slice

### Dynamic Ball Impact
Adjust impact point on the ball like a clock face:
- **Top Impact** → Topspin (low trajectory, high roll)
- **Bottom Impact** → Backspin (high trajectory, quick stop)
- **Side Impact** → Curve via Magnus Effect

### Special Shots
| Shot | Trajectory | Effect |
|------|------------|--------|
| **Tomahawk** | Normal arc → Vertical drop | Explosion stop or slope bounce |
| **Spike** | High apex → Sharp dive | Buries into ground |
| **Cobra** | Low skim → Sharp rise | Ignores wind during skim |

## 🛠️ Tech Stack

- **Engine**: Unity 6 (URP)
- **Language**: C#
- **Physics**: Rigidbody-based with custom Magnus effect

## 🎮 Controls (Dev Testing)

| Key | Action |
|-----|--------|
| `Spacebar` | Shoot ball |
| `R` | Reset ball |
| `Mouse Scroll` | Zoom camera |

## 📁 Project Structure

```
Assets/
├── GolfBallController.cs    # Ball physics & spin
├── BallCameraController.cs  # Camera follow system
├── BallMat.physicMaterial   # Ball physics material
├── Scenes/
│   └── SampleScene.unity    # Main game scene
└── Settings/                # URP & project settings
```

## 🚀 Getting Started

1. Clone this repository
2. Open with Unity 6.0+
3. Open `Assets/Scenes/SampleScene.unity`
4. Press Play and hit `Spacebar` to shoot!

## 📜 License

MIT License - Feel free to use and modify!

---

*Built with ❤️ for golf and anime fans*
