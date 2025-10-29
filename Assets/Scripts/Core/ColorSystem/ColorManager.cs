using UnityEngine;
using System;

namespace Core.ColorSystem
{
    /// <summary>
    /// Utility class for managing colors using OKLCH color space.
    /// Provides conversions and tier-based color generation for visual coherence.
    /// See COLOR_PALETTE_GUIDE.md for design philosophy.
    /// </summary>
    public static class ColorManager
    {
        // ============================================
        // TIER DEFINITIONS
        // ============================================
        
        public enum ColorTier
        {
            Background = 0,         // Air, empty space - dark backdrop
            Foreground = 1,         // All regular tiles (dirt, stone, ores, gems)
            Special = 2             // Glowing effects, highlights, magic
        }

        public struct TierParams
        {
            public float Lightness;
            public float Chroma;
            
            public TierParams(float l, float c)
            {
                Lightness = l;
                Chroma = c;
            }
        }

        private static readonly TierParams[] TierSettings = new TierParams[]
        {
            new TierParams(0.28f, 0.03f),    // Tier 0: Background (dark)
            new TierParams(0.58f, 0.15f),    // Tier 1: Foreground (medium)
            new TierParams(0.72f, 0.28f)     // Tier 2: Special (bright)
        };

        // ============================================
        // PUBLIC API - TIER-BASED COLOR CREATION
        // ============================================
        
        /// <summary>
        /// Creates a Unity Color from a tier and hue.
        /// Lightness and Chroma are determined by the tier.
        /// </summary>
        /// <param name="tier">Material tier (1-4)</param>
        /// <param name="hue">Hue angle in degrees (0-360)</param>
        /// <returns>Unity Color object</returns>
        public static Color CreateColorFromTier(ColorTier tier, float hue)
        {
            TierParams p = TierSettings[(int)tier];
            return OKLCHToColor(p.Lightness, p.Chroma, hue);
        }

        /// <summary>
        /// Takes any hex color and snaps it to the nearest tier.
        /// Preserves hue but adjusts lightness and chroma to match tier restrictions.
        /// </summary>
        /// <param name="hexColor">Hex color string (with or without #)</param>
        /// <returns>Tuple of (clamped Color, assigned tier, preserved hue)</returns>
        public static (Color color, ColorTier tier, float hue) ClampToNearestTier(string hexColor)
        {
            // Parse hex to RGB
            Color inputColor = HexToColor(hexColor);
            
            // Convert to OKLCH
            (float l, float c, float h) = ColorToOKLCH(inputColor);
            
            // Find nearest tier by comparing lightness and chroma distance
            ColorTier nearestTier = FindNearestTier(l, c);
            
            // Create color with tier's L and C, but original hue
            TierParams tierParams = TierSettings[(int)nearestTier];
            Color clampedColor = OKLCHToColor(tierParams.Lightness, tierParams.Chroma, h);
            
            return (clampedColor, nearestTier, h);
        }

        /// <summary>
        /// Finds which tier a given L and C value is closest to.
        /// </summary>
        private static ColorTier FindNearestTier(float l, float c)
        {
            float minDistance = float.MaxValue;
            ColorTier nearestTier = ColorTier.Background;

            for (int i = 0; i <= 2; i++)
            {
                TierParams tier = TierSettings[i];
                // Euclidean distance in L-C space (weighted slightly toward L for perceptual reasons)
                float distance = Mathf.Sqrt(Mathf.Pow(l - tier.Lightness, 2) * 1.5f + Mathf.Pow(c - tier.Chroma, 2));
                
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTier = (ColorTier)i;
                }
            }

            return nearestTier;
        }

        /// <summary>
        /// Gets the recommended background/air color.
        /// Default hue is 220° (cool blue-purple), but can be customized for different biomes.
        /// </summary>
        /// <param name="hue">Optional custom hue (default: 220° for cool atmospheric depth)</param>
        /// <returns>Background color</returns>
        public static Color GetBackgroundColor(float hue = 220f)
        {
            return CreateColorFromTier(ColorTier.Background, hue);
        }

        // ============================================
        // OKLCH <-> RGB CONVERSION
        // ============================================
        
        /// <summary>
        /// Converts OKLCH color to Unity Color (sRGB).
        /// </summary>
        /// <param name="l">Lightness (0-1)</param>
        /// <param name="c">Chroma (0-0.4+)</param>
        /// <param name="h">Hue in degrees (0-360)</param>
        /// <returns>Unity Color</returns>
        public static Color OKLCHToColor(float l, float c, float h)
        {
            // Convert to OKLab
            float hRad = h * Mathf.Deg2Rad;
            float a = c * Mathf.Cos(hRad);
            float b = c * Mathf.Sin(hRad);
            
            // OKLab to Linear RGB
            float l_ = l + 0.3963377774f * a + 0.2158037573f * b;
            float m_ = l - 0.1055613458f * a - 0.0638541728f * b;
            float s_ = l - 0.0894841775f * a - 1.2914855480f * b;

            float l3 = l_ * l_ * l_;
            float m3 = m_ * m_ * m_;
            float s3 = s_ * s_ * s_;

            float r_linear = +4.0767416621f * l3 - 3.3077115913f * m3 + 0.2309699292f * s3;
            float g_linear = -1.2684380046f * l3 + 2.6097574011f * m3 - 0.3413193965f * s3;
            float b_linear = -0.0041960863f * l3 - 0.7034186147f * m3 + 1.7076147010f * s3;

            // Linear RGB to sRGB (gamma correction)
            float r = LinearToSRGB(r_linear);
            float g = LinearToSRGB(g_linear);
            float b_srgb = LinearToSRGB(b_linear);

            // Clamp to valid range
            return new Color(
                Mathf.Clamp01(r),
                Mathf.Clamp01(g),
                Mathf.Clamp01(b_srgb),
                1f
            );
        }

