using System;
using System.Diagnostics;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class CpuUsageMetric : IMetricsProvider
    {
        public string Name => "CPU";
        public bool Enabled { get; set; } = true;

        private TimeSpan _prevCpu;
        private double _prevTime;
        private readonly int _cores;
        private float _ema;
        private const float A = 0.2f;
        private readonly Process _proc;

        public CpuUsageMetric()
        {
            _proc = Process.GetCurrentProcess();
            _cores = SystemInfo.processorCount > 0 ? SystemInfo.processorCount : 1;
            _prevCpu = _proc.TotalProcessorTime;
            _prevTime = Time.realtimeSinceStartupAsDouble;
        }

        public void Update(float dt)
        {
            try
            {
                var nowCpu = _proc.TotalProcessorTime;
                var nowTime = Time.realtimeSinceStartupAsDouble;

                var cpuDelta = (nowCpu - _prevCpu).TotalSeconds;
                var timeDelta = nowTime - _prevTime;
                if (timeDelta > 0)
                {
                    var load = (float)(cpuDelta / timeDelta * 100.0 / _cores);
                    load = Mathf.Clamp(load, 0f, 100f);
                    _ema = _ema <= 0f ? load : Mathf.Lerp(_ema, load, A);
                }

                _prevCpu = nowCpu;
                _prevTime = nowTime;
            }
            catch
            {
                // На некоторых платформах Process может быть ограничен
                Enabled = false;
            }
        }

        public void Emit(IStringBuilderTarget sb)
        {
            sb.Append("CPU: ");
            sb.Append(_ema.ToString("0.0"));
            sb.Append("%");
        }
    }
}