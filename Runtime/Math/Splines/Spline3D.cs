// MIT License
// Rusleo.Utils — Interpolation Module (mathematics-backed)
// Runtime/Mathx/Spline3D.cs

#nullable enable
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;
using Rusleo.Utils.Runtime.Math.Interpolations;

namespace Rusleo.Utils.Runtime.Math.Splines
{
    public sealed class Spline3D
    {
        // Публичное API: Vector3
        public Vector3[] Points { get; private set; }
        public bool Loop { get; private set; }
        public float Tau { get; private set; } = 0.5f;
        public FadeCurveType Fade { get; private set; } = FadeCurveType.Linear;
        public bool Centripetal { get; private set; } = false;
        public Vector3[]? SegmentTangents { get; private set; } = null;

        // Внутреннее хранение: float3
        private float3[] _pts;
        private float3[]? _segTangents;

        private float[]? _cumLength;
        private int _samplesPerSeg = 20;

        public Spline3D(Vector3[] points, bool loop = false, float tau = 0.5f,
                        FadeCurveType fade = FadeCurveType.Linear, bool centripetal = false)
        {
            if (points == null || points.Length < 2)
                throw new System.ArgumentException("Spline3D requires ≥2 points");

            Points = points;
            _pts   = ToFloat3(points);
            Loop   = loop;
            Tau    = tau;
            Fade   = fade;
            Centripetal = centripetal;
        }

        public Spline3D WithLoop(bool loop) { Loop = loop; _cumLength = null; return this; }
        public Spline3D WithTau(float tau) { Tau = tau; return this; }
        public Spline3D WithFade(FadeCurveType fade) { Fade = fade; return this; }
        public Spline3D WithCentripetal(bool on) { Centripetal = on; return this; }

        public Spline3D WithTangents(Vector3[] tangents)
        {
            SegmentTangents = tangents;
            _segTangents    = ToFloat3(tangents);
            return this;
        }

        public Spline3D WithSamplingDensity(int samplesPerSegment)
        {
            _samplesPerSeg = max(4, samplesPerSegment);
            _cumLength = null;
            return this;
        }

        public Vector3 Evaluate(float u, InterpolationKind kind)
        {
            u = saturate(u);
            int n = _pts.Length;

            if (kind == InterpolationKind.Lerp || n == 2)
            {
                float t = ApplyFade(u, Fade);
                return (Vector3)lerp(_pts[0], _pts[n - 1], t);
            }

            int segCount = Loop ? n : (n - 1);
            float tf = u * segCount;
            int i = clamp((int)floor(tf), 0, segCount - 1);
            float localT = ApplyFade(tf - i, Fade);

            float3 p0 = GetPoint(i - 1);
            float3 p1 = GetPoint(i + 0);
            float3 p2 = GetPoint(i + 1);
            float3 p3 = GetPoint(i + 2);

            switch (kind)
            {
                case InterpolationKind.CatmullRom:
                    return (Vector3)CatmullRomVec(p0, p1, p2, p3, localT, Tau, Centripetal);

                case InterpolationKind.CubicHermite:
                    {
                        if (_segTangents == null || _segTangents.Length < segCount * 2)
                            throw new System.InvalidOperationException("CubicHermite requires SegmentTangents per segment: [m0_i, m1_i].");
                        int baseIdx = i * 2;
                        float3 m0 = _segTangents[baseIdx + 0];
                        float3 m1 = _segTangents[baseIdx + 1];
                        return (Vector3)CubicHermiteVec(p1, p2, m0, m1, localT);
                    }

                default:
                    return (Vector3)lerp(_pts[0], _pts[n - 1], localT);
            }
        }

        public Vector3 EvaluateByArcLength(float u, InterpolationKind kind)
        {
            if (_cumLength == null) BuildArcTable(kind);
            float target = saturate(u) * _cumLength![_cumLength!.Length - 1];

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float3 GetPoint(int i)
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
        private static float3 CubicHermiteVec(in float3 p0, in float3 p1, in float3 m0, in float3 m1, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 =  2f * t3 - 3f * t2 + 1f;
            float h10 =       t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 =       t3 -      t2;
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        private static float3 CatmullRomVec(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, float tau, bool centripetal)
        {
            if (!centripetal)
            {
                float3 m1 = tau * (p2 - p0);
                float3 m2 = tau * (p3 - p1);
                return CubicHermiteVec(p1, p2, m1, m2, t);
            }

            float a0 = pow(length(p1 - p0), 0.5f);
            float a1 = pow(length(p2 - p1), 0.5f);
            float a2 = pow(length(p3 - p2), 0.5f);
            a0 = max(a0, 1e-6f); a1 = max(a1, 1e-6f); a2 = max(a2, 1e-6f);

            float3 m1c = (p2 - p1) * (1f + a0 / (a0 + a1)) * 0.5f + (p1 - p0) * (a1 / (a0 + a1)) * 0.5f;
            float3 m2c = (p2 - p1) * (a1 / (a1 + a2)) * 0.5f + (p3 - p2) * (1f + a2 / (a1 + a2)) * 0.5f;

            return CubicHermiteVec(p1, p2, m1c, m2c, t);
        }

        private void BuildArcTable(InterpolationKind kind)
        {
            int segCount = Loop ? _pts.Length : (_pts.Length - 1);
            int totalSamples = max(segCount * _samplesPerSeg, 2);

            _cumLength = new float[totalSamples + 1];
            Vector3 prev = Evaluate(0f, kind);
            _cumLength[0] = 0f;
            for (int i = 1; i <= totalSamples; i++)
            {
                float u = (float)i / totalSamples;
                Vector3 curr = Evaluate(u, kind);
                _cumLength[i] = _cumLength[i - 1] + Vector3.Distance(prev, curr);
                prev = curr;
            }
        }

        private static float3[] ToFloat3(Vector3[] src)
        {
            var r = new float3[src.Length];
            for (int i = 0; i < src.Length; i++) r[i] = new float3(src[i].x, src[i].y, src[i].z);
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float ApplyFade(float t, FadeCurveType fade) => Interpolation.ApplyFade(t, fade);
    }
}
