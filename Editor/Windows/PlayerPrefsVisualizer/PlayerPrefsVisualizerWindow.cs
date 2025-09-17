using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer
{
    public sealed class PlayerPrefsVisualizerWindow : EditorWindow
    {
        private PrefIndex _index;
        private string _search = string.Empty;
        private Vector2 _scroll;
        private int _sortColumn = 0; // 0=Key,1=Type
        private bool _sortAsc = true;
        private GUIStyle _header;

        [MenuItem("Rusleo/PlayerPrefs Visualizer")]
        public static void Open()
        {
            var w = GetWindow<PlayerPrefsVisualizerWindow>(false, "PlayerPrefs");
            w.minSize = new Vector2(560, 360);
            w.Show();
        }

        private void OnEnable() => _index = PrefIndex.Load();

        private void EnsureStyles() => _header ??= new GUIStyle(EditorStyles.boldLabel)
            { alignment = TextAnchor.MiddleLeft };

        private static string Company => PlayerSettings.companyName;
        private static string Product => PlayerSettings.productName;

        private void OnGUI()
        {
            EnsureStyles();
            DrawToolbar();
            DrawContext();
            EditorGUILayout.Space(4);
            DrawHeaderRow();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var rec in Query()) DrawRow(rec);
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    _index = PrefIndex.Load();
                if (GUILayout.Button("Add", EditorStyles.toolbarButton, GUILayout.Width(50))) AddDialog();
                if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(100))) ExportJson();
                if (GUILayout.Button("Import JSON", EditorStyles.toolbarButton, GUILayout.Width(100))) ImportJson();

#if UNITY_EDITOR_WIN
                if (GUILayout.Button("Import: Registry", EditorStyles.toolbarButton, GUILayout.Width(120)))
                {
                    var count = new WinRegistryImporter().TryImportIntoIndex(_index);
                    if (count > 0) ShowNotification(new GUIContent($"Imported {count}"));
                }
