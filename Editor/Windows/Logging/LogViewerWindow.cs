using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Rusleo.Utils.Runtime.Logging;

namespace Rusleo.Utils.Editor.Windows.Logging
{
    public sealed class LogViewerWindow : EditorWindow
    {
        private struct Row
        {
            public LogLevel level;
            public string owner;
            public string message;
            public string[] tags;
            public string meta;
            public string time;
        }

        private const int CAPACITY = 2000;
        private readonly List<Row> _rows = new(CAPACITY);
        private Vector2 _scroll;
        private bool _paused;
        private LogLevel _minLevel = LogLevel.Debug;
        private string _filterText = "";
        private string _filterOwner = "";
        private string _filterTag = "";

        [MenuItem("Rusleo/Log Viewer")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<LogViewerWindow>("Rusleo Log Viewer");
            wnd.Show();
        }

        private void OnEnable()
        {
            LogDispatcher.Instance.Emitted += OnLog;
            EditorApplication.playModeStateChanged += _ => Repaint();
        }

        private void OnDisable()
        {
            if (LogDispatcher.Instance != null)
                LogDispatcher.Instance.Emitted -= OnLog;
        }

        private void OnLog(in LogEvent e)
        {
            if (_paused) return;

            var meta = (e.Metadata != null && e.Metadata.Count > 0)
                ? string.Join(", ", e.Metadata.Select(kv => $"{kv.Key}={kv.Value}"))
                : "";

            var row = new Row
            {
                level = e.Level,
                owner = e.Owner ?? "",
                message = e.Message ?? "",
                tags = e.Tags ?? System.Array.Empty<string>(),
                meta = meta,
                time = e.UtcTimestamp.ToLocalTime().ToString("HH:mm:ss.fff")
            };

            _rows.Add(row);
            if (_rows.Count > CAPACITY) _rows.RemoveRange(0, _rows.Count - CAPACITY);
            Repaint();
        }

        private bool PassesFilters(in Row r)
        {
            if (r.level < _minLevel) return false;
            if (!string.IsNullOrEmpty(_filterOwner) && !r.owner.ToLower().Contains(_filterOwner.ToLower()))
                return false;
            if (!string.IsNullOrEmpty(_filterTag) &&
                (r.tags == null || !r.tags.Any(t => t.ToLower().Contains(_filterTag.ToLower())))) return false;
            if (!string.IsNullOrEmpty(_filterText) &&
                !($"{r.message} {r.meta}".ToLower().Contains(_filterText.ToLower()))) return false;
            return true;
        }

        private static Color LevelColor(LogLevel lvl) => lvl switch
        {
            LogLevel.Trace => new Color(0.7f, 0.7f, 0.7f),
            LogLevel.Debug => new Color(0.6f, 0.8f, 1f),
            LogLevel.Info => Color.white,
            LogLevel.Warn => new Color(1f, 0.87f, 0.4f),
            LogLevel.Error => new Color(1f, 0.55f, 0.55f),
            LogLevel.Fatal => new Color(1f, 0.2f, 0.2f),
            _ => Color.white
        };

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                // Toolbar
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    _paused = GUILayout.Toggle(_paused, _paused ? "Resume" : "Pause", EditorStyles.toolbarButton,
                        GUILayout.Width(70));
                    if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
                        _rows.Clear();

                    GUILayout.Space(10);
                    _minLevel = (LogLevel)EditorGUILayout.EnumPopup(_minLevel, GUILayout.Width(90));

                    GUILayout.Space(10);
                    _filterOwner =
                        GUILayout.TextField(_filterOwner, EditorStyles.toolbarTextField, GUILayout.Width(140));
                    GUILayout.Label("owner", GUILayout.Width(40));

                    _filterTag = GUILayout.TextField(_filterTag, EditorStyles.toolbarTextField, GUILayout.Width(140));
                    GUILayout.Label("tag", GUILayout.Width(28));

                    GUILayout.FlexibleSpace();

                    _filterText = GUILayout.TextField(_filterText, EditorStyles.toolbarTextField,
                        GUILayout.MinWidth(160));
                    if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(24))) _filterText = "";
                }

                // Table header
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Label("Time", GUILayout.Width(80));
                    GUILayout.Label("Level", GUILayout.Width(56));
                    GUILayout.Label("Owner", GUILayout.Width(160));
                    GUILayout.Label("Message");
                }

                // Rows
                _scroll = EditorGUILayout.BeginScrollView(_scroll);
                foreach (var r in _rows)
                {
                    if (!PassesFilters(r)) continue;

                    var c = GUI.color;
                    GUI.color = LevelColor(r.level);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(r.time, GUILayout.Width(80));
                        GUILayout.Label(r.level.ToString(), GUILayout.Width(56));
                        GUILayout.Label(string.IsNullOrEmpty(r.owner) ? "-" : r.owner, GUILayout.Width(160));

                        var msg = r.message;
                        if (!string.IsNullOrEmpty(r.meta)) msg += $"   | {r.meta}";
                        if (r.tags != null && r.tags.Length > 0) msg += $"   [#{string.Join(" #", r.tags)}]";

                        EditorGUILayout.SelectableLabel(msg, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                    }

                    GUI.color = c;
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }
}