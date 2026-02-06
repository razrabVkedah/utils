using System;
using System.Text;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Json
{
    public sealed class WireEventSerializer : IEventSerializer
    {
        public string SerializeLine(ITrackerEvent ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));

            var sb = new StringBuilder(256);

            sb.Append('{');

            WriteProp(sb, "k", KindToWire(ev.Kind), true);
            WriteProp(sb, "ts", ev.TimestampUtc.Value, false);
            WriteProp(sb, "d", ev.DeviceId.Value, false);
            WriteProp(sb, "s", ev.SessionId.Value, false);

            if (ev is SessionStartEvent start)
            {
                WriteProp(sb, "uv", start.UnityVersion, false);
                WriteProp(sb, "pid", start.ProjectId.Value, false);
                WriteProp(sb, "tv", start.TrackerVersion.Value, false);
            }
            else if (ev is HeartbeatEvent hb)
            {
                WriteProp(sb, "dt", hb.DeltaSeconds, false);
                WriteProp(sb, "pm", hb.Flags.IsPlayMode ? 1 : 0, false);
                WriteProp(sb, "afk", hb.Flags.IsAfk ? 1 : 0, false);

                if (hb.Flags.IsFocused.HasValue)
                    WriteProp(sb, "fc", hb.Flags.IsFocused.Value ? 1 : 0, false);

                if (hb.Flags.IsCompiling.HasValue)
                    WriteProp(sb, "cp", hb.Flags.IsCompiling.Value ? 1 : 0, false);
            }
            else if (ev is SessionEndEvent end)
            {
                WriteProp(sb, "r", EndReasonToWire(end.Reason), false);
            }

            sb.Append('}');

            return sb.ToString();
        }

        private static string KindToWire(TrackerEventKind kind)
        {
            return kind switch
            {
                TrackerEventKind.SessionStart => "session_start",
                TrackerEventKind.Heartbeat => "heartbeat",
                TrackerEventKind.SessionEnd => "session_end",
                _ => "unknown"
            };
        }

        private static string EndReasonToWire(SessionEndReason reason)
        {
            return reason switch
            {
                SessionEndReason.Quit => "quit",
                SessionEndReason.Reload => "reload",
                SessionEndReason.Unknown => "unknown",
                _ => "unknown"
            };
        }

        private static void WriteProp(StringBuilder sb, string key, string value, bool isFirst)
        {
            if (!isFirst) sb.Append(',');
            sb.Append('\"').Append(key).Append("\":\"");
            AppendEscaped(sb, value);
            sb.Append('\"');
        }

        private static void WriteProp(StringBuilder sb, string key, long value, bool isFirst)
        {
            if (!isFirst) sb.Append(',');
            sb.Append('\"').Append(key).Append("\":").Append(value);
        }

        private static void WriteProp(StringBuilder sb, string key, int value, bool isFirst)
        {
            if (!isFirst) sb.Append(',');
            sb.Append('\"').Append(key).Append("\":").Append(value);
        }

        private static void AppendEscaped(StringBuilder sb, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '\"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
        }
    }
}