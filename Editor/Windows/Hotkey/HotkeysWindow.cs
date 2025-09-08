using System.Linq;
using Rusleo.Utils.Editor.Hotkeys.Core;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.Hotkey
{
    /// <summary>
    /// Окно управления хоткеями пакета Rusleo: просмотр, поиск, редактирование и сброс к дефолтам пакета.
    /// </summary>
    public sealed class HotkeysWindow : EditorWindow
    {
        // UI state
        private string _search = "";
        private Vector2 _scroll;

        // Режим редактирования конкретной строки
        private string _editingShortcutId;
        private KeyCode _editKey = KeyCode.None;
        private bool _editAction, _editShift, _editAlt;

        [MenuItem("Rusleo/Hotkeys/Window")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<HotkeysWindow>("Rusleo Hotkeys");
            wnd.minSize = new Vector2(540, 280);
            wnd.Show();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawToolbar();
                DrawHeader();
                DrawRows();
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Toolbar (поиск, переход к системным настройкам, массовый сброс)
        // ──────────────────────────────────────────────────────────────────────────
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Поиск
                _search = GUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.MinWidth(160));
                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(24)))
                    _search = "";

                GUILayout.FlexibleSpace();

                // Открыть стандартные настройки шорткатов Unity
                if (GUILayout.Button("Open Unity Shortcuts…", EditorStyles.toolbarButton))
                {
                    EditorApplication.ExecuteMenuItem("Edit/Shortcuts...");
                }

                // Массовый ресет к дефолтам пакета
                if (GUILayout.Button("Reset All (Rusleo)", EditorStyles.toolbarButton))
                {
                    foreach (var e in HotkeysCatalog.All)
                    {
                        if (HotkeysDefaults.TryGet(e.Id, out var def))
                            ShortcutManager.instance.RebindShortcut(e.Id, def);
                        else
                            ShortcutManager.instance.RebindShortcut(e.Id, ShortcutBinding.empty);
                    }

                    // Выходим из режима редактирования, если он активен
                    _editingShortcutId = null;
                    Repaint();
                }
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Заголовок таблицы
        // ──────────────────────────────────────────────────────────────────────────
        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Action", GUILayout.Width(200));
                GUILayout.Label("Binding", GUILayout.Width(190));
                GUILayout.Label("Description");
            }
        }

        // ──────────────────────────────────────────────────────────────────────────
        // Строки таблицы
        // ──────────────────────────────────────────────────────────────────────────
        private void DrawRows()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            var query = _search?.Trim().ToLowerInvariant() ?? string.Empty;

            foreach (var e in HotkeysCatalog.All.Where(x =>
                         string.IsNullOrEmpty(query) ||
                         x.DisplayName.ToLowerInvariant().Contains(query) ||
                         x.Description.ToLowerInvariant().Contains(query)))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // ── Col 1: название действия
                    GUILayout.Label(e.DisplayName, GUILayout.Width(200));

                    // ── Col 2: текущая привязка + кнопки Edit/Reset/Clear
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(190)))
                    {
                        var current = ShortcutManager.instance.GetShortcutBinding(e.Id);
                        EditorGUILayout.LabelField(ShortcutBindingUtils.ToHumanString(current), EditorStyles.miniLabel);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Edit", GUILayout.Width(54)))
                            {
                                _editingShortcutId = e.Id;

                                // Безопасно берём первый KeyCombination (коллекция — IEnumerable)
                                var kc = current.keyCombinationSequence != null
                                    ? current.keyCombinationSequence.FirstOrDefault()
                                    : new KeyCombination();

                                _editKey = kc.keyCode;
                                _editAction = kc.action;
                                _editShift = kc.shift;
                                _editAlt = kc.alt;
                            }

                            if (GUILayout.Button("Reset", GUILayout.Width(60)))
                            {
                                if (HotkeysDefaults.TryGet(e.Id, out var def))
                                    ShortcutManager.instance.RebindShortcut(e.Id, def);
                                else
                                    ShortcutManager.instance.RebindShortcut(e.Id, ShortcutBinding.empty);

                                if (_editingShortcutId == e.Id) _editingShortcutId = null;
                            }

                            if (GUILayout.Button("Clear", GUILayout.Width(58)))
                            {
                                ShortcutManager.instance.RebindShortcut(e.Id, ShortcutBinding.empty);
                                if (_editingShortcutId == e.Id) _editingShortcutId = null;
                            }
                        }
                    }

                    // ── Col 3: описание + редактор биндинга (инлайн)
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField(e.Description, EditorStyles.wordWrappedLabel);

                        if (_editingShortcutId == e.Id)
                        {
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.LabelField("Edit binding", EditorStyles.boldLabel);

                                _editKey = (KeyCode)EditorGUILayout.EnumPopup("Key", _editKey);

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    _editAction = EditorGUILayout.ToggleLeft(
                                        Application.platform == RuntimePlatform.OSXEditor ? "Cmd" : "Ctrl",
                                        _editAction, GUILayout.Width(64));
                                    _editShift = EditorGUILayout.ToggleLeft("Shift", _editShift, GUILayout.Width(64));
                                    _editAlt = EditorGUILayout.ToggleLeft("Alt", _editAlt, GUILayout.Width(64));
                                }

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    if (GUILayout.Button("Apply", GUILayout.Width(70)))
                                    {
                                        var binding = ShortcutBindingUtils.BuildBinding(_editKey, _editAction,
                                            _editShift, _editAlt);
                                        ShortcutManager.instance.RebindShortcut(e.Id, binding);
                                        _editingShortcutId = null;
                                    }

                                    if (GUILayout.Button("Cancel", GUILayout.Width(70)))
                                    {
                                        _editingShortcutId = null;
                                    }

                                    GUILayout.FlexibleSpace();
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.Space(2);
            }

            EditorGUILayout.EndScrollView();
        }
    }
}