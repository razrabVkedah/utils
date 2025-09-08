using System;
using System.IO;
using System.Text;

namespace Rusleo.Utils.Runtime.Logging.Sinks
{
    public sealed class FileSink : ILogSink
    {
        private readonly ILogFormatter _formatter;
        private readonly string _path;
        private readonly object _lock = new();
        private StreamWriter _writer;
        private readonly long _maxBytes;
        private long _written; // для грубой ротации

        public FileSink(ILogFormatter formatter, string directory = null, string fileName = "game.log", long maxBytes = 5_000_000)
        {
            _formatter = formatter;
            var dir = directory ?? UnityEngine.Application.persistentDataPath + "/Logs";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, fileName);
            _maxBytes = maxBytes;
            _writer = new StreamWriter(new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read), Encoding.UTF8)
            {
                AutoFlush = true
            };
            _written = new FileInfo(_path).Length;
        }

        public void Emit(in LogEvent e)
        {
            var line = _formatter.Format(e);
            lock (_lock)
            {
                _writer.WriteLine(line);
                _written += Encoding.UTF8.GetByteCount(line) + 2;
                if (_written >= _maxBytes)
                {
                    Rotate();
                }
            }
        }

        private void Rotate()
        {
            try
            {
                _writer?.Dispose();
                var ts = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var archive = Path.ChangeExtension(_path, $".{ts}.log");
                File.Move(_path, archive);
            }
            catch { /* best effort */ }
            finally
            {
                _writer = new StreamWriter(new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8)
                {
                    AutoFlush = true
                };
                _written = 0;
            }
        }

        public void Flush()
        {
            lock (_lock) _writer?.Flush();
        }

        public void Dispose()
        {
            lock (_lock) _writer?.Dispose();
        }
    }
}