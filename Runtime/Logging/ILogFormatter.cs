namespace Rusleo.Utils.Runtime.Logging
{
    public interface ILogFormatter
    {
        // Возвращает готовую строку для "плоских" приёмников (файл, сеть и т.п.)
        string Format(in LogEvent e);
    }
}