        /// <summary>
        /// Converts Unity Color (sRGB) to OKLCH.
        /// </summary>
        /// <param name="color">Unity Color</param>
        /// <returns>Tuple of (lightness, chroma, hue in degrees)</returns>
        public static (float l, float c, float h) ColorToOKLCH(Color color)
        {
            // sRGB to Linear RGB
            float r_linear = SRGBToLinear(color.r);
            float g_linear = SRGBToLinear(color.g);
            float b_linear = SRGBToLinear(color.b);

            // Linear RGB to OKLab
            float l_ = 0.4122214708f * r_linear + 0.5363325363f * g_linear + 0.0514459929f * b_linear;
            float m_ = 0.2119034982f * r_linear + 0.6806995451f * g_linear + 0.1073969566f * b_linear;
            float s_ = 0.0883024619f * r_linear + 0.2817188376f * g_linear + 0.6299787005f * b_linear;

            float l_cube = Mathf.Pow(l_, 1f / 3f);
            float m_cube = Mathf.Pow(m_, 1f / 3f);
            float s_cube = Mathf.Pow(s_, 1f / 3f);

            float L = 0.2104542553f * l_cube + 0.7936177850f * m_cube - 0.0040720468f * s_cube;
            float a = 1.9779984951f * l_cube - 2.4285922050f * m_cube + 0.4505937099f * s_cube;
            float b = 0.0259040371f * l_cube + 0.7827717662f * m_cube - 0.8086757660f * s_cube;

            // OKLab to OKLCH
            float C = Mathf.Sqrt(a * a + b * b);
            float H = Mathf.Atan2(b, a) * Mathf.Rad2Deg;
            if (H < 0) H += 360f;

            return (L, C, H);
        }

        // ============================================
        // HEX COLOR PARSING
        // ============================================
        
        /// <summary>
        /// Converts hex string to Unity Color.
        /// Supports formats: #RRGGBB, RRGGBB, #RGB, RGB
        /// </summary>
        public static Color HexToColor(string hex)
        {
            // Remove # if present
            hex = hex.TrimStart('#');

            // Handle 3-digit shorthand (e.g., "F0A" -> "FF00AA")
            if (hex.Length == 3)
            {
                hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
            }

            if (hex.Length != 6)
            {
                Debug.LogError($"Invalid hex color format: {hex}");
                return Color.magenta; // Error color
            }

            try
            {
                int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                int b = Convert.ToInt32(hex.Substring(4, 2), 16);

                return new Color(r / 255f, g / 255f, b / 255f);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse hex color {hex}: {e.Message}");
                return Color.magenta;
            }
        }

        /// <summary>
        /// Converts Unity Color to hex string (uppercase, with #).
        /// </summary>
        public static string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * 255f);
            int g = Mathf.RoundToInt(color.g * 255f);
            int b = Mathf.RoundToInt(color.b * 255f);
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        // ============================================
        // HELPER FUNCTIONS
        // ============================================
        
        private static float LinearToSRGB(float linear)
        {
            if (linear <= 0.0031308f)
                return 12.92f * linear;
            else
                return 1.055f * Mathf.Pow(linear, 1f / 2.4f) - 0.055f;
        }

        private static float SRGBToLinear(float srgb)
        {
            if (srgb <= 0.04045f)
                return srgb / 12.92f;
            else
                return Mathf.Pow((srgb + 0.055f) / 1.055f, 2.4f);
        }

        // ============================================
        // DEBUG/INSPECTOR UTILITIES
        // ============================================
        
        /// <summary>
        /// Prints detailed information about a color in both RGB and OKLCH.
        /// </summary>
        public static void DebugColor(Color color)
        {
            (float l, float c, float h) = ColorToOKLCH(color);
            Debug.Log($"Color Debug:\n" +
                      $"  RGB: ({color.r:F3}, {color.g:F3}, {color.b:F3})\n" +
                      $"  Hex: {ColorToHex(color)}\n" +
                      $"  OKLCH: L={l:F3}, C={c:F3}, H={h:F1}°");
        }

        /// <summary>
        /// Gets the tier parameters for inspection.
        /// </summary>
        public static TierParams GetTierParams(ColorTier tier)
        {
            return TierSettings[(int)tier];
        }
    }
}

