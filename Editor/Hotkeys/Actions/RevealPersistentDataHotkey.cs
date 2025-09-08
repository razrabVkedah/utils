using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    /// <summary>Открыть проводник на Application.persistentDataPath.</summary>
    public static class RevealPersistentDataHotkey
    {
        [Shortcut(HotkeyIds.RevealPersistentData,
            KeyCode.P, ShortcutModifiers.Shift | ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        public static void InvokeShortcut() => Execute();

        public static void Execute()
        {
            var path = Application.persistentDataPath;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[Rusleo.Utils] persistentDataPath пуст.");
                return;
            }
            EditorUtility.RevealInFinder(path);
        }
    }
}