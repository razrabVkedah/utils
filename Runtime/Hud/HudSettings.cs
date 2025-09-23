using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud
{
    [CreateAssetMenu(fileName = "DefaultHudSettings", menuName = "Rusleo/Utils/HUD Settings")]
    public sealed class HudSettings : ScriptableObject
    {
        public enum AnchorCorner
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Header("Common")] public bool visible = true;
        public bool showHeader = true;
        public string headerText = "Rusleo HUD";

        [Header("Layout")] 
        public AnchorCorner anchor = AnchorCorner.TopLeft;
        public Vector2Int margin = new(8, 8);
        public int padding = 5;

        [Header("Style")] public Font font;
        [Range(6,64)]  public int minFontSize = 10;
        [Range(6,128)] public int maxFontSize = 100;
        public bool wordWrap = true;
        public Color textColor = Color.white;
        public Color backgroundColor = new(0, 0, 0, 0.5f);
        public bool drawBorder = true;

        public Color borderColor = new(0.85f, 1, 0.775f, 0.192f);


        [Header("Sizing (percent of screen)")]
        [Range(1, 100)] public int widthPercent = 20;
        [Range(0, 100)] public int heightPercent = 15;

        [HideInInspector] public Texture2D white1X1;
        
        [Header("Metrics (Init only)")]
        [SerializeField] private bool fpsMetrics = true;
        [SerializeField] private bool frameMetrics = true;
        [SerializeField] private bool memoryMetrics = true;
        [SerializeField] private bool gcMetrics = true;
        [SerializeField] private bool cpuMetrics = true;
        
        public bool FPSMetrics => fpsMetrics;
        public bool FrameMetrics => frameMetrics;
        public bool MemoryMetrics => memoryMetrics;
        public bool GcMetrics => gcMetrics;
        public bool CpuMetrics => cpuMetrics;

        public Texture2D GetSolidTex(Color c)
        {
            if (white1X1) return white1X1;

            white1X1 = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            white1X1.SetPixel(0, 0, Color.white);
            white1X1.Apply();

            return white1X1;
        }
    }
}