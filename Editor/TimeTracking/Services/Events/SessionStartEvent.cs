using System;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Events
{
    public sealed class SessionStartEvent : ITrackerEvent
    {
        public SessionStartEvent(
            UnixTime timestampUtc,
            string unityVersion,
            ProjectId projectId,
            DeviceId deviceId,
            SessionId sessionId,
            TrackerVersion trackerVersion)
        {
            TimestampUtc = timestampUtc;
            UnityVersion = unityVersion ?? throw new ArgumentNullException(nameof(unityVersion));
            ProjectId = projectId;
            DeviceId = deviceId;
            SessionId = sessionId;
            TrackerVersion = trackerVersion;
        }

        public TrackerEventKind Kind => TrackerEventKind.SessionStart;
        public UnixTime TimestampUtc { get; }
        public string UnityVersion { get; }
        public ProjectId ProjectId { get; }
        public DeviceId DeviceId { get; }
        public SessionId SessionId { get; }
        public TrackerVersion TrackerVersion { get; }
    }
}