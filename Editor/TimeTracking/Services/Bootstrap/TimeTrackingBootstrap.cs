using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using Rusleo.Utils.Editor.TimeTracking.Services.Ids;
using Rusleo.Utils.Editor.TimeTracking.Services.IO.Rusleo.Utils.Editor.TimeTracking;
using Rusleo.Utils.Editor.TimeTracking.Services.Json;
using Rusleo.Utils.Editor.TimeTracking.Services.Policy;
using Rusleo.Utils.Editor.TimeTracking.Services.Probes;
using Rusleo.Utils.Editor.TimeTracking.Services.Systems;
using Rusleo.Utils.Editor.TimeTracking.Services.TimeTracker;
using UnityEditor;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Bootstrap
{
    [InitializeOnLoad]
    public static class TimeTrackingBootstrap
    {
        private static readonly ITimeTracker Tracker;

        static TimeTrackingBootstrap()
        {
            Tracker = new EditorTimeTracker(
                clock: new ClockUtcSeconds(),
                paths: new LogPathProvider(),
                deviceIdProvider: new EditorPrefsDeviceIdProvider(),
                sessionIdProvider: new GuidSessionIdProvider(),
                projectIdProvider: new ProjectIdProvider(),
                unityContextProvider: new UnityContextProvider(),
                serializer: new WireEventSerializer(),
                heartbeatPolicy: new FixedHeartbeatPolicy(60),
                editorState: new EditorStateProbe(),
                inputProbe: new InputActivityProbe(120.0),
                trackerVersion: new TrackerVersion("0.1.0"));

            Tracker.Start();
        }
    }
}