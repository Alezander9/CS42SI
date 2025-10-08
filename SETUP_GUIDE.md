# Movement System Setup Guide

## Quick Setup Steps

### 1. Create Layers
1. Go to **Edit > Project Settings > Tags and Layers**
2. Add these layers:
   - Layer 6: `Ground`
   - Layer 7: `Platform`

### 2. Create Player GameObject

1. **Create Player:**
   - Right-click in Hierarchy > **2D Object > Sprite > Square**
   - Rename to `Player`
   - Set Position to (0, 2, 0)
   - Set Scale to (0.5, 0.5, 1)

2. **Add Components to Player:**
   - `BoxCollider2D` (should auto-add with sprite)
   - `PlayerController` (drag script)
   - `PlayerCollision` (drag script)
   - `PlayerInput` (drag script)
   - `PlayerMovement` (drag script)

3. **Configure PlayerCollision:**
   - Collision Mask: Select `Ground` layer
   - Platform Mask: Select `Platform` layer
   - Ray Count: 3
   - Skin Width: 0.15

4. **Configure PlayerMovement:**
   - All values should have good defaults
   - Tweak to taste after testing

### 3. Create Ground

1. **Create Ground:**
   - Right-click in Hierarchy > **2D Object > Sprite > Square**
   - Rename to `Ground`
   - Set Position to (0, -1, 0)
   - Set Scale to (10, 1, 1)
   - Set Layer to `Ground`

2. **Create Walls (optional):**
   - Duplicate Ground (Ctrl+D)
   - Rename to `Wall_Left`
   - Set Position to (-5, 2, 0)
   - Set Scale to (1, 8, 1)
   - Set Layer to `Ground`
   
   - Duplicate again for `Wall_Right`
   - Set Position to (5, 2, 0)

### 4. Camera Setup

1. Select Main Camera
2. Set Position to (0, 0, -10)
3. Set Size to 5 (or adjust to see the whole scene)

### 5. Test Controls

**Keyboard Controls:**
- **Arrow Keys**: Move left/right, climb up/down
- **C**: Jump
- **Z**: Wall Grab (hold while on wall)
- **X**: Dash

## Advanced: Create Moving Platform (Optional)

1. Create a new Sprite (Square)
2. Rename to `MovingPlatform`
3. Set Layer to `Platform`
4. Add `PlatformController` script
5. Configure:
   - X Speed: 2
   - Y Speed: 0
   - Point A: (-3, 0, 0)
   - Point B: (3, 0, 0)

## Testing Checklist

- [ ] Player spawns at checkpoint
- [ ] Can walk left/right (smooth acceleration)
- [ ] Can jump
- [ ] Jump height varies (tap vs hold)
- [ ] Coyote time works (can jump shortly after leaving ledge)
- [ ] Jump buffering works (pressing jump before landing)
- [ ] Wall slide on walls
- [ ] Wall grab when holding Z
- [ ] Wall jump
- [ ] Dash in 8 directions with X

## Troubleshooting

**Player falls through ground:**
- Check Ground layer is set correctly
- Check PlayerCollision Collision Mask includes Ground layer
- Check BoxCollider2D is on both Player and Ground

**Input not working:**
- Check PlayerInput component has no errors
- Make sure PlayerInputActions.cs was generated
- Check Console for input system errors

**Movement feels off:**
- Adjust values in PlayerMovement inspector
- Increase acceleration for snappier movement
- Adjust gravity/jump values for different feel

## Recommended Test Scene Layout

```
Player (0, 2)
    
Ground (0, -1) - Scale: (10, 1)
Wall_Left (-5, 2) - Scale: (1, 8)
Wall_Right (5, 2) - Scale: (1, 8)

Optional:
MovingPlatform (0, 1) - Platform layer
```

## Physics Settings (Important!)

Make sure your Physics2D settings are correct:
1. Go to **Edit > Project Settings > Physics 2D**
2. Gravity Y should be **0** (PlayerMovement handles its own gravity)
3. Or set to -9.81 and adjust PlayerMovement gravity values accordingly

