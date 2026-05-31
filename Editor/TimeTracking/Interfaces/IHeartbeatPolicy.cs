namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface IHeartbeatPolicy
    {
        /// <summary>
        /// Период heartbeat в секундах.
        /// </summary>
        int IntervalSeconds { get; }
    }
}