namespace Rusleo.Utils.Editor.TimeTracking.Core
{
    public readonly struct EditorFlags
    {
        public EditorFlags(bool isPlayMode, bool isAfk, bool? isFocused, bool? isCompiling)
        {
            IsPlayMode = isPlayMode;
            IsAfk = isAfk;
            IsFocused = isFocused;
            IsCompiling = isCompiling;
        }

        public bool IsPlayMode { get; }
        public bool IsAfk { get; }
        public bool? IsFocused { get; }
        public bool? IsCompiling { get; }
    }
}