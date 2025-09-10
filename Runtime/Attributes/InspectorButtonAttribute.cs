using System;

namespace Rusleo.Utils.Runtime.Attributes
{
    public enum ButtonMode
    {
        Always, // всегда активна
        EditorOnly, // только в редакторе (в Play выключена)
        PlayModeOnly // только в Play
    }

    public enum ButtonSize
    {
        Small,
        Normal,
        Large
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class InspectorButtonAttribute : Attribute
    {
        public readonly string Label;
        public readonly ButtonMode Mode;
        public readonly int Order;
        public readonly string Confirm; // null = без подтверждения
        public readonly ButtonSize Size; // размер кнопки
        public readonly string Icon; // Editor icon name (например, "d_PlayButton" / "console.infoicon")
        public readonly string Tooltip; // всплывающая подсказка

        public InspectorButtonAttribute(
            string label = null,
            ButtonMode mode = ButtonMode.Always,
            int order = 0,
            string confirm = null,
            ButtonSize size = ButtonSize.Normal,
            string icon = null,
            string tooltip = null)
        {
            Label = label;
            Mode = mode;
            Order = order;
            Confirm = confirm;
            Size = size;
            Icon = icon;
            Tooltip = tooltip;
        }
    }

    /// <summary>Показывать кнопку только если булево условие истинно.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class VisibleIfAttribute : Attribute
    {
        public readonly string MemberName;
        public VisibleIfAttribute(string memberName) => MemberName = memberName;
    }

    /// <summary>Включать/выключать кнопку (disabled) по булевому условию.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class EnableIfAttribute : Attribute
    {
        public readonly string MemberName;
        public EnableIfAttribute(string memberName) => MemberName = memberName;
    }

    /// <summary>
    /// Повесь на класс, чтобы отключить глобальный редактор кнопок для него.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class DisableGlobalButtonEditorAttribute : Attribute
    {
    }
}