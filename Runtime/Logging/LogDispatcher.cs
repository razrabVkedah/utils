using System;
using System.Collections.Generic;

namespace Rusleo.Utils.Runtime.Logging
{
    public sealed class LogDispatcher : IDisposable
    {
        public static LogDispatcher Instance { get; private set; } = new();
        public delegate void EmittedHandler(in LogEvent e);
        public event EmittedHandler Emitted; // для Editor Viewer
        private readonly List<ILogSink> _sinks = new(4);
        private readonly List<ILogFilter> _filters = new(2);

        public void AddSink(ILogSink sink) => _sinks.Add(sink);
        public void RemoveSink(ILogSink sink) => _sinks.Remove(sink);

        public void AddFilter(ILogFilter filter) => _filters.Add(filter);
        public void RemoveFilter(ILogFilter filter) => _filters.Remove(filter);

        public void Emit(in LogEvent e)
        {
            // фильтры
            for (int i = 0; i < _filters.Count; i++)
                if (!_filters[i].ShouldLog(in e))
                    return;

            // распространение по приёмникам
            for (int i = 0; i < _sinks.Count; i++)
                _sinks[i].Emit(in e);
            Emitted?.Invoke(in e);
        }

        public void Flush()
        {
            foreach (var s in _sinks) s.Flush();
        }

        public void Dispose()
        {
            foreach (var s in _sinks) s.Dispose();
            _sinks.Clear();
            _filters.Clear();
        }
    }
}