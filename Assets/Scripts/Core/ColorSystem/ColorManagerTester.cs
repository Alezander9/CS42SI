using UnityEngine;
using Core.ColorSystem;

namespace Core.ColorSystem
{
    /// <summary>
    /// Simple color picker for the 3-tier palette system.
    /// Select a tier, then either input a hex color or use the hue slider.
    /// </summary>
    public class ColorManagerTester : MonoBehaviour
    {
        [Header("Tier Selection")]
        [Tooltip("Select Background, Foreground, or Special")]
        public ColorManager.ColorTier selectedTier = ColorManager.ColorTier.Foreground;
        
        [Header("Method 1: From Reference Hex Color")]
        [Tooltip("Paste hex color from reference image")]
        public string hexInput = "#C87223";
        [Space(5)]
        [SerializeField] private Color hexInputPreview;
        [SerializeField] private Color hexOutputColor;
        [SerializeField] private string hexOutputHex;
        [SerializeField] private float hexOutputL;
        [SerializeField] private float hexOutputC;
        [SerializeField] private float hexOutputH;
        
        [Header("Method 2: From Hue Slider")]
        [Tooltip("Pick hue directly (0-360°)")]
        [Range(0f, 360f)]
        public float hueSlider = 45f;
        [Space(5)]
        [SerializeField] private Color hueOutputColor;
        [SerializeField] private string hueOutputHex;
        [SerializeField] private float hueOutputL;
        [SerializeField] private float hueOutputC;
        [SerializeField] private float hueOutputH;

        private void OnValidate()
        {
            UpdateFromHex();
            UpdateFromHue();
        }

        private void UpdateFromHex()
        {
            // Show original hex color
            hexInputPreview = ColorManager.HexToColor(hexInput);
            
            // Extract hue and apply to selected tier
            (float l, float c, float h) = ColorManager.ColorToOKLCH(hexInputPreview);
            hexOutputColor = ColorManager.CreateColorFromTier(selectedTier, h);
            
            // Update output fields
            hexOutputHex = ColorManager.ColorToHex(hexOutputColor);
            (hexOutputL, hexOutputC, hexOutputH) = ColorManager.ColorToOKLCH(hexOutputColor);
        }

        private void UpdateFromHue()
        {
            hueOutputColor = ColorManager.CreateColorFromTier(selectedTier, hueSlider);
            hueOutputHex = ColorManager.ColorToHex(hueOutputColor);
            (hueOutputL, hueOutputC, hueOutputH) = ColorManager.ColorToOKLCH(hueOutputColor);
        }

        [ContextMenu("Show All Tier Examples")]
        private void ShowAllTierExamples()
        {
            Debug.Log("=== Color Tier Examples (3-Tier System) ===");
            
            float[] exampleHues = { 30f, 45f, 60f, 120f, 180f, 240f, 270f }; // Orange, Brown, Yellow, Green, Cyan, Blue, Purple
            
            for (int tier = 0; tier <= 2; tier++)
            {
                ColorManager.ColorTier colorTier = (ColorManager.ColorTier)tier;
                var tierParams = ColorManager.GetTierParams(colorTier);
                
                Debug.Log($"\n--- Tier {tier}: {colorTier} (L={tierParams.Lightness:F2}, C={tierParams.Chroma:F2}) ---");
                
                // Show fewer hues for background (it's usually one color)
                float[] hues = (tier == 0) ? new float[] { 220f } : exampleHues;
                
                foreach (float hue in hues)
                {
                    Color c = ColorManager.CreateColorFromTier(colorTier, hue);
                    string hex = ColorManager.ColorToHex(c);
                    Debug.Log($"  H={hue:F0}°: {hex}");
                }
            }
        }

        [ContextMenu("Test Example Materials")]
        private void TestExampleMaterials()
        {
            Debug.Log("=== Example Material Colors (Foreground Tier) ===\n");

            string[] materials = new string[]
            {
                "Dirt", "Stone", "Clay", "Copper", "Iron", "Gold", "Ruby", "Emerald", "Sapphire"
            };
            
            float[] hues = new float[]
            {
                40f,   // Dirt - warm brown
                90f,   // Stone - cool gray
                35f,   // Clay - reddish brown
                30f,   // Copper - orange
                270f,  // Iron - purple-gray
                60f,   // Gold - yellow
                10f,   // Ruby - red
                145f,  // Emerald - green
                240f   // Sapphire - blue
            };

            for (int i = 0; i < materials.Length; i++)
            {
                Color c = ColorManager.CreateColorFromTier(ColorManager.ColorTier.Foreground, hues[i]);
                Debug.Log($"{materials[i]}: H={hues[i]:F0}° → {ColorManager.ColorToHex(c)}");
            }
        }
    }
}

