using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class FpsMetric : IMetricsProvider
    {
        public string Name => "FPS";
        public bool Enabled { get; set; } = true;

        private float _ema;
        private const float Alpha = 0.1f;

        public void Update(float dt)
        {
            var fps = (dt > 0f) ? (1f / dt) : 0f;
            _ema = _ema <= 0f ? fps : Mathf.Lerp(_ema, fps, Alpha);
        }

        public void Emit(IStringBuilderTarget sb)
        {
            sb.Append("FPS: ");
            sb.Append(((int)_ema).ToString());
        }
    }
}