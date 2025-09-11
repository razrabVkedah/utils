#nullable enable
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using Rusleo.Utils.Runtime.Math.Interpolations; // твой Interpolation + FadeCurveType

namespace Rusleo.Utils.Runtime.Math.Splines
{
    public sealed class Spline2D
    {
        // --- Публичное API: Vector2 ---
        public Vector2[] Points { get; private set; }
        public bool Loop { get; private set; }
        public float Tau { get; private set; } = 0.5f;
        public FadeCurveType Fade { get; private set; } = FadeCurveType.Linear;
        public bool Centripetal { get; private set; } = false;
        public Vector2[]? SegmentTangents { get; private set; } = null; // [m0_i, m1_i] на сегмент

        // --- Внутреннее хранение: float2 / mathematics ---
        private float2[] _pts;
        private float2[]? _segTangents;

        // Для равномерной скорости
        private float[]? _cumLength; // кумулятивные длины дискретизации (u in [0,1])
        private int _samplesPerSeg = 20;

        public Spline2D(Vector2[] points, bool loop = false, float tau = 0.5f,
                        FadeCurveType fade = FadeCurveType.Linear, bool centripetal = false)
        {
            if (points == null || points.Length < 2)
                throw new System.ArgumentException("Spline2D requires ≥2 points");

            Points = points;
            _pts   = ToFloat2(points);
            Loop   = loop;
            Tau    = tau;
            Fade   = fade;
            Centripetal = centripetal;
        }

        // --- Fluent setters (синхронизация UnityEngine <-> mathematics) ---

        public Spline2D WithLoop(bool loop) { Loop = loop; _cumLength = null; return this; }
        public Spline2D WithTau(float tau) { Tau = tau; return this; }
        public Spline2D WithFade(FadeCurveType fade) { Fade = fade; return this; }
        public Spline2D WithCentripetal(bool on) { Centripetal = on; return this; }

        public Spline2D WithTangents(Vector2[] tangents)
        {
            SegmentTangents = tangents;
            _segTangents    = ToFloat2(tangents);
            return this;
        }

        public Spline2D WithSamplingDensity(int samplesPerSegment)
        {
            _samplesPerSeg = math.max(4, samplesPerSegment);
            _cumLength = null;
            return this;
        }

        // ---------- Public API ----------

        /// Семпл по u∈[0,1] (равномерный по индексам, не по дуге).
        public Vector2 Evaluate(float u, InterpolationKind kind)
        {
            u = saturate(u);

            int n = _pts.Length;
            if (kind == InterpolationKind.Lerp || n == 2)
            {
                float t = ApplyFade(u, Fade);
                return (Vector2)lerp(_pts[0], _pts[n - 1], t);
            }

            int segCount = Loop ? n : (n - 1);
            float tf = u * segCount;
            int i = clamp((int)floor(tf), 0, segCount - 1);
            float localT = ApplyFade(tf - i, Fade);

            float2 p0 = GetPoint(i - 1);
            float2 p1 = GetPoint(i + 0);
            float2 p2 = GetPoint(i + 1);
            float2 p3 = GetPoint(i + 2);

            switch (kind)
            {
                case InterpolationKind.CatmullRom:
                    return (Vector2)CatmullRomVec(p0, p1, p2, p3, localT, Tau, Centripetal);

                case InterpolationKind.CubicHermite:
                    {
                        if (_segTangents == null || _segTangents.Length < segCount * 2)
                            throw new System.InvalidOperationException("CubicHermite requires SegmentTangents per segment: [m0_i, m1_i].");
                        int baseIdx = i * 2;
                        float2 m0 = _segTangents[baseIdx + 0];
                        float2 m1 = _segTangents[baseIdx + 1];
                        return (Vector2)CubicHermiteVec(p1, p2, m0, m1, localT);
                    }

                default:
                    return (Vector2)lerp(_pts[0], _pts[n - 1], localT);
            }
        }

