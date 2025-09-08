namespace Rusleo.Utils.Editor.Hotkeys.Core
{
    /// <summary>
    /// Единая точка правды для строковых ID шорткатов.
    /// </summary>
    public static class HotkeyIds
    {
        // Группа (namespace) в ShortcutManager
        public const string Group = "Rusleo/Hotkeys";

        // Конкретные действия
        public const string CreateMaterial = Group + "/Create Material";
        public const string SelectCurrentScene = Group + "/Select Current Scene";
        public const string ShowInExplorerSelected = Group + "/Show In Explorer (Selected Asset)";
        public const string CreateFolder = Group + "/Create Folder";
        public const string FocusConsole = Group + "/Focus Console";
        public const string RevealPersistentData = Group + "/Reveal PersistentDataPath";

    }
}