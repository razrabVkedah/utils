namespace Rusleo.Utils.Runtime.Logging
{
    public interface ILogSink
    {
        // Быстрый неблокирующий приём (по возможности)
        void Emit(in LogEvent e);
        // Освобождение ресурсов / флеш
        void Flush();
        void Dispose();
    }
}