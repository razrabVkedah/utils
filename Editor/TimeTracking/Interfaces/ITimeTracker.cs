using Rusleo.Utils.Editor.TimeTracking.Core;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface ITimeTracker
    {
        bool IsRunning { get; }
        void Start(bool isFirstStartInThisEditorLaunch, UnixTime startTime);
        void Stop(SessionEndReason reason);
        void OnDomainReload();
    }
}