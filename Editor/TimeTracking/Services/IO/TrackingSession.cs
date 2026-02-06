using System;
using System.IO;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.IO
{
    public sealed class TrackingSession : ITrackingSession
    {
        private readonly IJsonlWriter _writer;
        private readonly IEventSerializer _serializer;

        public TrackingSession(
            FileInfo file,
            IJsonlWriter writer,
            IEventSerializer serializer,
            DeviceId deviceId,
            SessionId sessionId,
            UnixTime sessionStartUtc)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            DeviceId = deviceId;
            SessionId = sessionId;
            SessionStartUtc = sessionStartUtc;
        }

        public DeviceId DeviceId { get; }
        public SessionId SessionId { get; }
        public UnixTime SessionStartUtc { get; }
        public FileInfo File { get; }

        public void Append(ITrackerEvent ev)
        {
            if (ev == null) throw new ArgumentNullException(nameof(ev));

            var line = _serializer.SerializeLine(ev);
            _writer.AppendLine(line);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}