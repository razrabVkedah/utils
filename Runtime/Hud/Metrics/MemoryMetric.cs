using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class MemoryMetric : IMetricsProvider
    {
        public string Name => "Memory";
        public bool Enabled { get; set; } = true;

        private float _emaMonoMb, _emaUnityMb;
        private const float A = 0.2f;

        float _emaReserved, _emaTemp;
        long _prevAlloc;
        private int _allocPerFrame;

        public void Update(float dt)
        {
            float monoMb  = (float)(System.GC.GetTotalMemory(false) / (1024.0 * 1024.0));
            _emaMonoMb = _emaMonoMb <= 0f ? monoMb : Mathf.Lerp(_emaMonoMb, monoMb, A);

            long unityAlloc = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            float unityMb = unityAlloc / (1024f * 1024f);
            _emaUnityMb = _emaUnityMb <= 0f ? unityMb : Mathf.Lerp(_emaUnityMb, unityMb, A);

            float reservedMb = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / (1024f * 1024f);
            _emaReserved = _emaReserved <= 0f ? reservedMb : Mathf.Lerp(_emaReserved, reservedMb, A);

            float tempMb = UnityEngine.Profiling.Profiler.GetTempAllocatorSize() / (1024f * 1024f);
            _emaTemp = _emaTemp <= 0f ? tempMb : Mathf.Lerp(_emaTemp, tempMb, A);

            // allocs/frame (приблизительно)
            long diff = unityAlloc - _prevAlloc;
            _prevAlloc = unityAlloc;
            _allocPerFrame = Mathf.Max(0, (int)diff); // покажем в байтах или кБ
        }

        public void Emit(IStringBuilderTarget sb)
        {
            sb.Append("Mem: Mono ");
            sb.Append(_emaMonoMb.ToString("0"));
            sb.Append("MB\nUnity ");
            sb.Append(_emaUnityMb.ToString("0"));
            sb.Append("\nMB (Rsv ");
            sb.Append(_emaReserved.ToString("0"));
            sb.Append("MB, Temp ");
            sb.Append(_emaTemp.ToString("0"));
            sb.Append("MB)\nAlloc/frame ");
            sb.Append((_allocPerFrame/1024).ToString());
            sb.Append("KB");
        }
    }
}