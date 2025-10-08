# Celeste-Style Movement System

A complete, production-ready 2D movement system for Unity inspired by Celeste, featuring tight controls and advanced platformer mechanics.

## Features

✅ **Core Movement**
- Smooth acceleration/deceleration
- Variable jump height (tap vs hold)
- Precise air control

✅ **Advanced Mechanics**
- **Coyote Time** - Jump grace period after leaving ledges
- **Jump Buffering** - Queue jumps before landing
- **Wall Sliding** - Controlled descent on walls
- **Wall Grab** - Cling to walls and climb
- **Wall Jump** - Launch off walls with momentum
- **Dash** - 8-directional dash with collision detection
- **Moving Platform Support** - Inherit platform velocity

## Scripts Overview

### Movement Scripts
- **`PlayerMovement.cs`** - Core movement logic with all Celeste-style features
- **`PlayerCollision.cs`** - Precise raycast-based collision detection
- **`PlayerInput.cs`** - Input handling using Unity's new Input System
- **`PlayerController.cs`** - Minimal controller (extend with game logic)

### Supporting Scripts
- **`PlatformController.cs`** - Simple moving platform implementation
- **`CollisionInfo.cs`** - Collision data structures (in PlayerCollision.cs)
- **`RaycastInfo.cs`** - Raycast configuration (in PlayerCollision.cs)

### Input System
- **`PlayerInputActions.inputactions`** - Input action definitions
- **`PlayerInputActions.cs`** - Auto-generated input wrapper

## Quick Start

See **[SETUP_GUIDE.md](SETUP_GUIDE.md)** for detailed setup instructions.

### TL;DR Setup (5 min):
1. Create layers: `Ground` and `Platform`
2. Create player sprite with all 4 movement scripts
3. Configure `PlayerCollision` layer masks
4. Create ground sprites and set to `Ground` layer
5. Set Physics2D gravity to 0
6. Test with arrow keys, C (jump), X (dash), Z (wall grab)

## Controls (Default)

| Action | Key | Customizable |
|--------|-----|--------------|
| Move | Arrow Keys | ✅ |
| Jump | C | ✅ |
| Dash | X | ✅ |
| Wall Grab | Z | ✅ |

Change controls by modifying `PlayerInputActions.inputactions` in Unity.

## Physics Settings

**Important:** Set `Physics2D` gravity to `0` (the PlayerMovement script handles its own gravity for precise control).

Go to: `Edit > Project Settings > Physics 2D` and set Gravity Y to 0.

## Customization

All movement values are exposed in the Inspector on `PlayerMovement`:

### Walking
- Max Move Speed
- Acceleration
- Deceleration

### Jumping
- Max Jump Height
- Min Jump Height
- Time To Jump Apex
- Jump Buffer (timing window)
- Coyote Jump (timing window)

### Wall Mechanics
- Wall Slide Speed
- Wall Climb Speed
- Wall Stick Time
- Wall Grab Duration
- Grab Distance
- Wall Jump Force

### Dash
- Dash Distance
- Dash Duration
- Speed After Dash

### Falling
- Min Fall Speed
- Max Fall Speed

## Code Quality

- ✅ Clean, well-documented code
- ✅ Consistent naming conventions
- ✅ No compiler warnings or errors
- ✅ Modular design for easy extension
- ✅ Event-driven architecture

## Requirements

- Unity 2021.3+ (tested on 2021.3+)
- Unity Input System package (com.unity.inputsystem)
- .NET Standard 2.1

## Credits

Original movement code by **Biebras** (2020)
- Inspired by Sebastian Lague and Tarodev
- Adapted and cleaned for production use

## License

See LICENSE file for details.

## Troubleshooting

**Player falls through ground:**
- Check layer masks in PlayerCollision
- Verify ground objects have BoxCollider2D
- Ensure layers are properly set

**Input not working:**
- Regenerate PlayerInputActions.cs from .inputactions file
- Check Input System package is installed
- Look for errors in Console

**Movement feels wrong:**
- Verify Physics2D gravity is set to 0
- Adjust PlayerMovement values in Inspector
- Check Time.deltaTime is working (not paused)

For detailed troubleshooting, see **[SETUP_GUIDE.md](SETUP_GUIDE.md)**.

## Support

This is a minimal, self-contained movement system. Extend `PlayerController.cs` with your game-specific logic (health, abilities, game state, etc.).