#elif UNITY_EDITOR_OSX
                if (GUILayout.Button("Import: Plist", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    var count = new MacPlistImporter().TryImportIntoIndex(_index);
                    if (count > 0) ShowNotification(new GUIContent($"Imported {count}"));
                }
#endif
                GUILayout.FlexibleSpace();
                _search = GUILayout.TextField(_search,
                    GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.Width(240));
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(24))) _search = string.Empty;
            }
        }

        private void DrawContext()
        {
            EditorGUILayout.LabelField("Context", _header);
            EditorGUILayout.LabelField($"Company: {Company}");
            EditorGUILayout.LabelField($"Product: {Product}");

#if UNITY_EDITOR_WIN
            EditorGUILayout.LabelField("Windows Registry:", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel($"HKCU/Software/{Company}/{Product}", EditorStyles.textField,
                GUILayout.Height(18));
#elif UNITY_EDITOR_OSX
            EditorGUILayout.LabelField("macOS .plist:", EditorStyles.miniBoldLabel);
            var path = $"~/Library/Preferences/unity.{Company}.{Product}.plist";
            EditorGUILayout.SelectableLabel(path, EditorStyles.textField, GUILayout.Height(18));
#else
            EditorGUILayout.LabelField("Linux prefs path (varies by distro):", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel($"~/.config/unity3d/{Company}/{Product}/prefs", EditorStyles.textField, GUILayout.Height(18));
#endif

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear ALL PlayerPrefs", GUILayout.Height(20)))
                {
                    if (EditorUtility.DisplayDialog("Clear PlayerPrefs?",
                            "This will delete ALL keys for this Product. Are you sure?", "Yes, delete", "Cancel"))
                    {
                        PlayerPrefs.DeleteAll();
                        _index.items.Clear();
                        _index.Save();
                    }
                }
            }
        }

        private void DrawHeaderRow()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                HeaderButton("Key", 0, 280);
                HeaderButton("Type", 1, 80);
                GUILayout.Label("Value");
                GUILayout.Space(8);
                GUILayout.Label("Actions", GUILayout.Width(180));
            }
        }

        private void HeaderButton(string title, int col, float width)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.Width(width)))
            {
                if (GUILayout.Button(title + (_sortColumn == col ? (_sortAsc ? " ▲" : " ▼") : string.Empty),
                        EditorStyles.miniBoldLabel))
                {
                    if (_sortColumn == col) _sortAsc = !_sortAsc;
                    else
                    {
                        _sortColumn = col;
                        _sortAsc = true;
                    }

                    SortNow();
                }
            }
        }

        private void SortNow()
        {
            IEnumerable<PrefRecord> q = _index.items;
            if (_sortColumn == 0)
                q = _sortAsc
                    ? q.OrderBy(i => i.key, StringComparer.Ordinal)
                    : q.OrderByDescending(i => i.key, StringComparer.Ordinal);
            else
                q = _sortAsc ? q.OrderBy(i => i.type) : q.OrderByDescending(i => i.type);
            _index.items = q.ToList();
        }

        private IEnumerable<PrefRecord> Query()
        {
            var q = _index.items.AsEnumerable();
            if (!string.IsNullOrEmpty(_search))
            {
                var s = _search.Trim();
                q = q.Where(i =>
                    i.key.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    i.raw.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return q;
        }

        private void DrawRow(PrefRecord rec)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel(rec.key, GUILayout.Width(280), GUILayout.Height(18));
                rec.type = (PrefType)EditorGUILayout.EnumPopup(rec.type, GUILayout.Width(80));
                var newRaw = EditorGUILayout.TextField(rec.raw);
                if (!ReferenceEquals(newRaw, rec.raw)) rec.raw = newRaw;

                GUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope(GUILayout.Width(180)))
                {
                    if (GUILayout.Button("Save", GUILayout.Width(50)))
                    {
                        try
                        {
                            rec.WriteToPlayerPrefs();
                            _index.AddOrUpdate(rec);
                            _index.Save();
                            PlayerPrefs.Save();
                        }
                        catch (Exception e)
                        {
                            EditorUtility.DisplayDialog("Invalid value", e.Message, "OK");
                        }
                    }

                    if (GUILayout.Button("Copy", GUILayout.Width(50)))
                        EditorGUIUtility.systemCopyBuffer = rec.raw ?? string.Empty;
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete key?", rec.key, "Delete", "Cancel"))
                        {
                            PlayerPrefs.DeleteKey(rec.key);
                            _index.Remove(rec.key);
                            _index.Save();
                            PlayerPrefs.Save();
                        }
                    }
                }
            }
        }

        private void AddDialog()
        {
            var key = string.Empty;
            var type = PrefType.String;
            var value = string.Empty;
            var ok = PlayerPrefsPromptWindow.ShowModal("Add PlayerPref", ref key, ref type, ref value);
            if (!ok) return;
            if (string.IsNullOrWhiteSpace(key))
            {
                EditorUtility.DisplayDialog("Key required", "Please enter a non‑empty key.", "OK");
                return;
            }

            var rec = new PrefRecord(key.Trim(), type, value ?? "");
            try
            {
                rec.WriteToPlayerPrefs();
                _index.AddOrUpdate(rec);
                _index.Save();
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Invalid value", e.Message, "OK");
            }
        }

        private void ExportJson()
        {
            var path = EditorUtility.SaveFilePanel("Export PlayerPrefs JSON", Application.dataPath,
                $"{Product}_PlayerPrefs.json", "json");
            if (string.IsNullOrEmpty(path)) return;
            var list = new List<PrefRecord>();
            foreach (var rec in _index.items) list.Add(PrefRecord.FromPlayerPrefs(rec.key, rec.type));
            var json = JsonUtility.ToJson(new ExportWrapper { items = list }, true);
            System.IO.File.WriteAllText(path, json, Encoding.UTF8);
            EditorUtility.RevealInFinder(path);
        }

        private void ImportJson()
        {
            var path = EditorUtility.OpenFilePanel("Import PlayerPrefs JSON", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                var json = System.IO.File.ReadAllText(path, Encoding.UTF8);
                var wrapper = JsonUtility.FromJson<ExportWrapper>(json);
                if (wrapper?.items == null) throw new Exception("Invalid JSON");
                if (!EditorUtility.DisplayDialog("Import PlayerPrefs",
                        $"Apply {wrapper.items.Count} keys? Existing keys will be overwritten.", "Import",
                        "Cancel")) return;
                foreach (var rec in wrapper.items)
                {
                    try
                    {
                        rec.WriteToPlayerPrefs();
                        _index.AddOrUpdate(rec);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to import {rec.key}: {ex.Message}");
                    }
                }

                _index.Save();
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Import failed", e.Message, "OK");
            }
        }
    }
}