using UnityEditor;

namespace Rusleo.Utils.Editor.InspectorButtons
{
    [InitializeOnLoad]
    public static class GlobalButtonsHeaderDrawer
    {
        static GlobalButtonsHeaderDrawer()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedDefaultHeaderGUI;
        }

        private static void OnFinishedDefaultHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor == null) return;

            var type = editor.target.GetType();
            GlobalButtonEditor.DrawButtons(editor.targets, type);
        }
    }
}