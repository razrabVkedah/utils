using UnityEditor;
using UnityEngine;
using System.IO;
using Rusleo.Utils.Runtime.Logging;
using UnityEngine.UIElements;

namespace Rusleo.Utils.Editor.Windows.Logging
{
    internal sealed class LoggingSettingsProvider : SettingsProvider
    {
        private const string RESOURCES_DIR = "Assets/Resources/LoggingSettings";
        private const string ASSET_PATH = "Assets/Resources/LoggingSettings/RusleoLoggingSettings.asset";
        private LogConfig _config;
        private SerializedObject _so;

        public LoggingSettingsProvider(string path, SettingsScope scope) : base(path, scope)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
            => new LoggingSettingsProvider("Project/Rusleo Utils/Logging", SettingsScope.Project);

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _config = Resources.Load<LogConfig>("RusleoLoggingSettings");
            if (_config == null)
            {
                if (!Directory.Exists(RESOURCES_DIR))
                    Directory.CreateDirectory(RESOURCES_DIR);

                _config = ScriptableObject.CreateInstance<LogConfig>();
                AssetDatabase.CreateAsset(_config, ASSET_PATH);
                AssetDatabase.SaveAssets();
            }

            EnsureConfigIntegrity(_config);
            _so = new SerializedObject(_config);
        }

