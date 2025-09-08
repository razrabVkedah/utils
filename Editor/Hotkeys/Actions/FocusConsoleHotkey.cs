using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    /// <summary>Открыть и сфокусировать Console.</summary>
    public static class FocusConsoleHotkey
    {
        [Shortcut(HotkeyIds.FocusConsole, KeyCode.C, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        public static void InvokeShortcut() => Execute();

        public static void Execute()
        {
            // Надежнее через меню, чем рефлексией на внутренние типы.
            if (!EditorApplication.ExecuteMenuItem("Window/General/Console"))
                Debug.LogWarning("[Rusleo.Utils] Не удалось открыть Console через меню.");
        }
    }
}