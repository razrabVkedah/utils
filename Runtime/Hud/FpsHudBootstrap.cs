using Rusleo.Utils.Runtime.Hud.Metrics;
using Rusleo.Utils.Runtime.Logging;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud
{
    public sealed class FpsHudBootstrap : MonoBehaviour
    {
        [SerializeField] private HudTheme theme;
        [SerializeField] private float updatePeriod = 0.2f;

        private HudOverlayRenderer _renderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            Log.Debug("Rusleo.Utils.Runtime.HudBootstrap AutoInit");

            if (FindObjectOfType<FpsHudBootstrap>() != null) return;

            var go = new GameObject("Rusleo.FpsHud");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<FpsHudBootstrap>();
        }

        private void Awake()
        {
            if (!theme)
            {
                theme = Resources.Load<HudTheme>("DefaultHudTheme");
                Log.Debug(theme == null ? "Failed to load HudTheme." : "Loaded DefaultHudTheme.", null);
            }

            if (!theme)
            {
                theme = ScriptableObject.CreateInstance<HudTheme>();
                theme.name = "RuntimeHudTheme";
                Log.Debug("Created RuntimeHudTheme.", null);
            }

            HudService.Instance.Configure(theme, updatePeriod);

            HudService.Instance.Register(new FpsMetric());
            HudService.Instance.Register(new FrameTimeMetric());
            HudService.Instance.Register(new MemoryMetric());
            HudService.Instance.Register(new GcMetric());
            HudService.Instance.Register(new CpuUsageMetric());

            _renderer = gameObject.AddComponent<HudOverlayRenderer>();
        }

        private void Update()
        {
            HudService.Instance.Tick(Time.unscaledDeltaTime);
        }
    }
}