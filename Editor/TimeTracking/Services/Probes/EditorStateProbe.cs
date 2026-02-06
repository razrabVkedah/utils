using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;
using UnityEditorInternal;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Probes
{
    public sealed class EditorStateProbe : IEditorStateProbe
    {
        public bool IsPlayMode => EditorApplication.isPlaying;

        public bool? IsFocused => InternalEditorUtility.isApplicationActive;

        public bool? IsCompiling => EditorApplication.isCompiling;
    }
}