using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Rusleo.Utils.Runtime.Math.Interpolations;
using Unity.Mathematics;

namespace Rusleo.Utils.Runtime.Math.Splines
{
    /// <summary>
    /// Удобный "контейнер" для работы с 1D-кривыми по контролл-точкам:
    /// - Catmull-Rom "через точки"
    /// - Hermite с явными касательными
    /// Позволяет семплировать по u∈[0,1] на всём диапазоне.
    /// </summary>
    public sealed class Spline1D
    {
        public float[] Points { get; private set; }
        public float Tau { get; private set; } = 0.5f; // натяжение для Catmull-Rom
        public FadeCurveType Fade { get; private set; } = FadeCurveType.Linear;

        // Для Hermite можно хранить попарно касательные к сегментам (по желанию пользователя).
        // Если не заданы, Hermite для Interp() будет недоступен.
        [CanBeNull] public float[] SegmentTangents { get; private set; } = null;

        public Spline1D(float[] points, float tau = 0.5f, FadeCurveType fade = FadeCurveType.Linear)
        {
            if (points == null || points.Length < 2)
                throw new System.ArgumentException("Spline1D requires ≥2 points");
            Points = points;
            Tau = tau;
            Fade = fade;
        }

        public Spline1D WithTau(float tau)
        {
            Tau = tau;
            return this;
        }

        public Spline1D WithFade(FadeCurveType fade)
        {
            Fade = fade;
            return this;
        }

        public Spline1D WithTangents(float[] tangents)
        {
            SegmentTangents = tangents;
            return this;
        }

        /// <summary>
        /// Семплировать кривую в глобальном u∈[0,1] по выбранному виду интерполяции.
        /// Для CubicHermite требуютcя касательные к текущему сегменту (2 значения).
        /// </summary>
        public float Evaluate(float u, InterpolationKind kind)
        {
            u = math.clamp(u, 0.0f, 1.0f);
            var points = Points;
            int n = points.Length;

            if (kind == InterpolationKind.Lerp || n == 2)
            {
                // Простая линейная на всём диапазоне
                return Interpolation.Lerp(points[0], points[n - 1], u, Fade);
            }

            // Вычисляем сегмент и локальный параметр
            float t = u * (n - 1);
            int i = math.min((int)math.floor(t), n - 2);
            float localT = Interpolation.ApplyFade(t - i, Fade);

            switch (kind)
            {
                case InterpolationKind.CatmullRom:
                {
                    float p0 = points[math.max(i - 1, 0)];
                    float p1 = points[i];
                    float p2 = points[i + 1];
                    float p3 = points[math.min(i + 2, n - 1)];
                    return Interpolation.CatmullRom(p0, p1, p2, p3, localT, Tau);
                }

                case InterpolationKind.CubicHermite:
                {
                    if (SegmentTangents == null || SegmentTangents.Length < (n - 1) * 2)
                        throw new System.InvalidOperationException(
                            "CubicHermite requires SegmentTangents per segment: [m0_i, m1_i] for each i.");
                    // Для сегмента i берём касательные m0_i, m1_i
                    int baseIdx = i * 2;
                    float m0 = SegmentTangents[baseIdx + 0];
                    float m1 = SegmentTangents[baseIdx + 1];
                    float p0 = points[i];
                    float p1 = points[i + 1];
                    return Interpolation.CubicHermite(p0, p1, m0, m1, localT);
                }

                default:
                    return Interpolation.Lerp(points[0], points[n - 1], u, Fade);
            }
        }

        /// <summary>
        /// Быстрый семплинг Catmull-Rom по всему диапазону без повторных ветвлений.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EvaluateCatmull(float u) => Evaluate(u, InterpolationKind.CatmullRom);

        /// <summary>
        /// Быстрый семплинг линейно по всему диапазону.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float EvaluateLerp(float u) => Evaluate(u, InterpolationKind.Lerp);
    }
}