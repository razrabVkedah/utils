using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.IconBrowser
{
    internal class IconTreeView : TreeView
    {
        public enum CopyMode
        {
            Name,
            IconContentSnippet,
            InspectorButtonSnippet
        }

        private readonly IconIndex _index;
        private CopyMode _copyMode;

        public Func<string, string> ResolveName; // injected

        private readonly GUIStyle _tagOn = new(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };

        private readonly GUIStyle _tagOff = new(EditorStyles.miniLabel)
            { normal = { textColor = new Color(1f, 1f, 1f, 0.35f) } };

        private int _iconColumnWidth;

        public IconTreeView(TreeViewState state, MultiColumnHeader header, IconIndex index) : base(state, header)
        {
            _index = index;
            showAlternatingRowBackgrounds = true;
            rowHeight = EditorGUIUtility.singleLineHeight + 6f;
        }

        public static MultiColumnHeaderState.Column[] BuildColumns()
        {
            return new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = EditorGUIUtility.IconContent("FilterByType"),
                    autoResize = false, minWidth = 36, width = 36,
                    headerTextAlignment = TextAlignment.Center, allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Icon Name"),
                    minWidth = 260, width = 320, autoResize = true, allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Variants"),
                    minWidth = 140, width = 160, autoResize = false, allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Copy"),
                    minWidth = 90, width = 100, autoResize = false, allowToggleVisibility = false
                }
            };
        }

        public void SetCopyMode(CopyMode mode) => _copyMode = mode;

        public void ReloadFromIndex()
        {
            Reload();
            Repaint();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var rows = new List<TreeViewItem>(_index.Count);

            foreach (var it in _index.Items)
            {
                rows.Add(new TreeViewItem { id = it.Id, depth = 0, icon = it.Icon, displayName = it.Name });
            }

            root.children = rows;
            return root;
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (string.IsNullOrEmpty(search)) return true;
            return item.displayName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            var col0 = multiColumnHeader.state.columns[0];
            var icon = item?.icon as Texture2D;
            if (!icon) return rowHeight;
            float maxW = Mathf.Max(16f, col0.width - 6f);
            float w = Mathf.Min(maxW, icon.width);
            float h = w * icon.height / Mathf.Max(1, icon.width);
            return Mathf.Max(h + 6f, rowHeight);
        }

        public override void OnGUI(Rect rect)
        {
            var col = multiColumnHeader.state.columns[0];
            int currentWidth = Mathf.RoundToInt(col.width);
            if (_iconColumnWidth != currentWidth)
            {
                _iconColumnWidth = currentWidth;
                RefreshCustomRowHeights();
            }

            base.OnGUI(rect);
        }

        private static Rect CenterRect(Rect rect, float width, float height)
        {
            var r = rect;
            if (width < rect.width)
            {
                var d = rect.width - width;
                r.xMin += d * .5f;
                r.xMax -= d * .5f;
            }

            if (height < rect.height)
            {
                var d = rect.height - height;
                r.yMin += d * .5f;
                r.yMax -= d * .5f;
            }

            return r;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;

            // Col 0: icon (centered)
            var col0 = args.GetCellRect(0);
            var icon = item.icon as Texture2D;
            if (icon)
            {
                var iw = icon.width;
                var ih = icon.height;
                float maxW = Mathf.Max(16f, col0.width - 6f);
                float w = Mathf.Min(maxW, iw);
                float h = w * ih / Mathf.Max(1, iw);
                var r = CenterRect(col0, w, h);
                GUI.DrawTexture(r, icon, ScaleMode.ScaleToFit);
            }

            // Col 1: name
            EditorGUI.LabelField(args.GetCellRect(1), item.displayName);

            // Col 2: variant badges
            var baseName = item.displayName;
            var hasBase = EditorGUIUtility.FindTexture(baseName) != null;
            var hasDark = EditorGUIUtility.FindTexture(baseName.StartsWith("d_") ? baseName : "d_" + baseName) != null;
            var hasOn = EditorGUIUtility.FindTexture(baseName.EndsWith(" On") ? baseName : baseName + " On") != null;
            var hasDarkOn = EditorGUIUtility.FindTexture((baseName.StartsWith("d_") ? baseName : "d_" + baseName) +
                                                         (baseName.EndsWith(" On") ? string.Empty : " On")) != null;

            var rectVar = args.GetCellRect(2);
            var pad = 2f;
            var bw = 46f;
            var bh = EditorGUIUtility.singleLineHeight;
            var r1 = new Rect(rectVar.x + pad, rectVar.y + (rectVar.height - bh) * .5f, bw, bh);
            var r2 = new Rect(r1.xMax + pad, r1.y, bw, bh);
            var r3 = new Rect(r2.xMax + pad, r1.y, bw + 4, bh);
            GUI.Label(r1, "[base]", hasBase ? _tagOn : _tagOff);
            GUI.Label(r2, "[d_]", hasDark ? _tagOn : _tagOff);
            GUI.Label(r3, "[ On ]", (hasOn || hasDarkOn) ? _tagOn : _tagOff);

            // Col 3: Copy button
            if (GUI.Button(args.GetCellRect(3), "Copy"))
            {
                var resolved = ResolveName?.Invoke(baseName) ?? baseName;
                string toCopy = _copyMode switch
                {
                    CopyMode.IconContentSnippet => $"EditorGUIUtility.IconContent(\"{resolved}\")",
                    CopyMode.InspectorButtonSnippet => $"[InspectorButton(label: null, icon: \"{resolved}\")]",
                    _ => resolved
                };
                EditorGUIUtility.systemCopyBuffer = toCopy;
                EditorWindow.focusedWindow?.ShowNotification(new GUIContent($"Copied: {toCopy}"));
            }
        }
    }
}