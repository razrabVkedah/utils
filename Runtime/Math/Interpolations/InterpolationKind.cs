namespace Rusleo.Utils.Runtime.Math.Interpolations
{
    /// <summary>
    /// Типы интерполяции между скалярными значениями.
    /// </summary>
    public enum InterpolationKind
    {
        /// <summary>
        /// линейная
        /// </summary>
        Lerp,

        /// <summary>
        /// требует касательные (m0, m1)
        /// </summary>
        CubicHermite,

        /// <summary>
        /// сплайн через точки, касательные вычисляются автоматически
        /// </summary>
        CatmullRom
    }
}