using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Rusleo.Utils.Runtime.Math.Interpolations
{
    /// <summary>
    /// Набор быстрых и детерминированных функций интерполяции и сглаживания.
    /// </summary>
    public static class Interpolation
    {
        // ========== F A D E  C U R V E S ==========

        /// <summary>Применить выбранную fade-кривую к t ∈ [0,1].</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ApplyFade(float t, FadeCurveType curve)
        {
            t = math.clamp(t, 0f, 1f);
            switch (curve)
            {
                case FadeCurveType.Smoothstep:
                    // 3t^2 - 2t^3 : C1 непрерывность, нулевая первая производная на концах
                    return t * t * (3f - 2f * t);

                case FadeCurveType.Smootherstep:
                    // 6t^5 - 15t^4 + 10t^3 : C2 непрерывность, нулевая первая и вторая производные
                    return t * t * t * (t * (6f * t - 15f) + 10f);

                case FadeCurveType.Cosine:
                    // 0.5 - 0.5 cos(πt) : мягкая, но формально C1 на концах не так строга как quintic
                    return 0.5f - 0.5f * math.cos(math.PI * t);

                default:
                    return t; // Linear
            }
        }

        // ========== S C A L A R  1 D ==========

        /// <summary>Линейная интерполяция без сглаживания.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        /// <summary>Линейная интерполяция с fade-кривой.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t, FadeCurveType fade) => Lerp(a, b, ApplyFade(t, fade));

        /// <summary>
        /// Кубическая эрмитова интерполяция (значения p0,p1 и касательные m0,m1 заданы явно).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicHermite(float p0, float p1, float m0, float m1, float t)
        {
            t = math.clamp(t, 0f, 1f);
            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        /// <summary>
        /// Cubic Hermite с fade-кривой (fade применяется к параметру t).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CubicHermite(float p0, float p1, float m0, float m1, float t, FadeCurveType fade)
            => CubicHermite(p0, p1, m0, m1, ApplyFade(t, fade));

        /// <summary>
        /// Catmull-Rom: p1..p2 — интервал, p0/p3 — соседи. tau (обычно 0.5) — натяжение.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CatmullRom(float p0, float p1, float p2, float p3, float t, float tau = 0.5f)
        {
            t = math.clamp(t, 0f, 1f);
            float m1 = tau * (p2 - p0);
            float m2 = tau * (p3 - p1);
            return CubicHermite(p1, p2, m1, m2, t);
        }

        /// <summary>Catmull-Rom с fade-кривой.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CatmullRom(float p0, float p1, float p2, float p3, float t, FadeCurveType fade,
            float tau = 0.5f)
            => CatmullRom(p0, p1, p2, p3, ApplyFade(t, fade), tau);

        /// <summary>
        /// Универсальный вход для простых случаев: Lerp / (Hermite с явными касательными) / Catmull-Rom.
        /// Для CubicHermite обязателен массив tangents длины ≥2 (m0,m1 для участка).
        /// Для CatmullRom обязателен массив neighbors длины ≥4 (p0..p3).
        /// </summary>
        public static float Interp1D(
            InterpolationKind kind,
            float t,
            FadeCurveType fade = FadeCurveType.Linear,
            float a = 0, float b = 0,
            float[]? tangents = null,
            float[]? neighbors = null,
            float catmullTau = 0.5f)
        {
            switch (kind)
            {
                case InterpolationKind.Lerp:
                    return Lerp(a, b, t, fade);

                case InterpolationKind.CubicHermite:
                {
                    if (tangents == null || tangents.Length < 2)
                        throw new System.ArgumentException("CubicHermite requires tangents[2]: m0,m1");
                    float m0 = tangents[0];
                    float m1 = tangents[1];
                    return CubicHermite(a, b, m0, m1, t, fade);
                }

                case InterpolationKind.CatmullRom:
                {
                    if (neighbors == null || neighbors.Length < 4)
                        throw new System.ArgumentException("CatmullRom requires neighbors[4]: p0,p1,p2,p3");
                    return CatmullRom(neighbors[0], neighbors[1], neighbors[2], neighbors[3], t, fade, catmullTau);
                }

                default:
                    return Lerp(a, b, t, fade);
            }
        }

        // ========== B I / T R I  L E R P ==========

        /// <summary>Двумерная билинейная интерполяция без fade.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Bilerp(float v00, float v10, float v01, float v11, float tx, float ty)
        {
            float a = Lerp(v00, v10, tx);
            float b = Lerp(v01, v11, tx);
            return Lerp(a, b, ty);
        }

        /// <summary>Билинейная интерполяция с fade (одна кривая на обе оси).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Bilerp(float v00, float v10, float v01, float v11, float tx, float ty, FadeCurveType fade)
        {
            tx = ApplyFade(tx, fade);
            ty = ApplyFade(ty, fade);
            float a = Lerp(v00, v10, tx);
            float b = Lerp(v01, v11, tx);
            return Lerp(a, b, ty);
        }

        /// <summary>Билинейная интерполяция с независимыми fade по осям.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Bilerp(float v00, float v10, float v01, float v11, float tx, float ty, FadeCurveType fadeX,
            FadeCurveType fadeY)
        {
            tx = ApplyFade(tx, fadeX);
            ty = ApplyFade(ty, fadeY);
            float a = Lerp(v00, v10, tx);
            float b = Lerp(v01, v11, tx);
            return Lerp(a, b, ty);
        }

        /// <summary>Трёхмерная трилинейная интерполяция без fade.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Trilerp(
            float v000, float v100, float v010, float v110,
            float v001, float v101, float v011, float v111,
            float tx, float ty, float tz)
        {
            float x00 = Lerp(v000, v100, tx);
            float x10 = Lerp(v010, v110, tx);
            float x01 = Lerp(v001, v101, tx);
            float x11 = Lerp(v011, v111, tx);

            float y0 = Lerp(x00, x10, ty);
            float y1 = Lerp(x01, x11, ty);
            return Lerp(y0, y1, tz);
        }

        /// <summary>Трилинейная интерполяция с одной общей fade-кривой.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Trilerp(
            float v000, float v100, float v010, float v110,
            float v001, float v101, float v011, float v111,
            float tx, float ty, float tz,
            FadeCurveType fade)
        {
            tx = ApplyFade(tx, fade);
            ty = ApplyFade(ty, fade);
            tz = ApplyFade(tz, fade);
            return Trilerp(v000, v100, v010, v110, v001, v101, v011, v111, tx, ty, tz);
        }

        /// <summary>Трилинейная интерполяция с независимыми fade по осям.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Trilerp(
            float v000, float v100, float v010, float v110,
            float v001, float v101, float v011, float v111,
            float tx, float ty, float tz,
            FadeCurveType fadeX, FadeCurveType fadeY, FadeCurveType fadeZ)
        {
            tx = ApplyFade(tx, fadeX);
            ty = ApplyFade(ty, fadeY);
            tz = ApplyFade(tz, fadeZ);
            return Trilerp(v000, v100, v010, v110, v001, v101, v011, v111, tx, ty, tz);
        }

        // ========== H E L P E R S ==========

        /// <summary>Вычислить Catmull-Rom для массива точек по нормализованному параметру u∈[0,1] на всём диапазоне.</summary>
        public static float CatmullRomSeries(float[] points, float u, float tau = 0.5f)
        {
            int n = points?.Length ?? 0;
            if (n < 2) throw new System.ArgumentException("CatmullRomSeries requires ≥2 points");
            u = math.clamp(u, 0f, 1f);

            if (n == 2)
            {
                // Деградация к линейной между [0] и [1]
                if (points != null) return Lerp(points[0], points[1], u);
            }

            // Сопоставляем u от 0..1 к "какому сегменту" принадлежит выборка
            float t = u * (n - 1);
            int i = math.min((int)math.floor(t), n - 2); // индекс сегмента [i..i+1]
            float localT = t - i;

            float p0 = points[math.max(i - 1, 0)];
            float p1 = points[i];
            float p2 = points[i + 1];
            float p3 = points[math.min(i + 2, n - 1)];

            return CatmullRom(p0, p1, p2, p3, localT, tau);
        }
    }
}