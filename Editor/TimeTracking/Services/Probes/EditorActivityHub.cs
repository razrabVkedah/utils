using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Probes
{
    [InitializeOnLoad]
    public static class EditorActivityHub
    {
        private static double _lastActivityTime;

        static EditorActivityHub()
        {
            _lastActivityTime = EditorApplication.timeSinceStartup;

            SceneView.duringSceneGui += OnSceneGui;
            EditorApplication.projectWindowItemOnGUI += OnProjectGui;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGui;
            EditorApplication.update += OnUpdate;
        }

        public static double LastActivityTime => _lastActivityTime;

        public static void MarkActivity()
        {
            _lastActivityTime = EditorApplication.timeSinceStartup;
        }

        private static void OnUpdate()
        {
            // Можно расширить: отслеживать смену фокуса окна/активацию Unity Editor.
            // Пока оставляем как есть.
        }

        private static void OnSceneGui(SceneView view)
        {
            TryMarkFromEvent(Event.current);
        }

        private static void OnProjectGui(string guid, Rect rect)
        {
            TryMarkFromEvent(Event.current);
        }

        private static void OnHierarchyGui(int instanceId, Rect rect)
        {
            TryMarkFromEvent(Event.current);
        }

        private static void TryMarkFromEvent(Event ev)
        {
            if (ev == null)
                return;

            if (!IsActivityEvent(ev))
                return;

            MarkActivity();
        }

        private static bool IsActivityEvent(Event ev)
        {
            switch (ev.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.MouseMove:
                case EventType.MouseDrag:
                case EventType.ScrollWheel:
                case EventType.KeyDown:
                case EventType.KeyUp:
                case EventType.DragUpdated:
                case EventType.DragPerform:
                case EventType.DragExited:
                case EventType.ContextClick:
                    return true;

                default:
                    return false;
            }
        }
    }
}