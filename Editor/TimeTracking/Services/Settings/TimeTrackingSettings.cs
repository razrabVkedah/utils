using UnityEditor;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Settings
{
    public static class TimeTrackingSettings
    {
        private const string Prefix = "Rusleo.TimeTracking.Settings.";

        private const string EnabledKey = Prefix + "Enabled";
        private const string HeartbeatSecondsKey = Prefix + "HeartbeatSeconds";
        private const string AfkSecondsKey = Prefix + "AfkSeconds";
        private const string TrackerVersionKey = Prefix + "TrackerVersion";

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        public static int HeartbeatSeconds
        {
            get => Clamp(EditorPrefs.GetInt(HeartbeatSecondsKey, 60), 10, 600);
            set => EditorPrefs.SetInt(HeartbeatSecondsKey, Clamp(value, 10, 600));
        }

        public static int AfkSeconds
        {
            get => Clamp(EditorPrefs.GetInt(AfkSecondsKey, 120), 10, 3600);
            set => EditorPrefs.SetInt(AfkSecondsKey, Clamp(value, 10, 3600));
        }

        public static string TrackerVersion
        {
            get => EditorPrefs.GetString(TrackerVersionKey, "0.1.0");
            set => EditorPrefs.SetString(TrackerVersionKey, value ?? "0.1.0");
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}