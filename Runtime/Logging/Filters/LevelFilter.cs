namespace Rusleo.Utils.Runtime.Logging.Filters
{
    public sealed class LevelFilter : ILogFilter
    {
        public LogLevel Minimum { get; set; }

        public LevelFilter(LogLevel min) => Minimum = min;

        public bool ShouldLog(in LogEvent e) => e.Level >= Minimum;
    }
}