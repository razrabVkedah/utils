namespace Rusleo.Utils.Editor.TimeTracking.Core
{
    /// <summary>
    /// Unix timestamp UTC.
    /// В реализации выбрать секунды ИЛИ миллисекунды и придерживаться одного формата.
    /// </summary>
    public readonly struct UnixTime
    {
        public UnixTime(long value)
        {
            Value = value;
        }

        public long Value { get; }
        public override string ToString() => Value.ToString();
    }
}