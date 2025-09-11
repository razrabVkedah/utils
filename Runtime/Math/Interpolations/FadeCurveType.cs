namespace Rusleo.Utils.Runtime.Math.Interpolations
{
    /// <summary>
    /// Типы кривых сглаживания параметра t ∈ [0,1] перед интерполяцией.
    /// </summary>
    public enum FadeCurveType
    {
        /// <summary>
        /// t
        /// </summary>
        Linear,

        /// <summary>
        /// 3t^2 - 2t^3  (C1)
        /// </summary>
        Smoothstep,

        /// <summary>
        /// 6t^5 - 15t^4 + 10t^3 (C2, "Perlin quintic")
        /// </summary>
        Smootherstep,

        /// <summary>
        /// 0.5 - 0.5 * cos(πt)
        /// </summary>
        Cosine
    }
}