using System;
using System.Linq;
using UnityEngine;

namespace Rusleo.Utils.Editor.GradientStudio
{
    [Serializable]
    public class GradientJson
    {
        [Serializable]
        public class ColorKeyJson
        {
            [Serializable]
            public class ColorRGBA
            {
                public float r, g, b, a = 1f;
            }

            public ColorRGBA color = new ColorRGBA();
            public float time;
        }

        [Serializable]
        public class AlphaKeyJson
        {
            public float alpha;
            public float time;
        }

        public int mode = 0; // 0 = Blend, 1 = Fixed (соответствует Unity.GradientMode)
        public ColorKeyJson[] colorKeys;
        public AlphaKeyJson[] alphaKeys;

        // ---------- API ----------
        public static GradientJson FromGradient(Gradient gradient)
        {
            var g = gradient ?? new Gradient();
            var mode = (int)g.mode;

            var ck = g.colorKeys.Select(k => new ColorKeyJson
            {
                color = new ColorKeyJson.ColorRGBA { r = k.color.r, g = k.color.g, b = k.color.b, a = 1f },
                time = Mathf.Clamp01(k.time)
            }).ToArray();

            var ak = g.alphaKeys.Select(k => new AlphaKeyJson
            {
                alpha = Mathf.Clamp01(k.alpha),
                time = Mathf.Clamp01(k.time)
            }).ToArray();

            return new GradientJson { mode = mode, colorKeys = ck, alphaKeys = ak };
        }

        public Gradient ToGradient()
        {
            var g = new Gradient();
            var ck = (colorKeys ?? Array.Empty<ColorKeyJson>()).Select(k =>
                new GradientColorKey(
                    new Color(
                        Mathf.Clamp01(k.color?.r ?? 0f),
                        Mathf.Clamp01(k.color?.g ?? 0f),
                        Mathf.Clamp01(k.color?.b ?? 0f),
                        1f),
                    Mathf.Clamp01(k.time))
            ).ToArray();

            var ak = (alphaKeys ?? Array.Empty<AlphaKeyJson>()).Select(k =>
                new GradientAlphaKey(
                    Mathf.Clamp01(k.alpha),
                    Mathf.Clamp01(k.time))
            ).ToArray();

            // Edge cases: Unity требует >=1 ключа
            if (ck.Length == 0)
                ck = new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) };
            if (ak.Length == 0) ak = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

            g.SetKeys(ck, ak);
            g.mode = (GradientMode)Mathf.Clamp(mode, 0, 1);
            return g;
        }

        // Красивый HEX (#RRGGBB), альфа 0–255
        public static string ColorToHex(Color c) =>
            $"#{Mathf.RoundToInt(c.r * 255f):X2}{Mathf.RoundToInt(c.g * 255f):X2}{Mathf.RoundToInt(c.b * 255f):X2}";
    }
}