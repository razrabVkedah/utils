using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.IconBrowser
{
    public class EditorIconBrowserWindow : EditorWindow
    {
        [SerializeField] private TreeViewState _treeState;
        [SerializeField] private MultiColumnHeaderState _headerState;
        [SerializeField] private bool _aggressiveLegacyScan = true;


        private SearchField _searchField;
        private IconTreeView _treeView;
        private MultiColumnHeader _header;
        private IconIndex _index;

        // Toolbar prefs
        [SerializeField] private bool _preferDark = true;
        [SerializeField] private bool _preferOn = false;
        [SerializeField] private IconTreeView.CopyMode _copyMode = IconTreeView.CopyMode.Name;
        [SerializeField] private bool _useLegacySeeds = true; // ← добавляет «много» имён
        [SerializeField] private bool _expandMorphology = true; // ← расширяет варианты (" Icon", etc.)

        private double _lastRefresh;
        private const float RefreshEverySeconds = 1.0f;
        private string _lastSettingsKey;

        [MenuItem("Rusleo/Editor Icon Browser")]
        private static void OpenWindow()
        {
            var window = GetWindow<EditorIconBrowserWindow>();
            window.minSize = new Vector2(600f, 360f);
            window.titleContent = new GUIContent(EditorGUIUtility.IconContent("FilterByType"))
                { text = "Editor Icon Browser" };
            window.Show();
        }

        private void OnEnable()
        {
            _index = new IconIndex();
            _searchField ??= new SearchField();
            EnsureHeader();
            _treeState ??= new TreeViewState();

            _treeView = new IconTreeView(_treeState, _header, _index)
            {
                ResolveName = baseName => IconVariantResolver.ResolveSafeName(baseName, _preferDark, _preferOn)
            };

            _treeView.SetCopyMode(_copyMode);

            // initial build
            _index.Rebuild(_useLegacySeeds, _expandMorphology, _aggressiveLegacyScan);
            _treeView.ReloadFromIndex();
            _lastSettingsKey = MakeKey();
        }

        private string MakeKey() => string.Concat(_useLegacySeeds ? '1' : '0', '|', _expandMorphology ? '1' : '0', '|',
            _preferDark ? '1' : '0', '|', _preferOn ? '1' : '0');

        private void EnsureHeader()
        {
            var cols = IconTreeView.BuildColumns();
            var state = new MultiColumnHeaderState(cols);
            if (_header == null)
                _header = new MultiColumnHeader(state);

            if (MultiColumnHeaderState.CanOverwriteSerializedFields(_headerState, state))
                MultiColumnHeaderState.OverwriteSerializedFields(_headerState, state);

            _headerState = state;
        }

        private void OnGUI()
        {
            if (_treeView == null || _index == null) 
            {
                // Мягкая инициализация на случай сброса домена/серилизации
                try
                {
                    if (_index == null) _index = new IconIndex();
                    if (_searchField == null) _searchField = new SearchField();
                    EnsureHeader();
                    _treeState ??= new TreeViewState();

                    _treeView ??= new IconTreeView(_treeState, _header, _index)
                    {
                        ResolveName = baseName => IconVariantResolver.ResolveSafeName(baseName, _preferDark, _preferOn)
                    };
                    _treeView.SetCopyMode(_copyMode);
                    _index.Rebuild(_useLegacySeeds, _expandMorphology, _aggressiveLegacyScan);
                    _treeView.ReloadFromIndex();
                    _lastSettingsKey = MakeKey();
                }
                catch
                {
                    /* дождёмся следующего GUI цикла */
                }

                return;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                _treeView.searchString = _searchField.OnGUI(_treeView.searchString);
                GUILayout.FlexibleSpace();

                var newMode = (IconTreeView.CopyMode)EditorGUILayout.EnumPopup(_copyMode, EditorStyles.toolbarPopup,
                    GUILayout.Width(210));
                if (newMode != _copyMode)
                {
                    _copyMode = newMode;
                    _treeView.SetCopyMode(newMode);
                }

                _preferDark = GUILayout.Toggle(_preferDark, "Prefer d_", EditorStyles.toolbarButton,
                    GUILayout.Width(90));
                _preferOn = GUILayout.Toggle(_preferOn, "Prefer \" On\"", EditorStyles.toolbarButton,
                    GUILayout.Width(110));
                _useLegacySeeds = GUILayout.Toggle(_useLegacySeeds, "Legacy seeds", EditorStyles.toolbarButton,
                    GUILayout.Width(110));
                _expandMorphology = GUILayout.Toggle(_expandMorphology, "Morphology", EditorStyles.toolbarButton,
                    GUILayout.Width(95));
                _aggressiveLegacyScan = GUILayout.Toggle(_aggressiveLegacyScan, "Aggressive",
                    EditorStyles.toolbarButton, GUILayout.Width(90));


                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.toolbarButton,
                        GUILayout.Width(28)))
                {
                    _index.Rebuild(_useLegacySeeds, _expandMorphology, _aggressiveLegacyScan);
                    _treeView.ReloadFromIndex();
                }

                GUILayout.Label($"Loaded {_index.Count} icons", EditorStyles.miniLabel);
            }

            // Rebuild if settings changed (force immediate apply)
            var key = MakeKey();
            if (key != _lastSettingsKey)
            {
                _lastSettingsKey = key;
                _index.Rebuild(_useLegacySeeds, _expandMorphology, _aggressiveLegacyScan);
                _treeView.ReloadFromIndex();
                Repaint();
            }

            RefreshIndexPeriodically();

            var listRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _treeView.OnGUI(listRect);
        }

        private void RefreshIndexPeriodically()
        {
            if (EditorApplication.timeSinceStartup - _lastRefresh < RefreshEverySeconds) return;
            _lastRefresh = EditorApplication.timeSinceStartup;
            if (_index.RefreshIfChanged(_useLegacySeeds, _expandMorphology, _aggressiveLegacyScan))
            {
                _treeView.ReloadFromIndex();
                Repaint();
            }
        }
    }
}