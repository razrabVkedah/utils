using Rusleo.Utils.Editor.TimeTracking.Interfaces;
using UnityEditor;

namespace Rusleo.Utils.Editor.TimeTracking.Services.Probes
{
    public sealed class InputActivityProbeV2 : IInputActivityProbe
    {
        private readonly double _afkSeconds;

        public InputActivityProbeV2(double afkSeconds)
        {
            _afkSeconds = afkSeconds <= 1.0 ? 120.0 : afkSeconds;
        }

        public bool IsAfk
        {
            get
            {
                var now = EditorApplication.timeSinceStartup;
                var last = EditorActivityHub.LastActivityTime;
                return (now - last) >= _afkSeconds;
            }
        }
    }
}