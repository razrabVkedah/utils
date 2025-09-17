using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class MemoryMetric : IMetricsProvider
    {
        public string Name => "Memory";
        public bool Enabled { get; set; } = true;

        private float _emaMonoMb, _emaUnityMb;
        private const float A = 0.2f;

        public void Update(float dt)
        {
            float monoMb = (float)(System.GC.GetTotalMemory(false) / (1024.0 * 1024.0));
            _emaMonoMb = _emaMonoMb <= 0f ? monoMb : Mathf.Lerp(_emaMonoMb, monoMb, A);

            // Unity allocated memory
            float unityMb = (float)(UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024.0 * 1024.0));
            _emaUnityMb = _emaUnityMb <= 0f ? unityMb : Mathf.Lerp(_emaUnityMb, unityMb, A);
        }

        public void Emit(IStringBuilderTarget sb)
        {
            sb.Append("Mem: Mono ");
            sb.Append(_emaMonoMb.ToString("0"));
            sb.Append(" MB | Unity ");
            sb.Append(_emaUnityMb.ToString("0"));
            sb.Append(" MB");
        }
    }
}