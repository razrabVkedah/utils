using System.Text;

namespace Rusleo.Utils.Runtime.Hud
{
    public sealed class StringBuilderTarget : IStringBuilderTarget
    {
        private readonly StringBuilder _sb = new(512);

        public void Clear() => _sb.Length = 0;
        public void Append(string s) => _sb.Append(s);
        public void Append(char c) => _sb.Append(c);

        public void AppendFormat(string format, params object[] args) => _sb.AppendFormat(format, args);

        public override string ToString() => _sb.ToString();
    }
}