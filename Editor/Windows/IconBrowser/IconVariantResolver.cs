using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.IconBrowser
{
    internal static class IconVariantResolver
    {
        private struct VariantInfo
        {
            public bool HasBase, HasDark, HasOn, HasDarkOn;
        }

        private static readonly Dictionary<string, VariantInfo> Cache = new(StringComparer.Ordinal);

        public static string NormalizeIconName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            name = name.Replace("@2x", string.Empty);
            while (name.Contains("  ")) name = name.Replace("  ", " ");
            return name.Trim();
        }

        private static bool Exists(string name) => EditorGUIUtility.FindTexture(name) != null;

        private static VariantInfo GetInfo(string baseName)
        {
            var key = NormalizeIconName(baseName);
            if (Cache.TryGetValue(key, out var info)) return info;

            string baseNorm = key;
            string dark = baseNorm.StartsWith("d_") ? baseNorm : "d_" + baseNorm;
            string on = baseNorm.EndsWith(" On") ? baseNorm : baseNorm + " On";
            string darkOn = on.StartsWith("d_") ? on : "d_" + on;

            info = new VariantInfo
            {
                HasBase = Exists(baseNorm),
                HasDark = Exists(dark),
                HasOn = Exists(on),
                HasDarkOn = Exists(darkOn)
            };
            Cache[key] = info;
            return info;
        }

        public static string ResolveSafeName(string baseName, bool preferDark, bool preferOn)
        {
            var norm = NormalizeIconName(baseName);
            var i = GetInfo(norm);

            if (preferDark && preferOn)
            {
                if (i.HasDarkOn) return MakeDarkOn(norm);
                if (i.HasDark) return MakeDark(norm);
                if (i.HasOn) return MakeOn(norm);
                if (i.HasBase) return norm;
            }
            else if (preferDark)
            {
                if (i.HasDark) return MakeDark(norm);
                if (i.HasBase) return norm;
            }
            else if (preferOn)
            {
                if (i.HasOn) return MakeOn(norm);
                if (i.HasBase) return norm;
            }
            else if (i.HasBase) return norm;

            if (i.HasDarkOn) return MakeDarkOn(norm);
            if (i.HasDark) return MakeDark(norm);
            if (i.HasOn) return MakeOn(norm);
            return norm;
        }

        public static GUIContent SafeIconContent(string name) => EditorGUIUtility.IconContent(name);

        private static string MakeDark(string baseName) => baseName.StartsWith("d_") ? baseName : "d_" + baseName;
        private static string MakeOn(string baseName) => baseName.EndsWith(" On") ? baseName : baseName + " On";
        private static string MakeDarkOn(string baseName) => MakeOn(MakeDark(baseName));
    }
}