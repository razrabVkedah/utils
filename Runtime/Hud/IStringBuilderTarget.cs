namespace Rusleo.Utils.Runtime.Hud
{
    public interface IStringBuilderTarget
    {
        void Clear();
        void Append(string s);
        void Append(char c);
        void AppendFormat(string format, params object[] args); // использовать осторожно
        string ToString();
    }
}