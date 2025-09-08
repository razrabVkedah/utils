using System;
using System.Collections.Generic;

namespace Rusleo.Utils.Runtime.Logging
{
    public sealed class LogEvent
    {
        public DateTime UtcTimestamp { get; }
        public LogLevel Level { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public UnityEngine.Object ContextObject { get; }
        public string Owner { get; } // Произвольная строка (тип/система/подсистема)
        public IReadOnlyDictionary<string, object> Metadata { get; }
        public string[] Tags { get; }
        public string ThreadName { get; }
        public int ThreadId { get; }
        public string CorrelationId { get; } // Для связывания цепочек событий

        public LogEvent(
            LogLevel level,
            string message,
            Exception exception = null,
            UnityEngine.Object contextObject = null,
            string owner = null,
            IReadOnlyDictionary<string, object> metadata = null,
            string[] tags = null,
            string correlationId = null)
        {
            UtcTimestamp = DateTime.UtcNow;
            Level = level;
            Message = message;
            Exception = exception;
            ContextObject = contextObject;
            Owner = owner;
            Metadata = metadata ?? s_emptyDict;
            Tags = tags ?? Array.Empty<string>();
            ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            ThreadName = System.Threading.Thread.CurrentThread.Name ?? "";
            CorrelationId = correlationId;
        }

        private static readonly Dictionary<string, object> s_emptyDict = new();
    }
}