using UnityEngine;
using Core.ColorSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core.ColorSystem
{
    /// <summary>
    /// Development tool for tuning and previewing colors in the 3-tier palette system.
    /// Attach to any GameObject in the scene to use.
    /// </summary>
    [ExecuteInEditMode]
    public class ColorManagerTester : MonoBehaviour
    {
        [Header("═══ TIER SELECTION ═══")]
        [Tooltip("Select which tier to work with")]
        public ColorManager.ColorTier selectedTier = ColorManager.ColorTier.Foreground;
        
        [Header("═══ METHOD 1: From Reference Color ═══")]
        [Tooltip("Paste hex from color picker (e.g., #C87223)")]
        public string hexInput = "#C87223";
        [Space(5)]
        [Tooltip("Original color from hex input")]
        public Color hexInputPreview;
        [Tooltip("Color adjusted to selected tier")]
        public Color hexOutputColor;
        [Space(3)]
        [SerializeField] private string hexOutputHex;
        [SerializeField] private float hexOutputL;
        [SerializeField] private float hexOutputC;
        [SerializeField] private float hexOutputH;
        
        [Header("═══ METHOD 2: Manual Tuning ═══")]
        [Tooltip("Hue angle: 0°=red, 60°=yellow, 120°=green, 180°=cyan, 240°=blue, 300°=magenta")]
        [Range(0f, 360f)]
        public float hueSlider = 40f;
        [Tooltip("Chroma (saturation): Lower=gray/earthy, Higher=vibrant. Will be clamped to tier range.")]
        [Range(0f, 0.35f)]
        public float chromaSlider = 0.10f;
        [Space(5)]
        [Tooltip("Result of manual tuning")]
        public Color manualOutputColor;
        [Space(3)]
        [SerializeField] private string manualOutputHex;
        [SerializeField] private float manualOutputL;
        [SerializeField] private float manualOutputC;
        [SerializeField] private float manualOutputH;
        
        [Header("═══ CHROMA COMPARISON ═══")]
        [Tooltip("Compare same hue at different saturation levels")]
        public bool showChromaComparison = true;
        [Space(5)]
        [Tooltip("Low chroma (gray/earthy)")]
        public Color chromaLow;
        [Tooltip("Mid chroma (moderate)")]
        public Color chromaMid;
        [Tooltip("High chroma (vibrant)")]
        public Color chromaHigh;
        
        [Header("═══ TIER INFO ═══")]
        [SerializeField] private string tierLightness;
        [SerializeField] private string tierChromaRange;

        private void OnValidate()
        {
            UpdateAllColors();
        }

        private void UpdateAllColors()
        {
            // Update tier info
            var tierParams = ColorManager.GetTierParams(selectedTier);
            tierLightness = $"L = {tierParams.Lightness:F2}";
            tierChromaRange = $"C = {tierParams.ChromaMin:F2} to {tierParams.ChromaMax:F2}";
            
            // Method 1: From hex
            UpdateFromHex();
            
            // Method 2: Manual
            UpdateFromManual();
            
            // Chroma comparison
            if (showChromaComparison)
            {
                UpdateChromaComparison();
            }
        }

        private void UpdateFromHex()
        {
            // Show original hex color
            hexInputPreview = ColorManager.HexToColor(hexInput);
            
            // Extract hue and chroma, apply to selected tier
            (float l, float c, float h) = ColorManager.ColorToOKLCH(hexInputPreview);
            hexOutputColor = ColorManager.CreateColorFromTier(selectedTier, h, c);
            
            // Update output fields
            hexOutputHex = ColorManager.ColorToHex(hexOutputColor);
            (hexOutputL, hexOutputC, hexOutputH) = ColorManager.ColorToOKLCH(hexOutputColor);
        }

        private void UpdateFromManual()
        {
            manualOutputColor = ColorManager.CreateColorFromTier(selectedTier, hueSlider, chromaSlider);
            manualOutputHex = ColorManager.ColorToHex(manualOutputColor);
            (manualOutputL, manualOutputC, manualOutputH) = ColorManager.ColorToOKLCH(manualOutputColor);
        }
        
        private void UpdateChromaComparison()
        {
            var tierParams = ColorManager.GetTierParams(selectedTier);
            float lowC = tierParams.ChromaMin;
            float midC = (tierParams.ChromaMin + tierParams.ChromaMax) / 2f;
            float highC = tierParams.ChromaMax;
            
            chromaLow = ColorManager.CreateColorFromTier(selectedTier, hueSlider, lowC);
            chromaMid = ColorManager.CreateColorFromTier(selectedTier, hueSlider, midC);
            chromaHigh = ColorManager.CreateColorFromTier(selectedTier, hueSlider, highC);
        }
        
        [ContextMenu("Copy Manual Output as Code")]
        private void CopyManualOutputCode()
        {
            string code = $"new TileColorDefinition({hueSlider:F1}f, {chromaSlider:F2}f, ColorManager.ColorTier.{selectedTier})";
            GUIUtility.systemCopyBuffer = code;
            print($"Copied to clipboard:\n{code}\nResult: {manualOutputHex}");
        }
        
        [ContextMenu("Copy Hex Output as Code")]
        private void CopyHexOutputCode()
        {
            string code = $"new TileColorDefinition({hexOutputH:F1}f, {hexOutputC:F2}f, ColorManager.ColorTier.{selectedTier})";
            GUIUtility.systemCopyBuffer = code;
            print($"Copied to clipboard:\n{code}\nResult: {hexOutputHex}");
        }

        [ContextMenu("📊 Show All Tier Examples")]
        private void ShowAllTierExamples()
        {
            print("=== Color Tier Examples (3-Tier System) ===");
            
            float[] exampleHues = { 0f, 30f, 60f, 120f, 180f, 240f, 300f };
            
            for (int tier = 0; tier <= 2; tier++)
            {
                ColorManager.ColorTier colorTier = (ColorManager.ColorTier)tier;
                var tierParams = ColorManager.GetTierParams(colorTier);
                
                print($"\n--- Tier {tier}: {colorTier} (L={tierParams.Lightness:F2}, C={tierParams.ChromaMin:F2}-{tierParams.ChromaMax:F2}) ---");
                
                float[] hues = (tier == 0) ? new float[] { 220f } : exampleHues;
                float midChroma = (tierParams.ChromaMin + tierParams.ChromaMax) / 2f;
                
                foreach (float hue in hues)
                {
                    Color c = ColorManager.CreateColorFromTier(colorTier, hue, midChroma);
                    string hex = ColorManager.ColorToHex(c);
                    print($"  H={hue:F0}°, C={midChroma:F2}: {hex}");
                }
            }
        }
        
        [ContextMenu("🎨 Show Hue Wheel for Current Tier")]
        private void ShowHueWheelForTier()
        {
            var tierParams = ColorManager.GetTierParams(selectedTier);
            float midChroma = (tierParams.ChromaMin + tierParams.ChromaMax) / 2f;
            
            print($"=== Hue Wheel: {selectedTier} Tier (C={midChroma:F2}) ===\n");
            
            for (int h = 0; h < 360; h += 30)
            {
                Color c = ColorManager.CreateColorFromTier(selectedTier, h, midChroma);
                string hex = ColorManager.ColorToHex(c);
                string name = GetHueName(h);
                print($"{name,-12} (H={h,3}°): {hex}");
            }
        }
        
        private string GetHueName(int hue)
        {
            if (hue >= 345 || hue < 15) return "Red";
            if (hue < 45) return "Orange";
            if (hue < 75) return "Yellow";
            if (hue < 105) return "Yellow-Green";
            if (hue < 135) return "Green";
            if (hue < 165) return "Cyan-Green";
            if (hue < 195) return "Cyan";
            if (hue < 225) return "Blue";
            if (hue < 255) return "Blue";
            if (hue < 285) return "Purple";
            if (hue < 315) return "Magenta";
            return "Pink";
        }

        [ContextMenu("🪨 Test Example Materials")]
        private void TestExampleMaterials()
        {
            print("=== Example Material Colors (Foreground Tier) ===\n");

            string[] materials = new string[]
            {
                "Stone", "Dirt", "Clay", "Bedrock", "Iron", "Copper", "Gold", "Ruby", "Emerald", "Sapphire", "Amethyst"
            };
            
            float[] hues = new float[]
            {
                220f, 40f, 35f, 200f, 240f, 30f, 60f, 10f, 145f, 240f, 280f
            };
            
            float[] chromas = new float[]
            {
                0.04f, 0.10f, 0.12f, 0.03f, 0.06f, 0.14f, 0.16f, 0.18f, 0.15f, 0.16f, 0.17f
            };

            for (int i = 0; i < materials.Length; i++)
            {
                Color c = ColorManager.CreateColorFromTier(ColorManager.ColorTier.Foreground, hues[i], chromas[i]);
                string code = $"new TileColorDefinition({hues[i]:F1}f, {chromas[i]:F2}f, ColorManager.ColorTier.Foreground)";
                print($"{materials[i],-10} → {ColorManager.ColorToHex(c)}  |  {code}");
            }
        }
        
        [ContextMenu("🎯 Compare Current Hue at All Tiers")]
        private void CompareHueAcrossTiers()
        {
            print($"=== Hue {hueSlider:F0}° across all tiers ===\n");
            
            for (int tier = 0; tier <= 2; tier++)
            {
                ColorManager.ColorTier colorTier = (ColorManager.ColorTier)tier;
                var tierParams = ColorManager.GetTierParams(colorTier);
                float midChroma = (tierParams.ChromaMin + tierParams.ChromaMax) / 2f;
                
                Color c = ColorManager.CreateColorFromTier(colorTier, hueSlider, midChroma);
                string hex = ColorManager.ColorToHex(c);
                print($"Tier {tier} ({colorTier}): L={tierParams.Lightness:F2}, C={midChroma:F2} → {hex}");
            }
        }
        
        [ContextMenu("📋 Generate Code for Current Manual Color")]
        private void GenerateCodeForManualColor()
        {
            print("=== Generated Code ===\n");
            print($"// Color: {manualOutputHex}");
            print($"ColorDef = new TileColorDefinition({hueSlider:F1}f, {chromaSlider:F2}f, ColorManager.ColorTier.{selectedTier})");
            print($"\n// Or directly:");
            print($"Color color = ColorManager.CreateColorFromTier(ColorManager.ColorTier.{selectedTier}, {hueSlider:F1}f, {chromaSlider:F2}f);");
        }
    }
}

