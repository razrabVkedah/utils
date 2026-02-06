using Rusleo.Utils.Editor.TimeTracking.Core;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface ITimeTracker
    {
        bool IsRunning { get; }
        void Start();
        void Stop(SessionEndReason reason);
    }
}