using System.IO;
using Rusleo.Utils.Editor.TimeTracking.Core;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface ILogPathProvider
    {
        /// <summary>
        /// ProjectSettings/RusleoTimeTracking/sessions/
        /// </summary>
        DirectoryInfo GetSessionsDirectory();

        /// <summary>
        /// {sessionStartUtc}__{deviceId}__{sessionId}.jsonl
        /// </summary>
        FileInfo GetSessionFile(UnixTime sessionStartUtc, DeviceId deviceId, SessionId sessionId);
    }
}