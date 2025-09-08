using UnityEngine;

namespace Rusleo.Utils.Runtime.Logging.Sinks
{
    public sealed class UnityConsoleSink : ILogSink
    {
        private readonly ILogFormatter _formatter;

        public UnityConsoleSink(ILogFormatter formatter) => _formatter = formatter;

        public void Emit(in LogEvent e)
        {
            var msg = _formatter.Format(e);
            switch (e.Level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    if (e.ContextObject) Debug.Log(msg, e.ContextObject);
                    else Debug.Log(msg);
                    break;
                case LogLevel.Warn:
                    if (e.ContextObject) Debug.LogWarning(msg, e.ContextObject);
                    else Debug.LogWarning(msg);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    if (e.ContextObject) Debug.LogError(msg, e.ContextObject);
                    else Debug.LogError(msg);
                    break;
            }
        }

        public void Flush() { }
        public void Dispose() { }
    }
}