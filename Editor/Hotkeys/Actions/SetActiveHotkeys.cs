using System.Collections.Generic;
using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    internal static class SetActiveUtils
    {
        public static void SetActiveForSelection(bool? stateOrNull)
        {
            var gos = new List<GameObject>();
            foreach (var obj in Selection.gameObjects)
            {
                if (obj != null) gos.Add(obj);
            }

            if (gos.Count == 0)
            {
                Debug.LogWarning("[Rusleo.Utils] No GameObjects selected.");
                return;
            }

            // Готовим Undo-группу
            Undo.SetCurrentGroupName("Set Active (Rusleo)");
            int group = Undo.GetCurrentGroup();

            int changed = 0;
            foreach (var go in gos)
            {
                // Для Toggle: вычисляем новое значение из текущего
                bool newState = stateOrNull ?? !go.activeSelf;

                if (go.activeSelf == newState)
                    continue;

                Undo.RecordObject(go, "Set Active");
                go.SetActive(newState);
                changed++;
            }

            Undo.CollapseUndoOperations(group);

            if (changed > 0)
                Debug.Log($"[Rusleo.Utils] SetActive: changed {changed} object(s).");
            else
                Debug.Log("[Rusleo.Utils] SetActive: nothing to change.");
        }
    }

    public static class SetActiveToggleHotkey
    {
        [Shortcut(HotkeyIds.SetActiveToggle, KeyCode.H, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void Invoke() => Execute();

        public static void Execute() => SetActiveUtils.SetActiveForSelection(null);
    }

    public static class SetActiveOnHotkey
    {
        [Shortcut(HotkeyIds.SetActiveOn, KeyCode.U, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void Invoke() => Execute();

        public static void Execute() => SetActiveUtils.SetActiveForSelection(true);
    }

    public static class SetActiveOffHotkey
    {
        [Shortcut(HotkeyIds.SetActiveOff, KeyCode.J, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        public static void Invoke() => Execute();

        public static void Execute() => SetActiveUtils.SetActiveForSelection(false);
    }
}