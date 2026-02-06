using System;
using System.IO;
using Rusleo.Utils.Editor.TimeTracking.Core;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    /// <summary>
    /// Одна сессия = один файл (append-only).
    /// Первая запись — session_start. session_end — best effort.
    /// </summary>
    public interface ITrackingSession : IDisposable
    {
        DeviceId DeviceId { get; }
        SessionId SessionId { get; }
        UnixTime SessionStartUtc { get; }
        FileInfo File { get; }

        void Append(ITrackerEvent ev);
        void Flush();
    }
}