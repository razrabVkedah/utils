using Rusleo.Utils.Editor.TimeTracking.Core;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface ITrackerEvent
    {
        TrackerEventKind Kind { get; }
        UnixTime TimestampUtc { get; }
        DeviceId DeviceId { get; }
        SessionId SessionId { get; }
    }
}