namespace Rusleo.Utils.Runtime.Hud.Metrics
{
    public sealed class GcMetric : IMetricsProvider
    {
        public string Name => "GC";
        public bool Enabled { get; set; } = true;

        private int _gen0, _gen1, _gen2;

        public void Update(float dt)
        {
            _gen0 = System.GC.CollectionCount(0);
            _gen1 = System.GC.CollectionCount(1);
            _gen2 = System.GC.CollectionCount(2);
        }

        public void Emit(IStringBuilderTarget sb)
        {
            sb.Append("GC: ");
            sb.Append(_gen0.ToString());
            sb.Append("/");
            sb.Append(_gen1.ToString());
            sb.Append("/");
            sb.Append(_gen2.ToString());
        }
    }
}