using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using Rusleo.Utils.Editor.TimeTracking.Services.Ids;
using Rusleo.Utils.Editor.TimeTracking.Services.IO;
using Rusleo.Utils.Editor.TimeTracking.Services.Json;
using Rusleo.Utils.Editor.TimeTracking.Services.Policy;
using Rusleo.Utils.Editor.TimeTracking.Services.Probes;
using Rusleo.Utils.Editor.TimeTracking.Services.Settings;
using Rusleo.Utils.Editor.TimeTracking.Services.Systems;
using Rusleo.Utils.Editor.TimeTracking.Services.TimeTracker;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Bootstrap
{
    [InitializeOnLoad]
    public static class TimeTrackingBootstrap
    {
        private static readonly ITimeTracker Tracker;

        static TimeTrackingBootstrap()
        {
            if (!TimeTrackingSettings.Enabled)
                return; 
            
#if UNITY_2020_2_OR_NEWER
            if (AssetDatabase.IsAssetImportWorkerProcess())
                return;
#endif
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
                return;

            var clock = new ClockUtcSeconds();
            var sessionIdProvider = new EditorLaunchSessionIdProvider();
            _ = sessionIdProvider.GetOrCreate(out var isFirstStart);
            var editorLaunchStartUtc = sessionIdProvider.GetOrCreateSessionStartUtc(clock);

            Tracker = new EditorTimeTracker(
                clock: clock,
                paths: new LogPathProvider(),
                deviceIdProvider: new EditorPrefsDeviceIdProvider(),
                sessionIdProvider: sessionIdProvider,
                projectIdProvider: new FileProjectIdProvider(),
                unityContextProvider: new UnityContextProvider(),
                serializer: new WireEventSerializer(),
                heartbeatPolicy: new FixedHeartbeatPolicy(TimeTrackingSettings.HeartbeatSeconds),
                editorState: new EditorStateProbe(),
                inputProbe: new InputActivityProbeV2(TimeTrackingSettings.AfkSeconds),
                trackerVersion: new TrackerVersion(TimeTrackingSettings.TrackerVersion));

            Tracker.Start(isFirstStart, editorLaunchStartUtc);
            Debug.Log("Create editor time tracker for session");

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
        }

        private static void OnBeforeAssemblyReload()
        {
            Tracker?.OnDomainReload();
        }

        private static void OnEditorQuitting()
        {
            Tracker?.Stop(SessionEndReason.Quit);
        }
    }
}