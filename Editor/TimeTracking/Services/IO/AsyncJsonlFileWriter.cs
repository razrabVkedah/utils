using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.IO
{
    public sealed class AsyncJsonlFileWriter : IJsonlWriter
    {
        private readonly StreamWriter _writer;
        private readonly ConcurrentQueue<string> _queue;
        private readonly AutoResetEvent _signal;
        private readonly ManualResetEventSlim _flushedEvent;
        private readonly CancellationTokenSource _cts;
        private readonly Thread _thread;

        private int _pendingCount;
        private int _disposed;

        public AsyncJsonlFileWriter(FileInfo file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));

            var stream = new FileStream(
                File.FullName,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read);

            _writer = new StreamWriter(stream, new UTF8Encoding(false))
            {
                AutoFlush = false
            };

            _queue = new ConcurrentQueue<string>();
            _signal = new AutoResetEvent(false);
            _flushedEvent = new ManualResetEventSlim(true);
            _cts = new CancellationTokenSource();

            _thread = new Thread(WorkerLoop)
            {
                IsBackground = true,
                Name = "Rusleo.TimeTracking.JsonlWriter"
            };
            _thread.Start();
        }

        public FileInfo File { get; }

        public void AppendLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            if (Volatile.Read(ref _disposed) == 1) return;

            _queue.Enqueue(line);
            Interlocked.Increment(ref _pendingCount);

            _flushedEvent.Reset();
            _signal.Set();
        }

        public void Flush()
        {
            if (Volatile.Read(ref _disposed) == 1) return;

            _signal.Set();

            if (Volatile.Read(ref _pendingCount) == 0)
            {
                try { _writer.Flush(); } catch { /* best-effort */ }
                _flushedEvent.Set();
                return;
            }

            _flushedEvent.Wait();
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;

            try
            {
                _cts.Cancel();
                _signal.Set();
                _thread.Join(2000);
            }
            catch
            {
                // best-effort
            }

            try
            {
                DrainQueue();
                _writer.Flush();
            }
            catch
            {
                // best-effort
            }
            finally
            {
                _writer.Dispose();
                _signal.Dispose();
                _flushedEvent.Dispose();
                _cts.Dispose();
            }
        }

        private void WorkerLoop()
        {
            var token = _cts.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    _signal.WaitOne(250);

                    if (token.IsCancellationRequested)
                        break;

                    DrainQueue();

                    if (Volatile.Read(ref _pendingCount) == 0)
                    {
                        try { _writer.Flush(); } catch { /* best-effort */ }
                        _flushedEvent.Set();
                    }
                }
            }
            catch
            {
                // best-effort: writer может умереть, но Editor должен жить
            }
        }

        private void DrainQueue()
        {
            var wroteAny = false;

            while (_queue.TryDequeue(out var line))
            {
                _writer.WriteLine(line);
                wroteAny = true;
                Interlocked.Decrement(ref _pendingCount);
            }

            if (wroteAny && Volatile.Read(ref _pendingCount) == 0)
                _flushedEvent.Set();
        }
    }
}
