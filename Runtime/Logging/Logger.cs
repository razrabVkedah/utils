using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Logging
{
    public sealed class Logger
    {
        public string Owner { get; }
        public UnityEngine.Object ContextObject { get; }
        public string CorrelationId { get; set; }

        private readonly Dictionary<string, object> _metadata = new();
        private string[] _tags = Array.Empty<string>();

        public Logger(string owner, UnityEngine.Object context = null, IDictionary<string, object> metadata = null,
            string[] tags = null, string correlationId = null)
        {
            Owner = owner;
            ContextObject = context;
            if (metadata != null)
                foreach (var kv in metadata)
                    _metadata[kv.Key] = kv.Value;
            if (tags != null) _tags = tags;
            CorrelationId = correlationId;
        }

        public Logger WithMeta(string key, object value)
        {
            _metadata[key] = value;
            return this;
        }

        public Logger WithTags(params string[] tags)
        {
            _tags = tags ?? Array.Empty<string>();
            return this;
        }

        public void Trace(string msg) => Write(LogLevel.Trace, msg);
        public void Debug(string msg) => Write(LogLevel.Debug, msg);
        public void Info(string msg) => Write(LogLevel.Info, msg);
        public void Warn(string msg) => Write(LogLevel.Warn, msg);
        public void Error(string msg, Exception ex = null) => Write(LogLevel.Error, msg, ex);
        public void Fatal(string msg, Exception ex = null) => Write(LogLevel.Fatal, msg, ex);

        private void Write(LogLevel level, string msg, Exception ex = null)
        {
            var e = new LogEvent(
                level: level,
                message: msg,
                exception: ex,
                contextObject: ContextObject,
                owner: Owner,
                metadata: _metadata,
                tags: _tags,
                correlationId: CorrelationId
            );
            LogDispatcher.Instance.Emit(in e);
        }
    }
}