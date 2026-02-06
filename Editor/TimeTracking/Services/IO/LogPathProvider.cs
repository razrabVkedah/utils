using System.IO;
using Rusleo.Utils.Editor.TimeTracking.Core;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.IO
{
    namespace Rusleo.Utils.Editor.TimeTracking
    {
        public sealed class LogPathProvider : ILogPathProvider
        {
            private const string RootDir = "ProjectSettings/RusleoTimeTracking/sessions";

            public DirectoryInfo GetSessionsDirectory()
            {
                var path = Path.Combine(Application.dataPath, "..", RootDir);
                var full = Path.GetFullPath(path);

                if (!Directory.Exists(full))
                    Directory.CreateDirectory(full);

                return new DirectoryInfo(full);
            }

            public FileInfo GetSessionFile(UnixTime sessionStartUtc, DeviceId deviceId, SessionId sessionId)
            {
                var dir = GetSessionsDirectory();
                var fileName = $"{sessionStartUtc.Value}__{deviceId.Value}__{sessionId.Value}.jsonl";
                var fullPath = Path.Combine(dir.FullName, fileName);

                return new FileInfo(fullPath);
            }
        }
    }
}