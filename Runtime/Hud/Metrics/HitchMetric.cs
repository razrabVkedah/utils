using System.Collections.Generic;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class HitchMetric : IMetricsProvider
    {
        public string Name => "Hitch";
        public bool Enabled { get; set; } = true;

        // окно последних N секунд
        const float WindowSec = 20f;
        const float TargetFps = 60f; // можно подставлять реальный refresh
        const float HitchMs = 1000f / TargetFps * 2.0f; // >2x от целевого кадра — считаем хитчем

        readonly Queue<(float t, float ms)> _samples = new();
        int _hitchesCount;
        float _maxMs;

        public void Update(float dt)
        {
            float now = Time.unscaledTime;
            float ms = dt * 1000f;

            _samples.Enqueue((now, ms));
            _maxMs = Mathf.Max(_maxMs, ms);
            if (ms > HitchMs) _hitchesCount++;

            while (_samples.Count > 0 && now - _samples.Peek().t > WindowSec)
            {
                var old = _samples.Dequeue();
                if (old.ms > HitchMs) _hitchesCount--;
                if (old.ms >= _maxMs) // пересчёт max
                {
                    _maxMs = 0f;
                    foreach (var s in _samples)
                        if (s.ms > _maxMs)
                            _maxMs = s.ms;
                }
            }
        }

        public void Emit(IStringBuilderTarget sb)
        {
            // Пример: H: 3/20s | Max: 41.7ms
            sb.Append("H: ");
            sb.Append(_hitchesCount.ToString());
            sb.Append("/");
            sb.Append(((int)WindowSec).ToString());
            sb.Append("s | Max: ");
            sb.Append(_maxMs.ToString("0.0"));
            sb.Append(" ms");
        }
    }
}