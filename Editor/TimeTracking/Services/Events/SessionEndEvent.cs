using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Events
{
    public sealed class SessionEndEvent : ITrackerEvent
    {
        public SessionEndEvent(
            UnixTime timestampUtc,
            DeviceId deviceId,
            SessionId sessionId,
            SessionEndReason reason)
        {
            TimestampUtc = timestampUtc;
            DeviceId = deviceId;
            SessionId = sessionId;
            Reason = reason;
        }

        public TrackerEventKind Kind => TrackerEventKind.SessionEnd;
        public UnixTime TimestampUtc { get; }
        public DeviceId DeviceId { get; }
        public SessionId SessionId { get; }
        public SessionEndReason Reason { get; }
    }
}