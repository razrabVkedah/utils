using System;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using Rusleo.Utils.Editor.TimeTracking.Services.IO;
using UnityEditor;

namespace Rusleo.Utils.Editor.TimeTracking.Services.TimeTracker
{
    public sealed class EditorTimeTracker : ITimeTracker
    {
        private readonly IClock _clock;
        private readonly ILogPathProvider _paths;
        private readonly IDeviceIdProvider _deviceIdProvider;
        private readonly ISessionIdProvider _sessionIdProvider;
        private readonly IProjectIdProvider _projectIdProvider;
        private readonly IUnityContextProvider _unityContextProvider;
        private readonly IEventSerializer _serializer;
        private readonly IHeartbeatPolicy _heartbeatPolicy;
        private readonly IEditorStateProbe _editorState;
        private readonly IInputActivityProbe _inputProbe;
        private readonly TrackerVersion _trackerVersion;

        private ITrackingSession _session;
        private UnixTime _lastHeartbeatUtc;
        private double _nextHeartbeatAt;
        private bool _subscribed;
        private bool _stopping;

        public EditorTimeTracker(
            IClock clock,
            ILogPathProvider paths,
            IDeviceIdProvider deviceIdProvider,
            ISessionIdProvider sessionIdProvider,
            IProjectIdProvider projectIdProvider,
            IUnityContextProvider unityContextProvider,
            IEventSerializer serializer,
            IHeartbeatPolicy heartbeatPolicy,
            IEditorStateProbe editorState,
            IInputActivityProbe inputProbe,
            TrackerVersion trackerVersion)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _paths = paths ?? throw new ArgumentNullException(nameof(paths));
            _deviceIdProvider = deviceIdProvider ?? throw new ArgumentNullException(nameof(deviceIdProvider));
            _sessionIdProvider = sessionIdProvider ?? throw new ArgumentNullException(nameof(sessionIdProvider));
            _projectIdProvider = projectIdProvider ?? throw new ArgumentNullException(nameof(projectIdProvider));
            _unityContextProvider =
                unityContextProvider ?? throw new ArgumentNullException(nameof(unityContextProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _heartbeatPolicy = heartbeatPolicy ?? throw new ArgumentNullException(nameof(heartbeatPolicy));
            _editorState = editorState ?? throw new ArgumentNullException(nameof(editorState));
            _inputProbe = inputProbe ?? throw new ArgumentNullException(nameof(inputProbe));
            _trackerVersion = trackerVersion;
        }

        public bool IsRunning => _session != null;

        public void Start()
        {
            if (_session != null)
                return;

            var now = _clock.UtcNow();
            var deviceId = _deviceIdProvider.GetOrCreate();
            var sessionId = _sessionIdProvider.Create();

            var file = _paths.GetSessionFile(now, deviceId, sessionId);
            var writer = new JsonlFileWriter(file);

            _session = new TrackingSession(
                file: file,
                writer: writer,
                serializer: _serializer,
                deviceId: deviceId,
                sessionId: sessionId,
                sessionStartUtc: now);

            var start = new SessionStartEvent(
                timestampUtc: now,
                unityVersion: _unityContextProvider.GetUnityVersion(),
                projectId: _projectIdProvider.GetProjectId(),
                deviceId: deviceId,
                sessionId: sessionId,
                trackerVersion: _trackerVersion);

            _session.Append(start);
            _session.Flush();

            _lastHeartbeatUtc = now;
            _nextHeartbeatAt = EditorApplication.timeSinceStartup + _heartbeatPolicy.IntervalSeconds;

            Subscribe();
        }

        public void Stop(SessionEndReason reason)
        {
            if (_session == null)
                return;

            if (_stopping)
                return;

            _stopping = true;

            try
            {
                var now = _clock.UtcNow();

                var end = new SessionEndEvent(
                    timestampUtc: now,
                    deviceId: _session.DeviceId,
                    sessionId: _session.SessionId,
                    reason: reason);

                _session.Append(end);
                _session.Flush();
            }
            catch
            {
                // best-effort
            }
            finally
            {
                Unsubscribe();

                _session.Dispose();
                _session = null;
                _stopping = false;
            }
        }

        private void Subscribe()
        {
            if (_subscribed)
                return;

            _subscribed = true;
            EditorApplication.update += OnUpdate;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            EditorApplication.quitting += OnQuitting;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
                return;

            _subscribed = false;
            EditorApplication.update -= OnUpdate;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeReload;
            EditorApplication.quitting -= OnQuitting;
        }

        private void OnBeforeReload()
        {
            Stop(SessionEndReason.Reload);
        }

        private void OnQuitting()
        {
            Stop(SessionEndReason.Quit);
        }

        private void OnUpdate()
        {
            if (_session == null)
                return;

            var nowEditor = EditorApplication.timeSinceStartup;
            if (nowEditor < _nextHeartbeatAt)
                return;

            var nowUtc = _clock.UtcNow();
            var dt = (int)Math.Max(0, nowUtc.Value - _lastHeartbeatUtc.Value);

            var flags = new EditorFlags(
                isPlayMode: _editorState.IsPlayMode,
                isAfk: _inputProbe.IsAfk,
                isFocused: _editorState.IsFocused,
                isCompiling: _editorState.IsCompiling);

            var hb = new HeartbeatEvent(
                timestampUtc: nowUtc,
                deviceId: _session.DeviceId,
                sessionId: _session.SessionId,
                deltaSeconds: dt,
                flags: flags);

            _session.Append(hb);
            _session.Flush();

            _lastHeartbeatUtc = nowUtc;
            _nextHeartbeatAt = nowEditor + _heartbeatPolicy.IntervalSeconds;
        }
    }
}