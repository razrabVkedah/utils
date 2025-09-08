using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Actions
{
    /// <summary>
    /// Ctrl/Cmd + Shift + M — создать новый материал в активной папке Project.
    /// </summary>
    public static class CreateMaterialHotkey
    {
        [Shortcut(HotkeyIds.CreateMaterial, KeyCode.M, ShortcutModifiers.Shift | ShortcutModifiers.Action)]
        public static void InvokeShortcut()
        {
            Execute();
        }

        public static void Execute()
        {
            var shader = HotkeyUtils.PickDefaultShader();
            var mat = new Material(shader) { name = "New Material" };
            HotkeyUtils.CreateAssetAndPing(mat, mat.name, "mat");
        }
    }
}