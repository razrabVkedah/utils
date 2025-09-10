using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.IconBrowser
{
    /// <summary>
    /// Wide, robust icon discovery across Editor versions.
    /// Sources: loaded Texture2D, GUISkin states (all skins), type icons (ObjectContent),
    /// legacy seeds, and safe probing via FindTexture. Optionally expands morphology
    /// (adds/strips " Icon", etc.) and variants (d_, " On").
    /// </summary>
    internal class IconIndex
    {
        private readonly List<IconItem> _items = new();
        private readonly Dictionary<string, IconItem> _byName = new(StringComparer.Ordinal);
        private int _fingerprint;

        public int Count => _items.Count;
        public IReadOnlyList<IconItem> Items => _items;

        public struct IconItem
        {
            public string Name; // display name
            public Texture2D Icon; // preview
            public int Id; // stable id based on name (Animator.StringToHash)
        }

        public void Rebuild(bool useLegacySeeds, bool expandMorphology, bool aggressiveLegacy)
        {
            _items.Clear();
            _byName.Clear();

            var candidates = new HashSet<string>(StringComparer.Ordinal);

            // 1) Everything already loaded as Texture2D
            foreach (var tex in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (tex == null) continue;
                if (string.IsNullOrEmpty(tex.name)) continue;
                candidates.Add(IconVariantResolver.NormalizeIconName(tex.name));
            }

            // 2) GUISkins from resources and builtin skins
            foreach (var skin in Resources.FindObjectsOfTypeAll<GUISkin>())
                CollectFromSkin(candidates, skin);
            CollectFromSkin(candidates, EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector));
            CollectFromSkin(candidates, EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene));
            CollectFromSkin(candidates, EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game));

            // 3) Type icons via ObjectContent(null, type)
            AddTypeIcons(candidates);

            // 4) Legacy seeds (programmatic) — «старый» широкий охват
            if (useLegacySeeds)
            {
                foreach (var s in LegacyIconSeeding.GetSeeds())
                    candidates.Add(IconVariantResolver.NormalizeIconName(s));
            }

            // 5) Probe names and their variants
            foreach (var baseName in candidates)
                ProbeAllForms(baseName, expandMorphology);

            if (aggressiveLegacy)
                AggressiveLegacyEnumerate(); // объединит с уже найденным


            _items.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            _fingerprint = ComputeFingerprint(useLegacySeeds, expandMorphology, aggressiveLegacy);
        }

        private void AggressiveLegacyEnumerate()
        {
            // NB: Не нормализуем имя — оставляем 1:1 как в legacy (чтобы не потерять @2x и прочие варианты)
            foreach (var x in Resources.FindObjectsOfTypeAll<Texture2D>())
            {
                if (x == null) continue;
                if (x.name.Length == 0) continue;

                // Точно как в legacy
                if (x.hideFlags != HideFlags.HideAndDontSave &&
                    x.hideFlags != (HideFlags.HideInInspector | HideFlags.HideAndDontSave))
                    continue;

                if (!EditorUtility.IsPersistent(x))
                    continue;

                // «Тихо» дергаем IconContent — временно выключаем логгер
                bool prev = Debug.unityLogger.logEnabled;
                Debug.unityLogger.logEnabled = false;
                GUIContent gc = EditorGUIUtility.IconContent(x.name);
                Debug.unityLogger.logEnabled = prev;

                if (gc == null) continue;
                var tex = gc.image as Texture2D;
                if (tex == null) continue;

                string name = x.name; // без Normalize, чтобы не потерять «дубликаты» вроде @2x
                if (_byName.ContainsKey(name)) continue;

                int id = Animator.StringToHash(name); // стабильнее, чем GetHashCode
                var item = new IconItem { Name = name, Icon = tex, Id = id };
                _byName.Add(name, item);
                _items.Add(item);
            }
        }


        public bool RefreshIfChanged(bool useLegacySeeds, bool expandMorphology, bool aggressiveLegacy)
        {
            var current = ComputeFingerprint(useLegacySeeds, expandMorphology, aggressiveLegacy);
            if (current == _fingerprint) return false;
            Rebuild(useLegacySeeds, expandMorphology, aggressiveLegacy);
            return true;
        }

        private int ComputeFingerprint(bool useLegacySeeds, bool expandMorphology, bool aggressiveLegacy)
        {
            unchecked
            {
                int h = 17;
                foreach (var tex in Resources.FindObjectsOfTypeAll<Texture2D>())
                {
                    if (tex == null || string.IsNullOrEmpty(tex.name)) continue;
                    var n = IconVariantResolver.NormalizeIconName(tex.name);
                    h = (h * 31) ^ Animator.StringToHash(n);
                }

                foreach (var skin in Resources.FindObjectsOfTypeAll<GUISkin>())
                {
                    if (skin == null) continue;
                    h = (h * 31) ^ skin.GetInstanceID();
                }

                // include settings so toggles force refresh
                h = (h * 31) ^ (useLegacySeeds ? 1 : 0);
                h = (h * 31) ^ (expandMorphology ? 1 : 0);
                h = (h * 31) ^ (aggressiveLegacy ? 1 : 0);
                return h;
            }
        }

        // ----------------------- Discovery helpers ----------------------------
        private static void CollectFromStyleState(HashSet<string> names, GUIStyleState st)
        {
            var bg = st?.background;
            if (!bg) return;
            if (string.IsNullOrEmpty(bg.name)) return;
            names.Add(IconVariantResolver.NormalizeIconName(bg.name));
        }

        private static void CollectFromStyle(HashSet<string> names, GUIStyle style)
        {
            if (style == null) return;
            CollectFromStyleState(names, style.normal);
            CollectFromStyleState(names, style.hover);
            CollectFromStyleState(names, style.active);
            CollectFromStyleState(names, style.focused);
            CollectFromStyleState(names, style.onNormal);
            CollectFromStyleState(names, style.onHover);
            CollectFromStyleState(names, style.onActive);
            CollectFromStyleState(names, style.onFocused);
        }

        private static void CollectFromSkin(HashSet<string> names, GUISkin skin)
        {
            if (skin == null) return;
            CollectFromStyle(names, skin.box);
            CollectFromStyle(names, skin.button);
            CollectFromStyle(names, skin.toggle);
            CollectFromStyle(names, skin.label);
            CollectFromStyle(names, skin.textField);
            CollectFromStyle(names, skin.textArea);
            CollectFromStyle(names, skin.window);
            CollectFromStyle(names, skin.horizontalSlider);
            CollectFromStyle(names, skin.horizontalSliderThumb);
            CollectFromStyle(names, skin.verticalSlider);
            CollectFromStyle(names, skin.verticalSliderThumb);
            CollectFromStyle(names, skin.horizontalScrollbar);
            CollectFromStyle(names, skin.horizontalScrollbarThumb);
            CollectFromStyle(names, skin.horizontalScrollbarLeftButton);
            CollectFromStyle(names, skin.horizontalScrollbarRightButton);
            CollectFromStyle(names, skin.verticalScrollbar);
            CollectFromStyle(names, skin.verticalScrollbarThumb);
            CollectFromStyle(names, skin.verticalScrollbarUpButton);
            CollectFromStyle(names, skin.verticalScrollbarDownButton);
            CollectFromStyle(names, skin.scrollView);

            if (skin.customStyles != null)
                foreach (var s in skin.customStyles)
                    CollectFromStyle(names, s);
        }

        private static IEnumerable<Type> SafeTypesFrom(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types?.Where(t => t != null) ?? Array.Empty<Type>();
            }
        }

        private static void AddTypeIcons(HashSet<string> names)
        {
            var asms = new[] { typeof(EditorWindow).Assembly, typeof(UnityEngine.Object).Assembly };
            foreach (var asm in asms)
            foreach (var t in SafeTypesFrom(asm))
            {
                if (t == null) continue;
                if (!typeof(UnityEngine.Object).IsAssignableFrom(t)) continue;
                var gc = EditorGUIUtility.ObjectContent(null, t);
                var tex = gc?.image as Texture2D;
                if (tex == null) continue;
                if (string.IsNullOrEmpty(tex.name)) continue;
                names.Add(IconVariantResolver.NormalizeIconName(tex.name));
            }
        }

        private void ProbeAllForms(string baseName, bool expandMorphology)
        {
            // base variants
            ProbeAndAdd(baseName);
            ProbeAndAdd("d_" + baseName);
            ProbeAndAdd(baseName + " On");
            ProbeAndAdd("d_" + baseName + " On");

            if (!expandMorphology) return;

            // Morphology expansions: add/strip " Icon", " Icon Small"
            var stripped = baseName.EndsWith(" Icon", StringComparison.Ordinal)
                ? baseName.Substring(0, baseName.Length - " Icon".Length)
                : baseName;

            // add common suffixes
            ProbeAndAdd(stripped + " Icon");
            ProbeAndAdd(stripped + " Icon Small");
            ProbeAndAdd(stripped + " Icon Asset");

            // try without double spaces
            var noDblSpace = stripped.Replace("  ", " ");
            ProbeAndAdd(noDblSpace);
        }

        private void ProbeAndAdd(string candidate)
        {
            var name = IconVariantResolver.NormalizeIconName(candidate);
            var tex = EditorGUIUtility.FindTexture(name); // тихо, без логов
            if (!tex) return;
            if (_byName.ContainsKey(name)) return;

            int id = Animator.StringToHash(name);
            var item = new IconItem { Name = name, Icon = tex, Id = id };
            _byName.Add(name, item);
            _items.Add(item);
        }
    }
}