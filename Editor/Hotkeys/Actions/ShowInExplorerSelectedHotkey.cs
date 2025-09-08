using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    /// <summary>Показать выделенный ассет в системном проводнике.</summary>
    public static class ShowInExplorerSelectedHotkey
    {
        [Shortcut(HotkeyIds.ShowInExplorerSelected, KeyCode.X, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        public static void InvokeShortcut() => Execute();

        public static void Execute()
        {
            var obj = Selection.activeObject;
            if (obj == null)
            {
                Debug.LogWarning("[Rusleo.Utils] Ничего не выбрано в Project.");
                return;
            }

            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[Rusleo.Utils] Объект не является ассетом Project.");
                return;
            }

            EditorUtility.RevealInFinder(path);
        }
    }
}