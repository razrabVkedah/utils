using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class FrameTimeMetric : IMetricsProvider
    {
        public string Name => "FrameTime";
        public bool Enabled { get; set; } = true;

        private float _cpuMsEma, _gpuMsEma;
        private const float A = 0.1f;
        private UnityEngine.FrameTiming[] _timings = new UnityEngine.FrameTiming[1];

        public void Update(float dt)
        {
            var cpuMs = dt * 1000f;
            _cpuMsEma = _cpuMsEma <= 0f ? cpuMs : Mathf.Lerp(_cpuMsEma, cpuMs, A);

            // GPU via FrameTimingManager (если доступно)
            UnityEngine.FrameTimingManager.CaptureFrameTimings();
            uint got = UnityEngine.FrameTimingManager.GetLatestTimings(1, _timings);
            if (got > 0)
            {
                var t = _timings[0];
                if (t.gpuFrameTime > 0)
                {
                    float gpuMs = (float)t.gpuFrameTime;
                    _gpuMsEma = _gpuMsEma <= 0f ? gpuMs : Mathf.Lerp(_gpuMsEma, gpuMs, A);
                }
            }
        }

        public void Emit(IStringBuilderTarget sb)
        {
            sb.Append("CPU: ");
            sb.Append(_cpuMsEma.ToString("0.0"));
            sb.Append(" ms");
            if (_gpuMsEma > 0.001f)
            {
                sb.Append(" | GPU: ");
                sb.Append(_gpuMsEma.ToString("0.0"));
                sb.Append(" ms");
            }
        }
    }
}