        /// Семпл по длине дуги (примерно равномерная скорость).
        public Vector2 EvaluateByArcLength(float u, InterpolationKind kind)
        {
            if (_cumLength == null) BuildArcTable(kind);
            float target = saturate(u) * _cumLength![_cumLength!.Length - 1];

            // бинпоиск по таблице
            int lo = 0, hi = _cumLength!.Length - 1;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (_cumLength![mid] < target) lo = mid + 1; else hi = mid;
            }
            int idx = clamp(lo - 1, 0, _cumLength!.Length - 2);
            float l0 = _cumLength![idx];
            float l1 = _cumLength![idx + 1];
            float s = (l1 > l0) ? (target - l0) / (l1 - l0) : 0f;

            int segCount = Loop ? _pts.Length : (_pts.Length - 1);
            int seg = idx / _samplesPerSeg;
            float tLocal = ((idx % _samplesPerSeg) + s) / _samplesPerSeg;

            float uApprox = (seg + tLocal) / segCount;
            return Evaluate(uApprox, kind);
        }

        // ---------- Math kernels ----------

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float2 GetPoint(int i)
        {
            int n = _pts.Length;
            if (Loop)
            {
                int idx = ((i % n) + n) % n;
                return _pts[idx];
            }
            if (i < 0) return _pts[0];
            if (i >= n) return _pts[n - 1];
            return _pts[i];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 CubicHermiteVec(in float2 p0, in float2 p1, in float2 m0, in float2 m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 =  2f * t3 - 3f * t2 + 1f;
            float h10 =       t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 =       t3 -      t2;
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        /// Catmull-Rom; если centripetal=true, используем chord-length^(0.5) параметризацию.
        private static float2 CatmullRomVec(in float2 p0, in float2 p1, in float2 p2, in float2 p3, float t, float tau, bool centripetal)
        {
            if (!centripetal)
            {
                float2 m1 = tau * (p2 - p0);
                float2 m2 = tau * (p3 - p1);
                return CubicHermiteVec(p1, p2, m1, m2, t);
            }

            float a0 = pow(length(p1 - p0), 0.5f);
            float a1 = pow(length(p2 - p1), 0.5f);
            float a2 = pow(length(p3 - p2), 0.5f);
            a0 = max(a0, 1e-6f); a1 = max(a1, 1e-6f); a2 = max(a2, 1e-6f);

            float2 m1c = (p2 - p1) * (1f + a0 / (a0 + a1)) * 0.5f + (p1 - p0) * (a1 / (a0 + a1)) * 0.5f;
            float2 m2c = (p2 - p1) * (a1 / (a1 + a2)) * 0.5f + (p3 - p2) * (1f + a2 / (a1 + a2)) * 0.5f;

            return CubicHermiteVec(p1, p2, m1c, m2c, t);
        }

        private void BuildArcTable(InterpolationKind kind)
        {
            int segCount = Loop ? _pts.Length : (_pts.Length - 1);
            int totalSamples = max(segCount * _samplesPerSeg, 2);

            _cumLength = new float[totalSamples + 1];
            Vector2 prev = Evaluate(0f, kind);
            _cumLength[0] = 0f;
            for (int i = 1; i <= totalSamples; i++)
            {
                float u = (float)i / totalSamples;
                Vector2 curr = Evaluate(u, kind);
                _cumLength[i] = _cumLength[i - 1] + Vector2.Distance(prev, curr);
                prev = curr;
            }
        }

        // ---------- Conversions ----------

        private static float2[] ToFloat2(Vector2[] src)
        {
            var r = new float2[src.Length];
            for (int i = 0; i < src.Length; i++) r[i] = new float2(src[i].x, src[i].y);
            return r;
        }

        private static float2[] ToFloat2(Vector2[]? src, int expectedEven = -1)
        {
            if (src == null) return null!;
            if (expectedEven > 0 && (src.Length % 2 != 0))
                throw new System.ArgumentException("Tangents array length must be even.");
            return ToFloat2(src);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ApplyFade(float t, FadeCurveType fade) => Interpolation.ApplyFade(t, fade);
    }
}
