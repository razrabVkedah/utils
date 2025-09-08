using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    /// <summary>
    /// Ctrl/Cmd + Shift + E — выбрать текущую сцену (asset) в Project.
    /// </summary>
    public static class SelectCurrentSceneHotkey
    {
        [Shortcut(HotkeyIds.SelectCurrentScene, KeyCode.E, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        public static void InvokeShortcut()
        {
            Execute();
        }

        public static void Execute()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogWarning("[Rusleo.Utils] Active scene is invalid.");
                return;
            }

            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("[Rusleo.Utils] Scene not saved. Save it first.");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            if (asset == null)
            {
                Debug.LogWarning("[Rusleo.Utils] Scene asset not found: " + scene.path);
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}