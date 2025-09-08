using System.Linq;
using System.Text;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Hotkeys.Core
{
    internal static class ShortcutBindingUtils
    {
        public static string ToHumanString(ShortcutBinding binding)
        {
            if (binding.keyCombinationSequence == null)
                return "—";

            var combos = binding.keyCombinationSequence.ToList();
            if (combos.Count == 0)
                return "—";

            var sb = new StringBuilder();
            for (int i = 0; i < combos.Count; i++)
            {
                if (i > 0) sb.Append(", then ");

                var kc = combos[i];

                // Action = Ctrl (Windows) / Cmd (macOS)
                if (kc.action) sb.Append(Application.platform == RuntimePlatform.OSXEditor ? "Cmd+" : "Ctrl+");
                if (kc.shift) sb.Append("Shift+");
                if (kc.alt) sb.Append("Alt+");
                sb.Append(kc.keyCode);
            }

            return sb.ToString();
        }

        public static ShortcutBinding BuildBinding(KeyCode key, bool action, bool shift, bool alt)
        {
            ShortcutModifiers mods = ShortcutModifiers.None;
            if (action) mods |= ShortcutModifiers.Action;
            if (shift) mods |= ShortcutModifiers.Shift;
            if (alt) mods |= ShortcutModifiers.Alt;

            var combo = new KeyCombination(key, mods);
            return new ShortcutBinding(combo);
        }
    }
}