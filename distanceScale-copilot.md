# Pangya-style Distance Scale Correction (distanceScale)

## Overview
This feature adds per-shot distance correction factors (`distanceScale`) to compensate for non-linear physics when scaling shot power in GRAND-DRIVE golf. It ensures each shot type (Normal, Spike, Tomahawk, Cobra) lands at the correct target distance when using a power multiplier (e.g. 1.25 for 250y).

## Why?
Unity's physics (air drag, bounce) are non-linear. When scaling power, each shot type overshoots or undershoots differently. `distanceScale` corrects this for each shot type.

## Implementation
- **ShotConfig.cs**: Adds 4 new fields:
  - `normalDistanceScale`, `spikeDistanceScale`, `tomahawkDistanceScale`, `cobraDistanceScale`
- **GolfBallController.cs**: Multiplies power by the correct `distanceScale` when `powerMultiplier > 1.0`.
- **DefaultShotConfig.asset**: Inspector now shows distanceScale for each shot type. Adjust these values to calibrate.

## Usage
1. Set `powerMultiplier` (e.g. 1.25 for 250y)
2. Calibrate each shot type:
   - Shoot, record actual distance
   - Calculate new scale: `distanceScale = oldScale * (target / actual)`
   - Update Inspector, save asset
3. Repeat until all shots land within ±1cm of target

## Example
| Shot      | Target | Actual   | Error      | New Scale |
|-----------|--------|----------|------------|-----------|
| Normal    | 228.6  | 225.29   | -1.45%     | 0.9549    |
| Spike     | 228.6  | 233.38   | +2.09%     | 0.9434    |
| Tomahawk  | 228.6  | 230.79   | +0.96%     | 0.9481    |
| Cobra     | 228.6  | 228.64   | +0.02%     | 0.9861    |

## Inspector
- All distanceScale fields are visible and editable in DefaultShotConfig.asset

## Pull Request Summary
- Adds distanceScale fields to ShotConfig.cs
- Updates GolfBallController.cs to use distanceScale
- Inspector now supports per-shot calibration
- Improves accuracy for all shot types at any power multiplier

---
**For calibration tips or code details, see the main README or ask Copilot!**