        public override void OnGUI(string searchContext)
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("Config asset missing. Reopen the window.", MessageType.Warning);
                return;
            }

            if (_so == null) _so = new SerializedObject(_config);

            _so.Update();

            EditorGUILayout.LabelField("Rusleo.Utils.Logging", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // --- General (SerializedObject)
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            DrawEnum(_so, "minimumLevel", "Minimum Level");
            EditorGUILayout.Space(6);

            // --- Sinks (SerializedProperty)
            EditorGUILayout.LabelField("Sinks", EditorStyles.boldLabel);
            var pUnityConsole = _so.FindProperty("unityConsole");
            var pFileSink = _so.FindProperty("fileSink");

            DrawSinkSection("Unity Console", pUnityConsole);
            DrawSinkSection("File Sink", pFileSink);

            // --- File settings (SerializedObject)
            if (_config.fileSink != null && _config.fileSink.enabled)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawString(_so, "fileName", "File Name");
                    DrawLong(_so, "maxFileBytes", "Max File Bytes");
                    DrawString(_so, "relativeDirectory", "Relative Directory");
                }
            }

            EditorGUILayout.Space(6);

            // --- Default Formatter Options (SerializedProperty)
            EditorGUILayout.LabelField("Default Formatter Options", EditorStyles.boldLabel);
            var fmt = _so.FindProperty("defaultFormatterOptions");
            if (fmt != null && !fmt.isArray)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawBool(fmt, "IncludeTimestamp", "Include Timestamp");
                    DrawBool(fmt, "IncludeOwner", "Include Owner");
                    DrawBool(fmt, "IncludeTags", "Include Tags");
                    DrawBool(fmt, "IncludeMetadata", "Include Metadata");
                    DrawBool(fmt, "IncludeCorrId", "Include Correlation Id");
                    DrawBool(fmt, "MultilineException", "Multiline Exception");

                    DrawInt(fmt, "MetadataMaxCount", "Metadata Max Count");
                    DrawStringArray(fmt, "MetadataPriorityOrder", "Priority Keys");
                    DrawBool(fmt, "SortRemainingMetaAlphabetically", "Sort Remaining Alphabetically");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Formatter options not found or not serializable.", MessageType.Warning);
            }

            EditorGUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Logs Folder"))
                {
                    var folder = Path.Combine(Application.persistentDataPath, _config.relativeDirectory);
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    EditorUtility.RevealInFinder(folder);
                }

                if (GUILayout.Button("Open Current Log"))
                {
                    var file = Path.Combine(Application.persistentDataPath, _config.relativeDirectory,
                        _config.fileName);
                    if (File.Exists(file)) EditorUtility.RevealInFinder(file);
                    else EditorUtility.DisplayDialog("Rusleo Logging", "Log file not found yet.", "OK");
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Reinitialize"))
                {
                    SafeReinitialize();
                }
            }

            if (_so.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                SafeReinitialize();
            }
        }

        // ----------------- helpers: SerializedObject -----------------

        private static void DrawEnum(SerializedObject so, string name, string label)
        {
            var p = so.FindProperty(name);
            if (p != null) EditorGUILayout.PropertyField(p, new GUIContent(label));
        }

        private static void DrawString(SerializedObject so, string name, string label)
        {
            var p = so.FindProperty(name);
            if (p != null) EditorGUILayout.PropertyField(p, new GUIContent(label));
        }

        private static void DrawLong(SerializedObject so, string name, string label)
        {
            var p = so.FindProperty(name);
            if (p != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                long val = p.longValue;
                val = EditorGUILayout.LongField(val);
                p.longValue = (long)Mathf.Max(0, val);
                EditorGUILayout.EndHorizontal();
            }
        }

        // ----------------- helpers: SerializedProperty -----------------

        private static void DrawSinkSection(string title, SerializedProperty sinkProp)
        {
            if (sinkProp == null)
            {
                EditorGUILayout.HelpBox($"{title} property not found.", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawBool(sinkProp, "enabled", "Enabled");
                    DrawEnum(sinkProp, "formatter", "Formatter");
                }
            }
        }

        private static void DrawBool(SerializedProperty root, string relativeName, string label)
        {
            var p = root.FindPropertyRelative(relativeName);
            if (p != null) EditorGUILayout.PropertyField(p, new GUIContent(label));
        }

        private static void DrawEnum(SerializedProperty root, string relativeName, string label)
        {
            var p = root.FindPropertyRelative(relativeName);
            if (p != null) EditorGUILayout.PropertyField(p, new GUIContent(label));
        }

        private static void DrawInt(SerializedProperty root, string relativeName, string label)
        {
            var p = root.FindPropertyRelative(relativeName);
            if (p != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(label);
                int val = p.intValue;
                val = EditorGUILayout.IntField(val);
                p.intValue = Mathf.Max(0, val);
                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawStringArray(SerializedProperty root, string relativeName, string label)
        {
            var arr = root.FindPropertyRelative(relativeName);
            if (arr == null || !arr.isArray)
            {
                EditorGUILayout.HelpBox($"{label} array not found.", MessageType.None);
                return;
            }

            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField(label);
                using (new EditorGUI.IndentLevelScope())
                {
                    int newSize = Mathf.Max(0, EditorGUILayout.IntField("Size", arr.arraySize));
                    if (newSize != arr.arraySize) arr.arraySize = newSize;

                    for (int i = 0; i < arr.arraySize; i++)
                    {
                        var el = arr.GetArrayElementAtIndex(i);
                        el.stringValue = EditorGUILayout.TextField($"Element {i}", el.stringValue);
                    }
                }
            }
        }

        // ----------------- misc -----------------

        private static void EnsureConfigIntegrity(LogConfig cfg)
        {
            if (cfg.unityConsole == null) cfg.unityConsole = new SinkConfig();
            if (cfg.fileSink == null) cfg.fileSink = new SinkConfig();
            if (cfg.defaultFormatterOptions == null) cfg.defaultFormatterOptions = new DefaultLogFormatterOptions();
            if (cfg.defaultFormatterOptions.MetadataPriorityOrder == null)
                cfg.defaultFormatterOptions.MetadataPriorityOrder = new[] { "playerId", "user", "scene", "matchId" };
            EditorUtility.SetDirty(cfg);
        }

        private static void SafeReinitialize()
        {
            LogDispatcher.Instance.Dispose();
            EditorApplication.delayCall += () =>
            {
                var method = typeof(LoggingBootstrap).GetMethod("Init",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                method?.Invoke(null, null);
            };
        }
    }
}