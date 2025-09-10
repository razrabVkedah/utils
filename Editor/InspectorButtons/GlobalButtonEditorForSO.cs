using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.InspectorButtons
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    public class GlobalButtonEditorForSO : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GlobalButtonEditor.DrawButtons(targets, target.GetType());
        }
    }
}