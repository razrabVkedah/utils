namespace Rusleo.Utils.Editor.TimeTracking.Services.Json
{
    internal static class Wire
    {
        internal static class Keys
        {
            internal const string Kind = "k";
            internal const string Timestamp = "ts";
            internal const string DeviceId = "d";
            internal const string SessionId = "s";

            internal const string UnityVersion = "uv";
            internal const string ProjectId = "pid";
            internal const string TrackerVersion = "tv";

            internal const string DeltaSeconds = "dt";
            internal const string PlayMode = "pm";
            internal const string Afk = "afk";
            internal const string Focused = "fc";
            internal const string Compiling = "cp";

            internal const string EndReason = "r";
        }

        internal static class Kinds
        {
            internal const string SessionStart = "session_start";
            internal const string Heartbeat = "heartbeat";
            internal const string SessionEnd = "session_end";
            internal const string Unknown = "unknown";
        }

        internal static class EndReasons
        {
            internal const string Quit = "quit";
            internal const string Reload = "reload";
            internal const string Unknown = "unknown";
        }
    }
}