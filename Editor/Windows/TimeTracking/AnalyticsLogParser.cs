using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Rusleo.Utils.Editor.Windows.TimeTracking
{
    internal static class AnalyticsLogParser
    {
        // Heartbeats with dt larger than this are gaps from sleep/suspend — capped to avoid counting idle time.
        private const int MaxGapSeconds = 300;

        public static AnalyticsReport Parse(string sessionsDir)
        {
            var report = new AnalyticsReport();
            if (!Directory.Exists(sessionsDir))
                return report;

            var files = Directory.GetFiles(sessionsDir, "*.jsonl");
            report.FileCount = files.Length;

            var dayMap = new Dictionary<DateTime, DayStat>();
            var sessionIds = new HashSet<string>();
            var eventCount = 0;

            foreach (var file in files)
            {
                try { ParseFile(file, dayMap, sessionIds, ref eventCount); }
                catch { }
            }

            report.SessionCount = sessionIds.Count;
            report.EventCount = eventCount;
            report.Days.AddRange(dayMap.Values.OrderBy(d => d.Date));

            foreach (var day in report.Days)
            {
                report.TotalActiveSec += day.ActiveSec;
                report.TotalSec += day.TotalSec;
                report.TotalPlayModeSec += day.PlayModeSec;
                report.TotalAfkSec += day.AfkSec;
            }

            return report;
        }

        private static void ParseFile(string path, Dictionary<DateTime, DayStat> dayMap, HashSet<string> sessionIds, ref int eventCount)
        {
            foreach (var line in File.ReadLines(path))
            {
                if (string.IsNullOrEmpty(line)) continue;
                ParseLine(line, dayMap, sessionIds, ref eventCount);
            }
        }

        private static void ParseLine(string line, Dictionary<DateTime, DayStat> dayMap, HashSet<string> sessionIds, ref int eventCount)
        {
            if (!TryGetString(line, "k", out var kind)) return;

            if (kind == "session_start")
            {
                if (TryGetString(line, "s", out var sid))
                    sessionIds.Add(sid);
                return;
            }

            if (kind != "heartbeat") return;

            if (!TryGetLong(line, "ts", out var ts)) return;
            if (!TryGetInt(line, "dt", out var dt)) return;

            TryGetInt(line, "pm", out var pm);
            TryGetInt(line, "afk", out var afk);

            var capped = Math.Min(dt, MaxGapSeconds);
            var date = DateTimeOffset.FromUnixTimeSeconds(ts).LocalDateTime.Date;

            if (!dayMap.TryGetValue(date, out var stat))
                stat = new DayStat { Date = date };

            stat.TotalSec += capped;

            if (afk == 0)
                stat.ActiveSec += capped;
            else
                stat.AfkSec += capped;

            if (pm == 1)
                stat.PlayModeSec += capped;

            dayMap[date] = stat;
            eventCount++;
        }

        private static bool TryGetString(string json, string key, out string value)
        {
            value = null;
            var search = $"\"{key}\":\"";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return false;
            idx += search.Length;
            var end = json.IndexOf('"', idx);
            if (end < 0) return false;
            value = json.Substring(idx, end - idx);
            return true;
        }

        private static bool TryGetInt(string json, string key, out int value)
        {
            value = 0;
            var search = $"\"{key}\":";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return false;
            idx += search.Length;
            var end = idx;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-'))
                end++;
            return int.TryParse(json.Substring(idx, end - idx), out value);
        }

        private static bool TryGetLong(string json, string key, out long value)
        {
            value = 0;
            var search = $"\"{key}\":";
            var idx = json.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return false;
            idx += search.Length;
            var end = idx;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-'))
                end++;
            return long.TryParse(json.Substring(idx, end - idx), out value);
        }
    }
}
