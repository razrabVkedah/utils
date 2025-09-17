using Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer.Internal;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.Windows.PlayerPrefsVisualizer
{
    internal sealed class PlayerPrefsPromptWindow : EditorWindow
    {
        private string _key;
        private PrefType _type;
        private string _value;
        private bool _ok;

        public static bool ShowModal(string title, ref string key, ref PrefType type, ref string value)
        {
            var w = CreateInstance<PlayerPrefsPromptWindow>();
            w.titleContent = new GUIContent(title);
            w.position = new Rect(Screen.width / 2f - 200, Screen.height / 2f - 80, 400, 160);
            w._key = key;
            w._type = type;
            w._value = value;
            w.ShowModalUtility();
            key = w._key;
            type = w._type;
            value = w._value;
            return w._ok;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            _key = EditorGUILayout.TextField("Key", _key);
            _type = (PrefType)EditorGUILayout.EnumPopup("Type", _type);
            _value = EditorGUILayout.TextField("Value", _value);
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(90)))
                {
                    _ok = false;
                    Close();
                }

                if (GUILayout.Button("OK", GUILayout.Width(90)))
                {
                    _ok = true;
                    Close();
                }
            }

            EditorGUILayout.Space();
        }
    }
}