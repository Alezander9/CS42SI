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

Our palette uses **fixed Lightness per tier** and **chroma ranges** to ensure visual coherence while providing flexibility. This 3-tier system creates depth through brightness contrast while allowing saturation variation within each tier.

### Tier 0: Background (Air/Empty Space)
**Purpose**: The dark space where terrain doesn't exist - creates depth

```
L = 28% (fixed)
C = 0.02-0.05 (range)
H = 0-360° (free)
```

**Effect**: Dark and nearly desaturated - tiles "pop" against it

**Why These Values**:
- **L=28%** is much darker than foreground (58%) - creates strong depth cue
- **C=0.02-0.05** is nearly achromatic - stays neutral, doesn't interfere with tile colors
- **H=220°** recommended (cool blue-purple) - psychologically recedes, feels like space/depth
  - Can adjust: H=200-240° for different cave atmospheres
  - Warmer (H=30-50°) for lava/fire areas
  - Cooler (H=200-220°) for ice caves

**Visual Effect**: Dark background makes all foreground content feel closer to the player

### Tier 1: Foreground (All Regular Tiles)
**Purpose**: Everything you interact with - dirt, stone, all ores, gems, blocks

```
L = 58% (fixed)
C = 0.03-0.20 (range)
H = 0-360° (free)
```

**Effect**: Medium brightness with variable saturation - the main visual layer

**Example Materials**:
- Stone: H=220°, C=0.04 (neutral gray - very low saturation)
- Dirt: H=40°, C=0.10 (warm brown - moderate saturation)
- Iron: H=240°, C=0.06 (cool gray - low saturation)
- Copper: H=30°, C=0.14 (orange-brown - richer)
- Gold: H=60°, C=0.16 (yellow - rich)
- Ruby: H=10°, C=0.18 (red - vibrant)
- Emerald: H=145°, C=0.15 (green - vibrant)
- Sapphire: H=240°, C=0.16 (blue - vibrant)

**Key Insight**: Low chroma (C=0.03-0.06) creates gray/earthy tones, higher chroma (C=0.12-0.20) creates vibrant colored materials.

### Tier 2: Special (Effects & Highlights)
**Purpose**: Glowing elements, magical effects, UI highlights, player indicators, character sprites

```
L = 72% (fixed)
C = 0.22-0.35 (range)
H = 0-360° (free)
```

**Effect**: Bright and highly saturated - immediately draws attention

**Example Uses**:
- Glowing crystals: H=180°, C=0.30 (cyan)
- Magic effects: H=280°, C=0.32 (purple)
- Player highlight: H=60°, C=0.28 (yellow)
- Danger indicators: H=15°, C=0.30 (red)
- Character sprites: Various H, C=0.22-0.35 (full saturation range)

## Usage Workflow

### For New Materials:
1. Decide which tier the material belongs to
2. Pick an appropriate hue (0-360°)
3. Pick a chroma value within the tier's range
   - Lower chroma for grays/earthy tones
   - Higher chroma for vibrant colors
4. Generate the color using `ColorManager.CreateColorFromTier(tier, hue, chroma)`

### For Reference Colors (Photos/Concept Art):
1. Get the hex color from your reference (color picker tool)
2. Input it into `ColorManager.ClampToNearestTier(hexColor)`
3. The system will:
   - Find the nearest tier based on lightness
   - Adjust lightness to match tier
   - Clamp chroma to tier's valid range (preserving it as much as possible)
   - Preserve the hue exactly
4. Returns: `(Color color, ColorTier tier, float hue, float chroma)`

## Visual Summary: Complete Color Space

Here's how the 3-tier system creates depth:

```
Tier 0 (Background):  L=28%, C=0.02-0.05  ←  Dark, nearly grayscale (recedes)
                      ↕ Gap: 30%
Tier 1 (Foreground):  L=58%, C=0.03-0.20  ←  Medium, variable saturation (main layer)
                      ↕ Gap: 14%
Tier 2 (Special):     L=72%, C=0.22-0.35  ←  Bright, highly saturated (pops forward)
```

**Key Insights**: 
- **Lightness** creates depth: Dark = recedes, Medium = focal plane, Bright = advances
- **Chroma ranges** provide flexibility: Low for grays/earthy, high for vibrant colors
- **Hue** is completely free within each tier for maximum artistic expression

## Why This Works

**Depth Perception**: Lightness creates psychological distance
- Dark (L=28%) = recedes into space, feels distant
- Medium (L=58%) = the "canvas" where gameplay happens
- Bright (L=72%) = advances toward player, demands attention

**Simplicity**: Only one tier for all regular content
- No need to categorize by rarity - just pick appropriate hues and chromas
- Common stone (low chroma) and rare ruby (high chroma) both use Tier 1
- Easier to add new materials without complex tier decisions

**Flexibility**: Both hue and chroma are free within tier constraints
- 360° of color choice per tier
- Variable saturation for grays to vibrant colors
- Easy to theme areas (warm desert, cool ice caves) by shifting hues
- Can create cohesive earthy blocks with low chroma

**Visual Clarity**: 30% lightness gap between background and foreground ensures perfect separation

**Gray Colors**: Low chroma (C=0.03-0.06) in Foreground tier enables true gray and earthy tones without losing tier separation

## Technical Notes

- OKLCH is converted to sRGB for Unity's Color class
- Some extreme OKLCH combinations may be out of RGB gamut (will be clamped)
- All conversions are handled by `ColorManager.cs`

