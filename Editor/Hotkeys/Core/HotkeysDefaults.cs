using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Core
{
    internal static class HotkeysDefaults
    {
        // Словарь "id -> дефолтный биндинг пакета"
        private static readonly Dictionary<string, ShortcutBinding> _defaults = new()
        {
            // Материал: Ctrl/Cmd + Shift + M
            { HotkeyIds.CreateMaterial, Make(KeyCode.M, action: true, shift: true, alt: false) },

            // Выбрать текущую сцену: Ctrl/Cmd + Shift + E
            { HotkeyIds.SelectCurrentScene, Make(KeyCode.E, action: true, shift: true, alt: false) },

            // Показать ассет в проводнике: Ctrl/Cmd + Shift + X
            { HotkeyIds.ShowInExplorerSelected, Make(KeyCode.X, action: true, shift: true, alt: false) },

            // Новая папка: Ctrl/Cmd + Shift + N
            { HotkeyIds.CreateFolder, Make(KeyCode.N, action: true, shift: true, alt: false) },

            // Консоль: Ctrl/Cmd + Shift + C
            { HotkeyIds.FocusConsole, Make(KeyCode.C, action: true, shift: true, alt: false) },

            // persistentDataPath: Ctrl/Cmd + Shift + Alt + P
            { HotkeyIds.RevealPersistentData, Make(KeyCode.P, action: true, shift: true, alt: true) },
            
            // Ctrl/Cmd+Shift+H
            { HotkeyIds.SetActiveToggle, Make(KeyCode.H, action: true, shift: true, alt: false) },
            
            // Ctrl/Cmd+Shift+U
            { HotkeyIds.SetActiveOn, Make(KeyCode.U, action: true, shift: true, alt: false) },
            
            // Ctrl/Cmd+Shift+J
            { HotkeyIds.SetActiveOff, Make(KeyCode.J, action: true, shift: true, alt: false) },
        };

        public static bool TryGet(string id, out ShortcutBinding binding) => _defaults.TryGetValue(id, out binding);

        private static ShortcutBinding Make(KeyCode key, bool action, bool shift, bool alt)
        {
            ShortcutModifiers mods = ShortcutModifiers.None;
            if (action) mods |= ShortcutModifiers.Action;
            if (shift) mods |= ShortcutModifiers.Shift;
            if (alt) mods |= ShortcutModifiers.Alt;
            return new ShortcutBinding(new KeyCombination(key, mods));
        }
    }
}