using System;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Ids
{
    public sealed class EditorLaunchSessionIdProvider : ISessionIdProvider
    {
        private const string SessionIdKey = "Rusleo.TimeTracking.SessionId";
        private const string SessionStartedKey = "Rusleo.TimeTracking.SessionStarted";
        private const string SessionStartUtcKey = "Rusleo.TimeTracking.SessionStartUtc";

        public SessionId GetOrCreate()
        {
            var raw = GetOrCreateEditorLaunchSessionId(out _);
            return new SessionId(raw);
        }

        public SessionId GetOrCreate(out bool isFirstStartInThisEditorLaunch)
        {
            var raw = GetOrCreateEditorLaunchSessionId(out isFirstStartInThisEditorLaunch);
            return new SessionId(raw);
        }
        
        public UnixTime GetOrCreateSessionStartUtc(IClock clock)
        {
            var existing = SessionState.GetString(SessionStartUtcKey, string.Empty);
            if (long.TryParse(existing, out var parsed) && parsed > 0)
                return new UnixTime(parsed);

            var now = clock.UtcNow();
            SessionState.SetString(SessionStartUtcKey, now.Value.ToString());
            return now;
        }

        private static string GetOrCreateEditorLaunchSessionId(out bool isFirstStart)
        {
            var started = SessionState.GetBool(SessionStartedKey, false);
            var existing = SessionState.GetString(SessionIdKey, string.Empty);

            if (!started || string.IsNullOrEmpty(existing))
            {
                var newId = Guid.NewGuid().ToString("N");
                SessionState.SetString(SessionIdKey, newId);
                SessionState.SetBool(SessionStartedKey, true);

                isFirstStart = true;
                return newId;
            }

            isFirstStart = false;
            return existing;
        }
    }
}