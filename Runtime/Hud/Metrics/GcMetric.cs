using System;

namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    /// <summary>
    /// Метрика по GC: показывает сколько сборок произошло за последний апдейт
    /// для каждого поколения (Gen0/Gen1/Gen2) и общее накопленное.
    /// </summary>
    public sealed class GcMetric : IMetricsProvider
    {
        public string Name => "GC";
        public bool Enabled { get; set; } = true;

        private int _prev0, _prev1, _prev2;
        private int _delta0, _delta1, _delta2;

        private int _total0, _total1, _total2;

        public void Update(float dt)
        {
            int g0 = GC.CollectionCount(0);
            int g1 = GC.CollectionCount(1);
            int g2 = GC.CollectionCount(2);

            _delta0 = g0 - _prev0;
            _delta1 = g1 - _prev1;
            _delta2 = g2 - _prev2;

            _total0 = g0;
            _total1 = g1;
            _total2 = g2;

            _prev0 = g0;
            _prev1 = g1;
            _prev2 = g2;
        }

        public void Emit(IStringBuilderTarget sb)
        {
            // Пример вывода: GC Δ: 1/0/0 | Tot: 372/371/371
            sb.Append("GC Δ: ");
            sb.Append(_delta0.ToString());
            sb.Append("/");
            sb.Append(_delta1.ToString());
            sb.Append("/");
            sb.Append(_delta2.ToString());
            sb.Append(" | Tot: ");
            sb.Append(_total0.ToString());
            sb.Append("/");
            sb.Append(_total1.ToString());
            sb.Append("/");
            sb.Append(_total2.ToString());
        }
    }
}