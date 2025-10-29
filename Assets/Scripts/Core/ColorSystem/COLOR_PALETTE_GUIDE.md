# Color Palette Guide - Mining Game

## Why OKLCH?

This project uses the **OKLCH color space** for all color definitions because it is **perceptually uniform**. Unlike RGB or HSL:
- A change of 10 in Lightness looks the same whether you're working with dark blues or bright yellows
- Chroma (saturation) behaves predictably across all hues
- It's easier to create harmonious palettes systematically

### OKLCH Components
- **L (Lightness)**: 0-100% - Perceptual brightness
- **C (Chroma)**: 0-0.4 - Colorfulness/saturation (higher = more vibrant)
- **H (Hue)**: 0-360° - Color angle (0=red, 120=green, 240=blue, etc.)

## Color Tier System

Our palette uses **fixed L and C values per tier** to ensure visual coherence, while **leaving Hue free** for artistic expression. This simplified 3-tier system creates depth through brightness contrast.

### Tier 0: Background (Air/Empty Space)
**Purpose**: The dark space where terrain doesn't exist - creates depth

```
L = 28%
C = 0.03
H = 220° (recommended: cool blue-purple for atmospheric depth)
```

**Effect**: Dark and nearly desaturated - tiles "pop" against it

**Why These Values**:
- **L=28%** is much darker than foreground (58%) - creates strong depth cue
- **C=0.03** is almost achromatic - stays neutral, doesn't interfere with tile colors
- **H=220°** (cool blue-purple) - psychologically recedes, feels like space/depth
  - Can adjust: H=200-240° for different cave atmospheres
  - Warmer (H=30-50°) for lava/fire areas
  - Cooler (H=200-220°) for ice caves

**Visual Effect**: Dark background makes all foreground content feel closer to the player

### Tier 1: Foreground (All Regular Tiles)
**Purpose**: Everything you interact with - dirt, stone, all ores, gems, blocks

```
L = 58%
C = 0.15
H = FREE (use any hue appropriate for the material)
```

**Effect**: Medium brightness, moderate saturation - the main visual layer

**Example Hues**:
- Dirt: H=40° (warm brown)
- Stone: H=90° (cool gray)
- Iron: H=270° (purple-gray)
- Copper: H=30° (orange-brown)
- Gold: H=60° (yellow)
- Ruby: H=10° (red)
- Emerald: H=145° (green)
- Sapphire: H=240° (blue)

**Why One Tier**: Simplifies your workflow - all regular tiles have the same visual weight. Distinction comes from hue alone, which is more flexible.

### Tier 2: Special (Effects & Highlights)
**Purpose**: Glowing elements, magical effects, UI highlights, player indicators

```
L = 72%
C = 0.28
H = FREE
```

**Effect**: Bright and highly saturated - immediately draws attention

**Example Uses**:
- Glowing crystals: H=180° (cyan), H=280° (magenta)
- Magic effects: H=280° (purple)
- Player highlight: H=60° (yellow)
- Danger indicators: H=15° (red)
- Interactive objects: H=120° (green)

## Usage Workflow

### For New Materials:
1. Decide which tier the material belongs to
2. Pick an appropriate hue (0-360°)
3. Use the fixed L and C values for that tier
4. Generate the color using `ColorManager.CreateColorFromTier()`

### For Reference Colors (Photos/Concept Art):
1. Get the hex color from your reference (color picker tool)
2. Input it into `ColorManager.ClampToNearestTier()`
3. The system will find the nearest tier and adjust the color to match our restrictions
4. The hue is preserved, but L and C are snapped to tier values

## Visual Summary: Complete Color Space

Here's how the 3-tier system creates depth:

```
Tier 0 (Background):  L=28%, C=0.03  ←  Dark, nearly grayscale (recedes)
                      ↕ Gap: 30%
Tier 1 (Foreground):  L=58%, C=0.15  ←  Medium, moderate saturation (main layer)
                      ↕ Gap: 14%
Tier 2 (Special):     L=72%, C=0.28  ←  Bright, highly saturated (pops forward)
```

**Key Insight**: The simple 3-tier structure creates natural depth perception through lightness alone:
- Dark = far away (background)
- Medium = at focal plane (tiles you interact with)  
- Bright = close/important (special effects)

## Why This Works

**Depth Perception**: Lightness creates psychological distance
- Dark (L=28%) = recedes into space, feels distant
- Medium (L=58%) = the "canvas" where gameplay happens
- Bright (L=72%) = advances toward player, demands attention

**Simplicity**: Only one tier for all regular content
- No need to categorize by rarity - just pick appropriate hues
- Common dirt and rare gold both use Tier 1, distinguished by hue alone
- Easier to add new materials without tier decisions

**Flexibility**: Hue is completely free within each tier
- 360° of color choice per tier
- Easy to theme areas (warm desert, cool ice caves) by shifting hues

**Visual Clarity**: 30% lightness gap between background and foreground ensures perfect separation

## Technical Notes

- OKLCH is converted to sRGB for Unity's Color class
- Some extreme OKLCH combinations may be out of RGB gamut (will be clamped)
- All conversions are handled by `ColorManager.cs`

