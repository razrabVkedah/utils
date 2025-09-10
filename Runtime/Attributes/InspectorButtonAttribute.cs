using System;

namespace Rusleo.Utils.Runtime.Attributes
{
    public enum ButtonMode
    {
        Always, // всегда активна
        EditorOnly, // только в редакторе (в Play выключена)
        PlayModeOnly // только в Play
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InspectorButtonAttribute : Attribute
    {
        public readonly string Label;
        public readonly ButtonMode Mode;
        public readonly int Order;
        public readonly string Confirm; // null = без подтверждения

        public InspectorButtonAttribute(string label = null, ButtonMode mode = ButtonMode.Always, int order = 0,
            string confirm = null)
        {
            Label = label;
            Mode = mode;
            Order = order;
            Confirm = confirm;
        }
    }

    /// <summary>
    /// Повесь на класс, чтобы отключить глобальный редактор кнопок для него.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class DisableGlobalButtonEditorAttribute : Attribute
    {
    }
}