namespace Rusleo.Utils.Runtime.Logging.Formatters
{
    public sealed class MinimalLineFormatter : ILogFormatter
    {
        public string Format(in LogEvent e)
        {
            // Чрезвычайно лаконично: время опускаем, owner короткий, без meta
            return $"[{e.Level}] {(string.IsNullOrEmpty(e.Owner) ? "" : $"{{{e.Owner}}} ")}{e.Message}";
        }
    }
}