namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface IInputActivityProbe
    {
        /// <summary>
        /// Была ли активность ввода за период AFK-порога.
        /// </summary>
        bool IsAfk { get; }
    }
}