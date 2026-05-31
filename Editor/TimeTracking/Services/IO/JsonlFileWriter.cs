using System;
using System.IO;
using System.Text;
using Rusleo.Utils.Editor.TimeTracking.Interfaces;

namespace Rusleo.Utils.Editor.TimeTracking.Services.IO
{
    public sealed class JsonlFileWriter : IJsonlWriter
    {
        private readonly StreamWriter _writer;

        public JsonlFileWriter(FileInfo file)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));

            var stream = new FileStream(
                File.FullName,
                FileMode.Append,
                FileAccess.Write,
                FileShare.Read);

            _writer = new StreamWriter(stream, new UTF8Encoding(false))
            {
                AutoFlush = false
            };
        }

        public FileInfo File { get; }

        public void AppendLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            _writer.WriteLine(line);
        }

        public void Flush()
        {
            _writer.Flush();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}