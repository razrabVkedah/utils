using UnityEditor;

namespace Rusleo.Utils.Editor.Hotkeys.Core
{
    /// <summary>
    /// Дублируем команды в меню как fallback и для discoverability.
    /// В названиях пунктов указываем комбинации в скобках чисто текстом,
    /// чтобы не было конфликтов с атрибутами [Shortcut].
    /// </summary>
    internal static class HotkeysMenuFallback
    {
        [MenuItem("Rusleo/Hotkeys/Create Material (Ctrl+Shift+M)")]
        private static void MenuCreateMaterial() => Actions.CreateMaterialHotkey.Execute();

        [MenuItem("Rusleo/Hotkeys/Select Current Scene (Ctrl+Shift+E)", priority = 1)]
        private static void MenuSelectScene() => Actions.SelectCurrentSceneHotkey.Execute();

        [MenuItem("Rusleo/Hotkeys/Show In Explorer (Ctrl+Shift+X)", priority = 10)]
        private static void MenuShowInExplorerSelected() => Actions.ShowInExplorerSelectedHotkey.Execute();

        [MenuItem("Rusleo/Hotkeys/Create Folder (Ctrl+Shift+N)", priority = 11)]
        private static void MenuCreateFolder() => Actions.CreateFolderHotkey.Execute();

        [MenuItem("Rusleo/Hotkeys/Focus Console (Ctrl+Shift+C)", priority = 12)]
        private static void MenuFocusConsole() => Actions.FocusConsoleHotkey.Execute();

        [MenuItem("Rusleo/Hotkeys/Reveal persistentDataPath (Ctrl+Shift+Alt+P)", priority = 13)]
        private static void MenuRevealPersistentData() => Actions.RevealPersistentDataHotkey.Execute();
    }
}