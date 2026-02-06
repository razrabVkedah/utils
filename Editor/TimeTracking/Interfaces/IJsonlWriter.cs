using System;
using System.IO;

namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface IJsonlWriter : IDisposable
    {
        FileInfo File { get; }
        void AppendLine(string line);
        void Flush();
    }
}