using System;
using System.Collections.Generic;

namespace Rusleo.Utils.Editor.Windows.IconBrowser
{
    /// <summary>
    /// Programmatic legacy seeding: a curated set of base names + generators to emulate
    /// the "old" wide coverage (1000+). FindTexture will silently drop non-existing ones.
    /// </summary>
    internal static class LegacyIconSeeding
    {
        public static IEnumerable<string> GetSeeds()
        {
            // Base buckets (150+ base tokens) — расширяй по мере надобности
            // Список подобран по реальным встроенным именам Unity (без @2x),
            // но FindTexture сам профильтрует «лишнее» под конкретную версию редактора.
            string[] baseTokens = new[]
            {
                // Color Picker
                "ColorPicker-HueRing", "ColorPicker-AlphaGradient", "ColorPicker-Background",
                "ColorPicker-Exposure", "ColorPicker-Checker", "ColorPicker-ColorCycle",

                // Common toolbar / filters
                "FilterByType", "FilterSelectedOnly", "Search Icon", "Toolbar Plus", "Toolbar Minus",
                "Animation.AddKeyframe", "Animation.AddEvent", "Animation.Play", "Animation.PrevKey",
                "Animation.NextKey",

                // Prefab / Assets
                "Prefab Icon", "PrefabModel Icon", "GameObject Icon", "SceneAsset Icon", "Folder Icon",
                "Material Icon", "Texture Icon", "Sprite Icon", "ScriptableObject Icon", "Shader Icon",
                "ComputeShader Icon",
                "AnimationClip Icon", "AnimatorController Icon", "AnimatorState Icon", "AnimatorStateMachine Icon",
                "AudioClip Icon", "AudioMixerController Icon", "Font Icon", "GUISkin Icon", "Light Icon", "Camera Icon",
                "PhysicMaterial Icon", "Mesh Icon", "SkinnedMeshRenderer Icon", "MeshRenderer Icon", "Terrain Icon",

                // Profiler/Timeline-ish
                "Profiler.CPU", "Profiler.GPU", "Profiler.Memory", "Profiler.NetworkOperations", "Profiler.Rendering",
                "Profiler.UI", "Profiler.GlobalIllumination", "Profiler.Audio", "Profiler.Video", "Profiler.Physics",

                // Misc
                "PlayButton", "PauseButton", "StepButton", "Collab", "UnityEditor.InspectorWindow",
                "SceneViewOrtho", "SceneViewFx", "SceneViewLighting",
            };

            foreach (var t in baseTokens)
                yield return t;

            // Small variants often exist (Unity packs some icons with " Icon Small")
            foreach (var t in baseTokens)
            {
                if (!t.EndsWith(" Icon", StringComparison.Ordinal)) continue;
                yield return t + " Small"; // e.g., "Prefab Icon Small"
            }

            // Extra hand-crafted names for older skins
            string[] extra = new[]
            {
                "d_FilterByType", "d_SceneViewFx", "d_SceneViewLighting", "AvatarCompass",
                "BuildSettings.Editor", "BuildSettings.Metro", "BuildSettings.Android",
                "BuildSettings.iPhone", "BuildSettings.tvOS", "BuildSettings.WebGL"
            };
            foreach (var e in extra) yield return e;
        }
    }
}