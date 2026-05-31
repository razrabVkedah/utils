using System;
using System.Collections.Generic;

namespace Rusleo.Utils.Editor.Windows.TimeTracking
{
    internal sealed class AnalyticsReport
    {
        public List<DayStat> Days { get; } = new();
        public int TotalActiveSec { get; set; }
        public int TotalSec { get; set; }
        public int TotalPlayModeSec { get; set; }
        public int TotalAfkSec { get; set; }
        public int SessionCount { get; set; }
        public int FileCount { get; set; }
        public int EventCount { get; set; }
    }

    internal struct DayStat
    {
        public DateTime Date;
        public int ActiveSec;
        public int TotalSec;
        public int PlayModeSec;
        public int AfkSec;
    }
}
