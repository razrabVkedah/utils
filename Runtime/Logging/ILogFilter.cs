namespace Rusleo.Utils.Runtime.Logging
{
    public interface ILogFilter
    {
        // true = пропускаем событие дальше
        bool ShouldLog(in LogEvent e);
    }
}