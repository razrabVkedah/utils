using System.Collections.Generic;

namespace Rusleo.Utils.Editor.Hotkeys.Core
{
    /// <summary>
    /// Каталог хоткеев (ID -> человекочитаемое имя/описание).
    /// Добавляй сюда новые команды по мере появления.
    /// </summary>
    public static class HotkeysCatalog
    {
        public sealed class Entry
        {
            public string Id;
            public string DisplayName;
            public string Description;
        }

        private static readonly List<Entry> _all = new()
        {
            new Entry
            {
                Id = HotkeyIds.CreateMaterial,
                DisplayName = "Create Material",
                Description = "Создать новый материал в активной папке Project."
            },
            new Entry
            {
                Id = HotkeyIds.SelectCurrentScene,
                DisplayName = "Select Current Scene",
                Description = "Выбрать ассет активной сцены в окне Project."
            },
            new Entry
            {
                Id = HotkeyIds.ShowInExplorerSelected,
                DisplayName = "Show In Explorer (Selected)",
                Description = "Открыть системный проводник и показать выделенный ассет."
            },
            new Entry
            {
                Id = HotkeyIds.CreateFolder,
                DisplayName = "Create Folder",
                Description = "Создать новую папку в активной папке Project."
            },
            new Entry
            {
                Id = HotkeyIds.FocusConsole,
                DisplayName = "Focus Console",
                Description = "Открыть и сфокусировать окно Console (просмотр логов)."
            },
            new Entry
            {
                Id = HotkeyIds.RevealPersistentData,
                DisplayName = "Reveal persistentDataPath",
                Description = "Открыть системный проводник на Application.persistentDataPath."
            },
        };

        public static IReadOnlyList<Entry> All => _all;
    }
}