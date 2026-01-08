using System;
using System.Linq;
using Rusleo.Utils.Runtime.Hud.Metrics;
using Rusleo.Utils.Runtime.Logging;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = Rusleo.Utils.Runtime.Logging.Logger;

namespace Rusleo.Utils.Runtime.Hud
{
    public sealed class FpsHudBootstrap : MonoBehaviour
    {
        [SerializeField] private HudSettings settings;
        [SerializeField] private float updatePeriod = 0.2f;
        private Logger _logger;

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
            _logger = new Logger("Rusleo HUD", gameObject);
            if (!settings)
            {
                settings = Resources.LoadAll<HudSettings>("").FirstOrDefault(r => r.name == "DefaultHudSettings");
                _logger.Debug(settings == null ? "Failed to load DefaultHudSettings." : "Loaded DefaultHudSettings.");
            }

            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<HudSettings>();
                settings.name = "RuntimeHudSettings";
                _logger.Debug("Created RuntimeHudSettings.");
            }

            HudService.Instance.Configure(settings, updatePeriod);

            if (settings.FPSMetrics) TryRegisterMetric<FpsMetric>();
            if (settings.FrameMetrics) TryRegisterMetric<FrameTimeMetric>();
            if (settings.MemoryMetrics) TryRegisterMetric<MemoryMetric>();
            if (settings.GcMetrics) TryRegisterMetric<GcMetric>();
            if (settings.CpuMetrics) TryRegisterMetric<CpuUsageMetric>();
            if (settings.HitchMetrics) TryRegisterMetric<HitchMetric>();
            if (settings.RenderStatsMetrics) TryRegisterMetric<RenderStatsMetric>();

            _renderer = gameObject.AddComponent<HudOverlayRenderer>();
        }

        private void TryRegisterMetric<T>(string metricName = null) where T : IMetricsProvider, new()
        {
            try
            {
                HudService.Instance.Register(new T());
                _logger.Info($"Registered Metric: {metricName ?? typeof(T).Name}");
            }
            catch (Exception e)
            {
                _logger.Error("Failed to register FPS Metrics", e);
            }
        }

        private void Update()
        {
            HudService.Instance.Tick(Time.unscaledDeltaTime);
        }
    }
}