using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;
using UnityEngine;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Probes
{
    public sealed class InputActivityProbe : IInputActivityProbe
    {
        private readonly double _afkSeconds;
        private double _lastActivityTime;
        private Vector2 _lastMousePos;

        public InputActivityProbe(double afkSeconds = 120.0)
        {
            _afkSeconds = afkSeconds <= 1.0 ? 120.0 : afkSeconds;
            _lastActivityTime = EditorApplication.timeSinceStartup;
            _lastMousePos = GetMousePositionSafe();
        }

        public bool IsAfk
        {
            get
            {
                var now = EditorApplication.timeSinceStartup;

                if (HasAnyActivity())
                    _lastActivityTime = now;

                return (now - _lastActivityTime) >= _afkSeconds;
            }
        }

        private bool HasAnyActivity()
        {
            var mouse = GetMousePositionSafe();
            var moved = (mouse - _lastMousePos).sqrMagnitude > 0.01f;
            _lastMousePos = mouse;

            if (moved)
                return true;

            // Клавиатуру в Editor надёжно глобально не снять без костылей, поэтому считаем мышь основной метрикой.
            // Если нужно — расширим через global event hook (EditorApplication.update + Event.current из окон).
            return false;
        }

        private static Vector2 GetMousePositionSafe()
        {
            // В редакторе это позиция относительно текущего GUI события, может быть (0,0) если нет событий.
            // Но нам достаточно “двигается/не двигается”.
            return GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : Vector2.zero);
        }
    }
}