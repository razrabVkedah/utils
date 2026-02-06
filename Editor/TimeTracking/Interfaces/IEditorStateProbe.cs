namespace Rusleo.Utils.Editor.TimeTracking.Interfaces
{
    public interface IEditorStateProbe
    {
        /// <summary>
        /// PlayMode: 0/1 в логике, но наружу даём bool.
        /// </summary>
        bool IsPlayMode { get; }

        /// <summary>
        /// Editor window focused? может быть недоступно — тогда null.
        /// </summary>
        bool? IsFocused { get; }

        /// <summary>
        /// Идёт компиляция? может быть недоступно — тогда null.
        /// </summary>
        bool? IsCompiling { get; }
    }
}