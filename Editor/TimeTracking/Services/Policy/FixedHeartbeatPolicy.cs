using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Policy
{
    public sealed class FixedHeartbeatPolicy : IHeartbeatPolicy
    {
        public FixedHeartbeatPolicy(int intervalSeconds)
        {
            IntervalSeconds = intervalSeconds <= 0 ? 60 : intervalSeconds;
        }

        public int IntervalSeconds { get; }
    }
}