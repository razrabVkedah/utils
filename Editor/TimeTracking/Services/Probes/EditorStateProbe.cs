using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;
namespace Rusleo.Utils.Editor.TimeTracking.Services.Probes
{
    public sealed class EditorStateProbe : IEditorStateProbe
    {
        public bool IsPlayMode => EditorApplication.isPlaying;

        public bool? IsFocused
        {
            get
            {
                var window = EditorWindow.focusedWindow;
                return window != null;
            }
        }

        public bool? IsCompiling => EditorApplication.isCompiling;
    }
}
