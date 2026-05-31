using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.TimeTracking
{
    public sealed class TimeTrackingAnalyticsWindow : EditorWindow
    {
        private static readonly Color ActiveColor   = new(0.25f, 0.72f, 0.25f);
        private static readonly Color AfkColor      = new(0.75f, 0.62f, 0.12f);
        private static readonly Color PlayModeColor = new(0.30f, 0.50f, 0.90f);
        private static readonly Color BarBgColor    = new(0.18f, 0.18f, 0.18f);

        // Lazy-init styles — only accessed during OnGUI when EditorStyles are ready.
        private static GUIStyle s_bigValue;
        private static GUIStyle s_centerMini;

        private static GUIStyle BigValue   => s_bigValue   ??= new GUIStyle(EditorStyles.boldLabel) { fontSize = 13, alignment = TextAnchor.MiddleCenter };
        private static GUIStyle CenterMini => s_centerMini ??= new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };

        private AnalyticsReport _report;
        private Vector2 _chartScroll;
        private Vector2 _tableScroll;
        private string _statusText = "Not loaded";
        private int _tab;

        [MenuItem("Rusleo/Time Tracking Analytics")]
        public static void ShowWindow()
        {
            var w = GetWindow<TimeTrackingAnalyticsWindow>("Time Tracking");
            w.minSize = new Vector2(560, 400);
            w.Show();
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_report == null)
            {
                GUILayout.Space(16);
                EditorGUILayout.HelpBox("Press 'Load' to parse analytics logs from the current project.", MessageType.Info);
                return;
            }

            GUILayout.Space(4);
            DrawSummary();
            GUILayout.Space(6);

            _tab = GUILayout.Toolbar(_tab, new[] { "Chart", "Table" }, GUILayout.Height(22));
            GUILayout.Space(4);

            if (_tab == 0) DrawChart();
            else DrawTable();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(55)))
                    Load();

                GUILayout.Label(_statusText, EditorStyles.label);
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawSummary()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawStat("Active Time",  FormatTime(_report.TotalActiveSec),  ActiveColor);
                DrawStat("Total Time",   FormatTime(_report.TotalSec),         Color.white);
                DrawStat("AFK",          FormatTime(_report.TotalAfkSec),      AfkColor);
                DrawStat("Play Mode",    FormatTime(_report.TotalPlayModeSec), PlayModeColor);
                DrawStat("Sessions",     _report.SessionCount.ToString(),       Color.white);
                DrawStat("Days worked",  _report.Days.Count.ToString(),        Color.white);
            }
        }

        private static void DrawStat(string label, string value, Color valueColor)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandWidth(true)))
            {
                var prev = GUI.color;
                GUI.color = valueColor;
                GUILayout.Label(value, BigValue, GUILayout.ExpandWidth(true));
                GUI.color = prev;
                GUILayout.Label(label, CenterMini, GUILayout.ExpandWidth(true));
            }
        }

        private void DrawChart()
        {
            if (_report.Days.Count == 0) return;

            var maxSec = _report.Days.Max(d => d.TotalSec);
            if (maxSec == 0) maxSec = 1;

            const float rowH   = 22f;
            const float dateW  = 90f;
            const float labelW = 58f;
            const float pad    = 4f;

            _chartScroll = EditorGUILayout.BeginScrollView(_chartScroll, GUILayout.ExpandHeight(true));

            foreach (var day in _report.Days.AsEnumerable().Reverse())
            {
                var row    = EditorGUILayout.GetControlRect(false, rowH);
                var dateR  = new Rect(row.x,               row.y + 1, dateW,                                row.height - 2);
                var barR   = new Rect(row.x + dateW + pad, row.y + 2, row.width - dateW - labelW - pad * 2, row.height - 4);
                var lblR   = new Rect(row.xMax - labelW,   row.y + 1, labelW,                               row.height - 2);

                EditorGUI.DrawRect(barR, BarBgColor);
                DrawBar(barR, day.TotalSec,   maxSec, AfkColor);
                DrawBar(barR, day.ActiveSec,  maxSec, ActiveColor);

                if (day.PlayModeSec > 0)
                {
                    var pmW = barR.width * day.PlayModeSec / maxSec;
                    EditorGUI.DrawRect(new Rect(barR.x, barR.yMax - 3, pmW, 3), PlayModeColor);
                }

                var isToday = day.Date == DateTime.Today;
                var dateStr = isToday
                    ? $"► {day.Date:ddd dd.MM}"
                    : $"   {day.Date:ddd dd.MM}";

                GUI.Label(dateR, dateStr, isToday ? EditorStyles.boldLabel : EditorStyles.label);
                GUI.Label(lblR, FormatTime(day.ActiveSec), EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
            DrawLegend();
        }

        private static void DrawBar(Rect area, int valueSec, int maxSec, Color color)
        {
            if (valueSec <= 0) return;
            var w = area.width * (float)valueSec / maxSec;
            EditorGUI.DrawRect(new Rect(area.x, area.y, w, area.height), color);
        }

        private static void DrawLegend()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                LegendItem("Active",             ActiveColor);
                LegendItem("AFK",                AfkColor);
                LegendItem("Play Mode (stripe)", PlayModeColor);
                GUILayout.FlexibleSpace();
            }
        }

        private static void LegendItem(string label, Color color)
        {
            var r = EditorGUILayout.GetControlRect(false, 14f, GUILayout.Width(14));
            EditorGUI.DrawRect(new Rect(r.x, r.y + 2, 12, 10), color);
            GUILayout.Label(label, EditorStyles.label, GUILayout.Width(120));
        }

        private void DrawTable()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                TCol("Date",      100);
                TCol("Active",     68);
                TCol("Total",      68);
                TCol("AFK",        68);
                TCol("Play Mode",  68);
                GUILayout.FlexibleSpace();
            }

            _tableScroll = EditorGUILayout.BeginScrollView(_tableScroll);

            foreach (var day in _report.Days.AsEnumerable().Reverse())
            {
                var isToday = day.Date == DateTime.Today;
                using (new EditorGUILayout.HorizontalScope())
                {
                    var style = isToday ? EditorStyles.boldLabel : EditorStyles.label;
                    GUILayout.Label(day.Date.ToString("ddd dd.MM.yy"), style,             GUILayout.Width(100));
                    GUILayout.Label(FormatTime(day.ActiveSec),          EditorStyles.label, GUILayout.Width(68));
                    GUILayout.Label(FormatTime(day.TotalSec),            EditorStyles.label, GUILayout.Width(68));
                    GUILayout.Label(FormatTime(day.AfkSec),              EditorStyles.label, GUILayout.Width(68));
                    GUILayout.Label(FormatTime(day.PlayModeSec),         EditorStyles.label, GUILayout.Width(68));
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private static void TCol(string text, int width) =>
            GUILayout.Label(text, EditorStyles.label, GUILayout.Width(width));

        private void Load()
        {
            var dir = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", "ProjectSettings/RusleoTimeTracking/sessions"));

            _report = AnalyticsLogParser.Parse(dir);

            _statusText = _report.FileCount == 0
                ? "No session files found"
                : $"{_report.FileCount} files · {_report.Days.Count} days · {_report.SessionCount} sessions · {_report.EventCount:N0} events";

            Repaint();
        }

        private static string FormatTime(int seconds)
        {
            if (seconds <= 0) return "—";
            var h = seconds / 3600;
            var m = seconds % 3600 / 60;
            return h > 0 ? $"{h}h {m:00}m" : $"{m}m";
        }
    }
}
