using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.GradientStudio
{
    public class GradientStudioWindow : EditorWindow
    {
        private const string DefaultSaveFolder = "Assets/Rusleo.Utils/GradientStudio/Presets";
        private const int PreviewTexWidthSmall = 256;
        private const int PreviewTexWidthLarge = 1024;

        [MenuItem("Rusleo/Gradient Studio")] 
        public static void Open()
        {
            var wnd = GetWindow<GradientStudioWindow>("Gradient Studio");
            wnd.minSize = new Vector2(760, 420);
            wnd.Show();
        }

        // State
        private Gradient _gradient = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f, 1), 0f),
                new GradientColorKey(new Color(0.95f, 0.45f, 0.80f, 1), 1f),
            },
            alphaKeys = new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 1f) },
            mode = GradientMode.Blend
        };

        private string _json = "";
        private Vector2 _scrollLeft, _scrollRight;
        private Texture2D _previewTex;
        private List<GradientPresetSO> _presets = new();

        private GUIStyle _monoMini;

        private GUIStyle MonoMini => _monoMini ??= new GUIStyle(EditorStyles.textArea)
        {
            font = Font.CreateDynamicFontFromOSFont("Consolas", 11),
            fontSize = 11
        };

        private void OnEnable()
        {
            RefreshPresetsList();
            RegeneratePreview(PreviewTexWidthSmall);
        }

        private void OnDisable()
        {
            if (_previewTex != null)
            {
                DestroyImmediate(_previewTex);
                _previewTex = null;
            }
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawLeftPanel();
                DrawRightPanel();
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                if (GUILayout.Button("Bake 256×1 PNG")) BakeTexture(PreviewTexWidthSmall);
                if (GUILayout.Button("Bake 1024×1 PNG")) BakeTexture(PreviewTexWidthLarge);
                GUILayout.FlexibleSpace();
                _gradient.mode =
                    (GradientMode)EditorGUILayout.EnumPopup("Mode", _gradient.mode, GUILayout.MaxWidth(280));
            }
        }

        // --------- LEFT PANEL: Градиент + Таблица ключей + Пресеты ----------
        private void DrawLeftPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.52f)))
            {
                EditorGUILayout.LabelField("Gradient", EditorStyles.boldLabel);
                _gradient = EditorGUILayout.GradientField(_gradient, GUILayout.Height(22));

                // Большой превью-бар
                DrawGradientPreviewBar();

                // Таблица ключей (как ты просил: HEX, Alpha 0–255, Position 0–100)
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Ключи (таблица):", EditorStyles.boldLabel);
                _scrollLeft =
                    EditorGUILayout.BeginScrollView(_scrollLeft, GUILayout.MinHeight(120), GUILayout.MaxHeight(220));
                DrawKeysTable();
                EditorGUILayout.EndScrollView();

                // Пресеты
                EditorGUILayout.Space(6);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Пресеты", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Обновить", GUILayout.Width(90))) RefreshPresetsList();
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Сохранить как пресет…", GUILayout.Height(22)))
                            SaveAsPreset();
                        if (GUILayout.Button("Открыть папку", GUILayout.Width(120)))
                            EditorUtility.RevealInFinder(Path.GetFullPath(DefaultSaveFolder));
                    }

                    EditorGUILayout.Space(2);
                    if (_presets.Count == 0)
                    {
                        EditorGUILayout.HelpBox("Нет сохранённых пресетов. Нажми «Сохранить как пресет…».",
                            MessageType.Info);
                    }
                    else
                    {
                        foreach (var p in _presets.Where(p => p != null))
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(p.name, GUILayout.Height(20)))
                                {
                                    _gradient = p.gradient;
                                    RegeneratePreview(PreviewTexWidthSmall);
                                }

                                if (GUILayout.Button("В JSON", GUILayout.Width(70)))
                                {
                                    _json = ToJson(_gradient, true);
                                    GUI.FocusControl(null);
                                }

                                if (GUILayout.Button("Переименовать", GUILayout.Width(100)))
                                {
                                    var path = AssetDatabase.GetAssetPath(p);
                                    var newName = EditorUtility.SaveFilePanel("Rename Preset",
                                        Path.GetDirectoryName(path), p.name, "asset");
                                    if (!string.IsNullOrEmpty(newName))
                                    {
                                        var projRel = ToProjectPath(newName);
                                        AssetDatabase.RenameAsset(path, Path.GetFileNameWithoutExtension(projRel));
                                        AssetDatabase.SaveAssets();
                                        RefreshPresetsList();
                                    }
                                }

                                if (GUILayout.Button("×", GUILayout.Width(24)))
                                {
                                    var path = AssetDatabase.GetAssetPath(p);
                                    AssetDatabase.DeleteAsset(path);
                                    AssetDatabase.SaveAssets();
                                    RefreshPresetsList();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawGradientPreviewBar()
        {
            var rect = GUILayoutUtility.GetRect(10, 32, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect,
                EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.9f, 0.9f, 0.9f));
            if (_previewTex != null) GUI.DrawTexture(rect, _previewTex, ScaleMode.StretchToFill);
            if (Event.current.type == EventType.Repaint) RegeneratePreview(PreviewTexWidthSmall);
        }

        private void DrawKeysTable()
        {
            var ck = _gradient.colorKeys.OrderBy(k => k.time).ToArray();
            var ak = _gradient.alphaKeys.OrderBy(k => k.time).ToArray();

            // «Сшиваем» по позициям (для таблицы выводим «составной» HEX + альфа в 0–255)
            var positions = ck.Select(k => k.time)
                .Concat(ak.Select(a => a.time))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            // Заголовок
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("HEX", GUILayout.Width(90));
                EditorGUILayout.LabelField("Alpha (0–255)", GUILayout.Width(110));
                EditorGUILayout.LabelField("Позиция (0–100)", GUILayout.Width(120));
                EditorGUILayout.LabelField("Описание/назначение", GUILayout.ExpandWidth(true));
            }

            foreach (var t in positions)
            {
                var col = _gradient.Evaluate(t);
                var hex = GradientJson.ColorToHex(col);
                var alpha255 = Mathf.RoundToInt(Mathf.Clamp01(col.a) * 255f);
                var pos100 = Mathf.RoundToInt(t * 100f);

                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.SelectableLabel(hex, GUILayout.Width(90), GUILayout.Height(18));
                    EditorGUILayout.SelectableLabel(alpha255.ToString(), GUILayout.Width(110), GUILayout.Height(18));
                    EditorGUILayout.SelectableLabel(pos100.ToString(), GUILayout.Width(120), GUILayout.Height(18));
                    EditorGUILayout.TextField("", GUILayout.ExpandWidth(true)); // свободная заметка под назначение
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Добавить цветовой ключ"))
                {
                    var list = _gradient.colorKeys.ToList();
                    var t = 0.5f;
                    list.Add(new GradientColorKey(_gradient.Evaluate(t), t));
                    _gradient.colorKeys = list.OrderBy(k => k.time).ToArray();
                    RegeneratePreview(PreviewTexWidthSmall);
                }

                if (GUILayout.Button("Добавить альфа-ключ"))
                {
                    var list = _gradient.alphaKeys.ToList();
                    var t = 0.5f;
                    list.Add(new GradientAlphaKey(_gradient.Evaluate(t).a, t));
                    _gradient.alphaKeys = list.OrderBy(k => k.time).ToArray();
                    RegeneratePreview(PreviewTexWidthSmall);
                }

                if (GUILayout.Button("Нормализовать (0..1)"))
                {
                    // Гарантируем ключи в [0..1] и сортировку
                    _gradient.colorKeys = _gradient.colorKeys
                        .Select(k => new GradientColorKey(k.color, Mathf.Clamp01(k.time)))
                        .OrderBy(k => k.time).ToArray();
                    _gradient.alphaKeys = _gradient.alphaKeys
                        .Select(k => new GradientAlphaKey(Mathf.Clamp01(k.alpha), Mathf.Clamp01(k.time)))
                        .OrderBy(k => k.time).ToArray();
                    RegeneratePreview(PreviewTexWidthSmall);
                }
            }
        }

        // --------- RIGHT PANEL: JSON I/O ----------
        private void DrawRightPanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.LabelField("JSON", EditorStyles.boldLabel);

                _scrollRight = EditorGUILayout.BeginScrollView(_scrollRight);
                _json = EditorGUILayout.TextArea(_json, MonoMini, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Export JSON"))
                    {
                        _json = ToJson(_gradient, pretty: true);
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("Minify"))
                    {
                        _json = ToJson(_gradient, pretty: false);
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("Copy"))
                    {
                        EditorGUIUtility.systemCopyBuffer = _json;
                        ShowNotification(new GUIContent("JSON скопирован"));
                    }

                    if (GUILayout.Button("Paste"))
                    {
                        _json = EditorGUIUtility.systemCopyBuffer;
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("Import JSON"))
                    {
                        try
                        {
                            var g = JsonUtility.FromJson<GradientJson>(_json)?.ToGradient();
                            if (g != null)
                            {
                                _gradient = g;
                                RegeneratePreview(PreviewTexWidthSmall);
                                ShowNotification(new GUIContent("Импортировано"));
                            }
                            else
                            {
                                ShowNotification(new GUIContent("Пустой JSON"));
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[Gradient Studio] Import error: {e.Message}\n{e}");
                            ShowNotification(new GUIContent("Ошибка импорта (см. Console)"));
                        }
                    }
                }
            }
        }

        // --------- Helpers ----------
        private static string ToJson(Gradient gradient, bool pretty)
        {
            var payload = GradientJson.FromGradient(gradient);
            return JsonUtility.ToJson(payload, pretty);
        }

        private void RegeneratePreview(int width)
        {
            if (width <= 0) width = PreviewTexWidthSmall;
            var tex = _previewTex;
            if (tex == null || tex.width != width)
            {
                if (tex != null) DestroyImmediate(tex);
                tex = new Texture2D(width, 1, TextureFormat.RGBA32, false, true);
                tex.wrapMode = TextureWrapMode.Clamp;
                _previewTex = tex;
            }

            for (int x = 0; x < width; x++)
            {
                float t = (float)x / (width - 1);
                tex.SetPixel(x, 0, _gradient.Evaluate(t));
            }

            tex.Apply(false, false);
            Repaint();
        }

        private void BakeTexture(int width)
        {
            RegeneratePreview(width);
            var path = EditorUtility.SaveFilePanel("Save Gradient Texture", "Assets", "Gradient", "png");
            if (string.IsNullOrEmpty(path)) return;

            var temp = new Texture2D(width, 1, TextureFormat.RGBA32, false, true);
            for (int x = 0; x < width; x++)
            {
                float t = (float)x / (width - 1);
                temp.SetPixel(x, 0, _gradient.Evaluate(t));
            }

            temp.Apply();

            var png = temp.EncodeToPNG();
            DestroyImmediate(temp);

            File.WriteAllBytes(path, png);
            var projRel = ToProjectPath(path);
            AssetDatabase.ImportAsset(projRel, ImportAssetOptions.ForceUpdate);

            var ti = (TextureImporter)AssetImporter.GetAtPath(projRel);
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Default;
                ti.mipmapEnabled = false;
                ti.isReadable = false;
                ti.npotScale = TextureImporterNPOTScale.None;
                ti.filterMode = FilterMode.Bilinear;
                ti.wrapMode = TextureWrapMode.Clamp;
                AssetDatabase.ImportAsset(projRel, ImportAssetOptions.ForceUpdate);
            }

            ShowNotification(new GUIContent($"PNG сохранён:\n{projRel}"));
        }

        private static string EnsureFolder(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                var parts = folder.Split('/');
                var current = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    var next = $"{current}/{parts[i]}";
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            return folder;
        }

        private void SaveAsPreset()
        {
            EnsureFolder(DefaultSaveFolder);
            var file = EditorUtility.SaveFilePanelInProject("Save Gradient Preset", "NewGradientPreset", "asset",
                "Выбери имя и место сохранения", DefaultSaveFolder);
            if (string.IsNullOrEmpty(file)) return;

            var so = CreateInstance<GradientPresetSO>();
            so.gradient = _gradient;
            AssetDatabase.CreateAsset(so, file);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshPresetsList();
            ShowNotification(new GUIContent("Пресет сохранён"));
        }

        private void RefreshPresetsList()
        {
            _presets.Clear();
            var guids = AssetDatabase.FindAssets("t:GradientPresetSO");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<GradientPresetSO>(path);
                if (asset != null) _presets.Add(asset);
            }
        }

        private static string ToProjectPath(string absolutePath)
        {
            var dataPath = Application.dataPath.Replace('\\', '/');
            absolutePath = absolutePath.Replace('\\', '/');
            if (absolutePath.StartsWith(dataPath))
            {
                return "Assets" + absolutePath.Substring(dataPath.Length);
            }

            // Если файл вне проекта — импортнём в Assets/…
            var fileName = Path.GetFileName(absolutePath);
            var newPath = Path.Combine("Assets", fileName).Replace('\\', '/');
            File.Copy(absolutePath, newPath, true);
            return newPath;
        }
    }
}