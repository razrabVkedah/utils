namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface IEventSerializer
    {
        /// <summary>
        /// Одна строка JSONL без завершающего \n.
        /// </summary>
        string SerializeLine(ITrackerEvent ev);
    }
}