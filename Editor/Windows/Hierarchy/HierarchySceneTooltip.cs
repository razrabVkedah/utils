using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rusleo.Utils.Editor.Windows.Hierarchy
{
    [InitializeOnLoad]
    public static class HierarchySceneTooltip
    {
        static HierarchySceneTooltip()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        private static bool TryGetSceneByInstanceID(int instanceID, out Scene scene)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.IsValid() && s.GetHashCode() == instanceID) // scene handle == instanceID
                {
                    scene = s;
                    return true;
                }
            }

            scene = default;
            return false;
        }

        private static void OnHierarchyGUI(int instanceID, Rect rect)
        {
            // Работает только на строке-заголовке сцены
            if (!TryGetSceneByInstanceID(instanceID, out var scene))
                return;

            // Что показываем/копируем
            string path = string.IsNullOrEmpty(scene.path) ? "(unsaved scene)" : scene.path;

            // Чуть сдвигаем зону ховера, чтобы не зацепить стрелку раскрытия и иконку
            var hoverRect = new Rect(rect.x + 18f, rect.y, rect.width - 18f, rect.height);

            // 1) Невидимый контрол с tooltip (две строки)
            //    Важно: второй параметр конструктора GUIContent — это tooltip.
            var tooltip = $"{path}\nCtrl+C (⌘C) — copy";
            GUI.Label(hoverRect, new GUIContent(string.Empty, tooltip), GUIStyle.none);

            // 2) Копирование по Ctrl/⌘ + C, пока мышь над строкой сцены
            var e = Event.current;
            bool modifier = e.type == EventType.KeyDown && (e.control || e.command);
            if (modifier && e.keyCode == KeyCode.C && hoverRect.Contains(e.mousePosition))
            {
                EditorGUIUtility.systemCopyBuffer = path;
                e.Use();

                // 3) Неблокирующее уведомление прямо в окне Hierarchy
                var wnd = EditorWindow.mouseOverWindow; // как раз Hierarchy под курсором
                if (wnd != null)
                {
                    wnd.ShowNotification(new GUIContent($"Copied scene path"), 0.5f);
                    // уведомление само исчезнет через короткое время
                }
            }
        }
    }
}