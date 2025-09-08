using System.IO;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Core
{
/// <summary>
    /// Общие утилиты, используемые действиями хоткеев.
    /// </summary>
    public static class HotkeyUtils
    {
        /// <summary>
        /// Возвращает активную папку Project (если недоступно — "Assets").
        /// </summary>
        public static string GetActiveProjectFolder()
        {
            var method = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            if (method != null)
            {
                object result = method.Invoke(null, null);
                if (result is string path && !string.IsNullOrEmpty(path))
                    return path;
            }
            return "Assets";
        }

        /// <summary>
        /// Выбирает подходящий «дефолтный» шейдер под текущий Render Pipeline.
        /// </summary>
        public static Shader PickDefaultShader()
        {
            var rp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (rp != null && rp.GetType().Name.IndexOf("Universal", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Unlit/Texture");
            if (rp != null && rp.GetType().Name.IndexOf("HDRenderPipeline", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return Shader.Find("HDRP/Lit") ?? Shader.Find("Unlit/Texture");
            return Shader.Find("Standard") ?? Shader.Find("Unlit/Texture");
        }

        /// <summary>
        /// Создаёт Scriptable-ассет в активной папке и пингует его.
        /// </summary>
        public static void CreateAssetAndPing(Object obj, string baseName, string extensionWithoutDot)
        {
            string folder = GetActiveProjectFolder();
            string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, $"{baseName}.{extensionWithoutDot}"));
            AssetDatabase.CreateAsset(obj, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
            Debug.Log($"[Rusleo.Utils] Created: {path}");
        }
    }
}