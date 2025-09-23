using UnityEngine;

namespace Rusleo.Utils.Runtime.Logging
{
    public enum FormatterKind
    {
        Default,
        Minimal,
        Json
    }

    [System.Serializable]
    public sealed class SinkConfig
    {
        public bool enabled = true;
        public FormatterKind formatter = FormatterKind.Default;
    }

    [CreateAssetMenu(fileName = "RusleoLoggingSettings", menuName = "Rusleo/Utils/Logging Settings")]
    public sealed class LogConfig : ScriptableObject
    {
        [Header("General")] public LogLevel minimumLevel = LogLevel.Debug;

        [Header("Sinks")] public SinkConfig unityConsole = new() { enabled = true, formatter = FormatterKind.Minimal };
        public SinkConfig fileSink = new() { enabled = true, formatter = FormatterKind.Default };

        [Header("File Sink")] public string fileName = "game.log";
        public long maxFileBytes = 5_000_000;
        public string relativeDirectory = "Logs";

        [Header("Formatter: Default options")] public DefaultLogFormatterOptions defaultFormatterOptions = new();

        private void OnValidate()
        {
            if (unityConsole == null) unityConsole = new SinkConfig();
            if (fileSink == null) fileSink = new SinkConfig();
            if (defaultFormatterOptions == null) defaultFormatterOptions = new DefaultLogFormatterOptions();
            if (defaultFormatterOptions.MetadataPriorityOrder == null)
                defaultFormatterOptions.MetadataPriorityOrder = new[] { "playerId", "user", "scene", "matchId" };
        }
    }
}