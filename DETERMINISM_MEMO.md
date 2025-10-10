# Deterministic Movement System

## Purpose
This game uses **deterministic movement** for competitive speedrunning. Identical inputs must produce identical results regardless of hardware or framerate, enabling replay verification and fair competition.

## Critical Requirements - DO NOT BREAK

### 1. Fixed Timestep Lock
- `Time.fixedDeltaTime = 1f / 60f` and `Time.maximumDeltaTime = Time.fixedDeltaTime` (locked in `PlayerMovement.Awake()`)
- **Never modify these dynamically**

### 2. Physics in FixedUpdate Only
- All movement/physics calculations in `FixedUpdate()`, never `Update()`
- Always use `Time.fixedDeltaTime`, never `Time.deltaTime` for physics

### 3. Input Separation Pattern
- Poll input in `Update()` → store in variables → process in `FixedUpdate()`
- Buffer input events between physics frames

### 4. Deterministic Systems Only
- No unseeded RNG affecting gameplay
- No Unity physics solver (use kinematic movement)
- No frame-dependent collision detection or async gameplay operations
- Keep Unity version consistent across builds

### 5. Avoid Floating-Point Drift
- Use consistent calculation order; consider fixed-point math for perfect cross-platform reproducibility

## Testing
Before releases: Record input sequence, replay 10+ times on different machines/framerates, verify positions match exactly.

---
**Last Updated:** October 2025

