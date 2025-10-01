using System.Collections.Generic;
using System.Linq;

namespace Rusleo.Utils.Runtime.Hud
{
    public sealed class HudService
    {
        public static HudService Instance { get; } = new();

        private readonly List<IMetricsProvider> _providers = new(8);
        private readonly StringBuilderTarget _sb = new();
        private float _accum;
        private float _updatePeriod = 0.2f; // раз в 200мс обновляем тексты
        private string _cachedText = "";

        public HudSettings Settings { get; private set; }
        
        public static void TrySetVisible(bool visible)
        {
            if (Instance == null || Instance.Settings == null) return;
            Instance.Settings.visible = visible;
        }

        private HudService()
        {
        }

        public void Configure(HudSettings settings, float updatePeriod)
        {
            Settings = settings;
            if (updatePeriod > 0f) _updatePeriod = updatePeriod;
        }

        public void Register(IMetricsProvider provider)
        {
            if (!_providers.Contains(provider))
                _providers.Add(provider);
        }

        public void Unregister(IMetricsProvider provider) => _providers.Remove(provider);

        public void Tick(float dt)
        {
            _accum += dt;
            
            foreach (var p in _providers.Where(p => p.Enabled)) p.Update(dt);

            if (_accum < _updatePeriod) return;

            _accum = 0f;
            _sb.Clear();
            if (Settings && Settings.showHeader)
            {
                _sb.Append(Settings.headerText);
                _sb.Append('\n');
            }

            foreach (var p in _providers.Where(p => p.Enabled))
            {
                p.Emit(_sb);
                _sb.Append('\n');
            }

            _cachedText = _sb.ToString();
        }

        public string GetText() => _cachedText;
    }
}