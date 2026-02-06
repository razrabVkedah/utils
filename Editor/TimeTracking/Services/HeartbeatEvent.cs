using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services
{
    public sealed class HeartbeatEvent : ITrackerEvent
    {
        public HeartbeatEvent(
            UnixTime timestampUtc,
            DeviceId deviceId,
            SessionId sessionId,
            int deltaSeconds,
            EditorFlags flags)
        {
            TimestampUtc = timestampUtc;
            DeviceId = deviceId;
            SessionId = sessionId;
            DeltaSeconds = deltaSeconds;
            Flags = flags;
        }

        public TrackerEventKind Kind => TrackerEventKind.Heartbeat;
        public UnixTime TimestampUtc { get; }
        public DeviceId DeviceId { get; }
        public SessionId SessionId { get; }
        public int DeltaSeconds { get; }
        public EditorFlags Flags { get; }
    }
}