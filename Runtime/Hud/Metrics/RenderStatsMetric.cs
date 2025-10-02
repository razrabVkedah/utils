using System;
using Unity.Profiling;
using UnityEngine;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class RenderStatsMetric : IMetricsProvider, IDisposable
    {
        public string Name => "Render";
        public bool Enabled { get; set; } = true;

        ProfilerRecorder _drawCalls;
        ProfilerRecorder _batches;
        ProfilerRecorder _setPass;
        ProfilerRecorder _tris;
        ProfilerRecorder _verts;

        // EMA чтобы не прыгало
        const float A = 0.2f;
        float _emaDraw, _emaBatch, _emaSP, _emaTris, _emaVerts;

        public RenderStatsMetric()
        {
#if UNITY_2020_2_OR_NEWER
            try
            {
                _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
                _batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
                _setPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
                _tris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
                _verts = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
            }
            catch
            {
                Enabled = false;
            }
#else
            Enabled = false;
#endif
        }

        public void Update(float dt)
        {
#if UNITY_2020_2_OR_NEWER
            if (!Enabled) return;

            UpdateEma(ref _emaDraw, _drawCalls.LastValue);
            UpdateEma(ref _emaBatch, _batches.LastValue);
            UpdateEma(ref _emaSP, _setPass.LastValue);
            UpdateEma(ref _emaTris, _tris.LastValue);
            UpdateEma(ref _emaVerts, _verts.LastValue);
#endif
        }

        static void UpdateEma(ref float ema, long val)
        {
            var f = (float)val;
            ema = ema <= 0f ? f : Mathf.Lerp(ema, f, A);
        }

        public void Emit(IStringBuilderTarget sb)
        {
#if UNITY_2020_2_OR_NEWER
            if (!Enabled) return;
            sb.Append("DR: ");
            sb.Append(((int)_emaDraw).ToString());
            sb.Append(" | BT: ");
            sb.Append(((int)_emaBatch).ToString());
            sb.Append(" | SP: ");
            sb.Append(((int)_emaSP).ToString());
            sb.Append(" | Tris: ");
            sb.Append(((int)_emaTris).ToString());
            sb.Append(" | Verts: ");
            sb.Append(((int)_emaVerts).ToString());
#endif
        }

        public void Dispose()
        {
#if UNITY_2020_2_OR_NEWER
            _drawCalls.Dispose();
            _batches.Dispose();
            _setPass.Dispose();
            _tris.Dispose();
            _verts.Dispose();
#endif
        }
    }
}