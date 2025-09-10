using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.InspectorButtons
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    public class GlobalButtonEditorForMB : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GlobalButtonEditor.DrawButtons(targets, target.GetType());
        }
    }
}