using System.IO;
using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    /// <summary>Создать новую папку в активной папке Project.</summary>
    public static class CreateFolderHotkey
    {
        [Shortcut(HotkeyIds.CreateFolder, KeyCode.N, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        public static void InvokeShortcut() => Execute();

        public static void Execute()
        {
            var folder = HotkeyUtils.GetActiveProjectFolder();
            var baseName = "New Folder";
            var uniqueName = baseName;
            int i = 1;

            while (AssetDatabase.IsValidFolder(Path.Combine(folder, uniqueName)))
                uniqueName = $"{baseName} {i++}";

            string guid = AssetDatabase.CreateFolder(folder, uniqueName);
            var createdPath = AssetDatabase.GUIDToAssetPath(guid);

            AssetDatabase.Refresh();
            var created = AssetDatabase.LoadAssetAtPath<Object>(createdPath);
            Selection.activeObject = created;
            EditorGUIUtility.PingObject(created);
            Debug.Log($"[Rusleo.Utils] Folder created: {createdPath}");
        }
    }
}